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

  public static void Play(Config config) {
    using (var t = new Rendering.Terminal(60, 60, "minesweeper")) {
      var initialState = new State {
        random = new Random.State(12),
        tick = 0,
        isPlaying = false,
        cells = new Lst<Cell>(config.size * config.size).Map(c => new Cell{}),
        x = 0,
        y = 0,
      };
      var store = new Store<State, Event>(
        (s, e) => Step(config, s, e),
        initialState
      );
      store.Subscribe(() => {
        t.Clear();
        Draw(t, config, store.GetState());
        t.Render();
      });
      var nextTick = Time.Now() + TICK_INTERVAL;
      var isRunning = true;
      while (isRunning && !t.ShouldClose) {
        if (Time.Now() >= nextTick) {
          store.Dispatch(new Event.Tick());
          nextTick = Time.Now() + TICK_INTERVAL;
        }
        isRunning = Input(t, store.Dispatch);
        store.Process();
        t.Poll();
      }
    }
  }

  // Structs
  ////////////////////

  public record Config {
    public int size         {get; init; }
    public float mineChance {get; init; }
  }

  record State {
    public Random.State random { get; init; }
    public int tick            { get; init; }
    public bool isPlaying      { get; init; }
    public Lst<Cell> cells     { get; init; }
    public int x               { get; init; }
    public int y               { get; init; }
  }

  record Cell {
    public int count       { get; init; }
    public bool isMine     { get; init; }
    public bool isRevealed { get; init; }
    public bool isFlagged  { get; init; }
  }

  class Event {
    public class Tick    : Event { }
    public class NewGame : Event { }
    public class Move    : Event { public int x; public int y; }
    public class Check   : Event { }
    public class Flag    : Event { }
  }

  // Internal methods
  ////////////////////

  static bool Input(Terminal t, Action<Event> dispatch) {
    Console.WriteLine(t.KeyDown(Key.M));
    if (t.KeyDown(Key.H)) dispatch(new Event.Move{ x = -1, y =  0 });
    if (t.KeyDown(Key.J)) dispatch(new Event.Move{ x =  0, y =  1 });
    if (t.KeyDown(Key.K)) dispatch(new Event.Move{ x =  0, y = -1 });
    if (t.KeyDown(Key.L)) dispatch(new Event.Move{ x =  1, y =  0 });
    if (t.KeyDown(Key.Z)) dispatch(new Event.Check());
    if (t.KeyDown(Key.X)) dispatch(new Event.Flag());
    if (t.KeyDown(Key.S)) dispatch(new Event.NewGame());
    if (t.KeyDown(Key.Q)) return false;
    return true;
  }

  static State Step(Config config, Event evt, State state) {
    switch(evt) {
      case Event.Tick e: {
        state = state with { tick = state.tick + 1 };
        break;
      }
      case Event.NewGame e: {
        if (state.isPlaying) break;
        var random = state.random;
        var cells = new List<Cell>(config.size * config.size);
        for (var i = 0; i < config.size * config.size; i++) {
          cells.Add(new Cell {
            isMine = random.Next(out random) < config.mineChance,
            isRevealed = false,
            isFlagged = false,
          });
        }
        for (var i = 0; i < config.size * config.size; i++) {
          var count = 0;
          foreach (var ni in Neighbors(config.size, i)) {
            if (cells[ni].isMine) count++;
          }
          cells[i] = cells[i] with { count = count };
        }
        state = state with {
          random = random,
          isPlaying = true,
          cells = Lst<Cell>.Empty.AddRange(cells),
          x = (int)(config.size / 2),
          y = (int)(config.size / 2),
        };
        break;
      }
      case Event.Move e: {
        if (!state.isPlaying) break;
        state = state with {
          x = Clamp(0, config.size - 1, state.x + e.x),
          y = Clamp(0, config.size - 1, state.y + e.y),
        };
        break;
      }
      case Event.Check e: {
        if (!state.isPlaying) break;
        var cursorIndex = state.x + state.y * config.size;
        var cell = state.cells[cursorIndex];
        if (!cell.isFlagged) {
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
                foreach (var ni in Neighbors(config.size, ci)) {
                  if (!visited.Contains(ni)) front.Add(ni);
                }
              }
            }
          }
          state = state with {
            cells = new Lst<Cell>(cells),
            isPlaying = !cells.All(c => c.isMine || c.isRevealed),
          };
        }
        break;
      }
      case Event.Flag e: {
        if (!state.isPlaying) break;
        var i = state.x + state.y * config.size;
        var c = state.cells[i];
        state = state with {
          cells = state.cells.Set(i, c with { isFlagged = !c.isFlagged})
        };
        break;
      }
    }
    return state;
  }

  static void Draw(Terminal t, Config config, State state) {
    var isToggleFrame = state.tick % 2 == 0;
    for (var y = 0; y < config.size; y++) {
      for (var x = 0; x < config.size; x++) {
        var cell = state.cells[x + y * config.size];
        if (state.isPlaying && isToggleFrame && state.x == x && state.y == y) {
          t.Set(x, y, 'x');
        } else {
          var (c, fg, bg) = RenderCell(cell);
          t.Set(x, y, c, fg, bg);
        }
      }
    }
    t.Set(0, config.size, state.isPlaying ? "playing" : "game over");
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
