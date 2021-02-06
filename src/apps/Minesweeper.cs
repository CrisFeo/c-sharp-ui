using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Rendering;

public static class Minesweeper {

  // Constants
  ////////////////////

  const float TICK_INTERVAL = 0.3f;

  // Public methods
  ////////////////////

  public static void Run(Config config) {
    App.Terminal(
      init: () => Init(config),
      subs: Subs,
      step: Step,
      view: View,
      width: 60,
      height: 60,
      title: "minesweeper"
    );
  }

  // Records
  ////////////////////

  public record Config {
    public int   size       {get; init; }
    public float mineChance {get; init; }
  }

  record State {
    public Random.State random    { get; init; }
    public Config       config    { get; init; }
    public int          tick      { get; init; }
    public bool         isPlaying { get; init; }
    public Lst<Cell>    cells     { get; init; }
    public int          x         { get; init; }
    public int          y         { get; init; }
    public float        time      { get; init; }
  }

  record Cell {
    public int  count      { get; init; }
    public bool isMine     { get; init; }
    public bool isRevealed { get; init; }
    public bool isFlagged  { get; init; }
  }

  record Event {
    public record Time(float time)   : Event;
    public record Tick()             : Event;
    public record Quit()             : Event;
    public record NewGame()          : Event;
    public record Move(int x, int y) : Event;
    public record Check()            : Event;
    public record Flag()             : Event;
  }

  // Internal methods
  ////////////////////

  static State Init(Config config) {
    return new State {
      random = new Random.State(12),
      config = config,
      tick = 0,
      isPlaying = false,
      cells = new Lst<Cell>(config.size * config.size).Map(c => new Cell()),
      x = 0,
      y = 0,
    };
  }

  static Sub<Event> Subs(Terminal t) {
    return Sub.Many(
      Sub.Every<Event>(TICK_INTERVAL, () => new Event.Tick()),
      Sub.KeyDown(t, OnKeyDown)
    );
  }

  static Event OnKeyDown(Key k) {
    switch (k) {
      case Key.Q: return new Event.Quit();
      case Key.S: return new Event.NewGame();
      case Key.H: return new Event.Move(-1,  0);
      case Key.J: return new Event.Move( 0,  1);
      case Key.K: return new Event.Move( 0, -1);
      case Key.L: return new Event.Move( 1,  0);
      case Key.Z: return new Event.Check();
      case Key.X: return new Event.Flag();
    }
    return null;
  }

  static (State, Cmd<Event>) Step(State state, Event evt) {
    switch(evt) {
      case Event.Quit e: {
        return (state, Cmd.Quit<Event>());
      }
      case Event.Tick e: {
        return (state with { tick = state.tick + 1 }, null);
      }
      case Event.Time e: {
        return (state with { time = e.time }, null);
      }
      case Event.NewGame e: {
        if (state.isPlaying) break;
        var random = state.random;
        var cells = new List<Cell>(state.config.size * state.config.size);
        for (var i = 0; i < state.config.size * state.config.size; i++) {
          cells.Add(new Cell {
            isMine = random.Next(out random) < state.config.mineChance,
            isRevealed = false,
            isFlagged = false,
          });
        }
        for (var i = 0; i < state.config.size * state.config.size; i++) {
          var count = 0;
          foreach (var ni in Neighbors(state.config.size, i)) {
            if (cells[ni].isMine) count++;
          }
          cells[i] = cells[i] with { count = count };
        }
        return (state with {
          random = random,
          isPlaying = true,
          cells = Lst<Cell>.Empty.AddRange(cells),
          x = (int)(state.config.size / 2),
          y = (int)(state.config.size / 2),
        }, null);
      }
      case Event.Move e: {
        if (!state.isPlaying) break;
        return (state with {
          x = Clamp(0, state.config.size - 1, state.x + e.x),
          y = Clamp(0, state.config.size - 1, state.y + e.y),
        }, null);
      }
      case Event.Check e: {
        if (!state.isPlaying) break;
        var cursorIndex = state.x + state.y * state.config.size;
        var cell = state.cells[cursorIndex];
        if (cell.isFlagged) break;
        var cells = state.cells.ToBuilder();
        if (cell.isMine) {
          for (var i = 0; i < cells.Count; i++) {
            cells[i] = cells[i] with { isRevealed = true };
          }
        } else {
          var front = new HashSet<int>();
          var visited = new HashSet<int>();
          front.Add(cursorIndex);
          while (front.Count != 0) {
            var ci = front.First();
            front.Remove(ci);
            visited.Add(ci);
            var c = cells[ci] with { isRevealed = true };
            cells[ci] = c;
            if (c.count == 0) {
              foreach (var ni in Neighbors(state.config.size, ci)) {
                if (!visited.Contains(ni)) front.Add(ni);
              }
            }
          }
        }
        return (state with {
          cells = new Lst<Cell>(cells),
          isPlaying = !cells.All(c => c.isMine || c.isRevealed),
        }, null);
      }
      case Event.Flag e: {
        if (!state.isPlaying) break;
        var i = state.x + state.y * state.config.size;
        var c = state.cells[i];
        return (state with {
          cells = state.cells.Set(i, c with { isFlagged = !c.isFlagged})
        }, null);
      }
    }
    return (state, null);
  }

  static void View(Terminal t, State state) {
    t.Clear();
    var isToggleFrame = state.tick % 2 == 0;
    for (var y = 0; y < state.config.size; y++) {
      for (var x = 0; x < state.config.size; x++) {
        var cell = state.cells[x + y * state.config.size];
        if (state.isPlaying && isToggleFrame && state.x == x && state.y == y) {
          t.Set(x, y, 'x');
        } else {
          var (c, fg, bg) = RenderCell(cell);
          t.Set(x, y, c, fg, bg);
        }
      }
    }
    t.Set(0, state.config.size, state.isPlaying ? "playing" : "game over");
    t.Set(0, state.config.size + 1, state.time.ToString());
    t.Render();
  }

  static (char, Color, Color) RenderCell(Cell c) {
    var fg = Colors.White;
    var bg = Colors.Black;
    if (c.isFlagged) return ('F', fg, bg);
    if (c.isRevealed) {
      if (c.isMine) return ('*', fg, bg);
      if (c.count == 0) return (' ', fg, bg);
      switch (c.count) {
        case 1:  fg = Colors.Blue;        break;
        case 2:  fg = Colors.Green;       break;
        case 3:  fg = Colors.Red;         break;
        case 4:  fg = Colors.Magenta;     break;
        case 5:  fg = Colors.DarkMagenta; break;
        case 6:  fg = Colors.Cyan;        break;
        case 7:  fg = Colors.Yellow;      break;
        default: fg = Colors.Gray;        break;
      }
      var ch = (char)(c.count + 48);
      return (ch, fg, bg);
    }
    return ('.', fg, bg);
  }

  static List<int> Neighbors(int size, int i) {
    var y = (int)(i / size);
    var x = (i - y * size) % size;
    var notMinX = x != 0;
    var notMaxX = x != size - 1;
    var notMinY = y != 0;
    var notMaxY = y != size - 1;
    List<int> ns = new List<int>(8);
    if (notMinY           ) ns.Add((x    ) + (y - 1) * size);
    if (notMaxY           ) ns.Add((x    ) + (y + 1) * size);
    if (notMinX           ) ns.Add((x - 1) + (y    ) * size);
    if (notMaxX           ) ns.Add((x + 1) + (y    ) * size);
    if (notMinX && notMaxY) ns.Add((x - 1) + (y + 1) * size);
    if (notMaxX && notMaxY) ns.Add((x + 1) + (y + 1) * size);
    if (notMinX && notMinY) ns.Add((x - 1) + (y - 1) * size);
    if (notMaxX && notMinY) ns.Add((x + 1) + (y - 1) * size);
    return ns;
  }

  static int Clamp(int min, int max, int val) {
    if (val < min) return min;
    if (val > max) return max;
    return val;
  }

}
