using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

public static class Minesweeper {

  // Constants
  ////////////////////

  const float TICK_INTERVAL = 0.3f;

  // Public methods
  ////////////////////

  public static void Play(int size, float mineChance) {
    using (Terminal.Initialize()) {
      var config = new Config {
        size = size,
        mineChance = mineChance,
      };
      var initialState = new State {
        random = Random.New(12),
        tick = 0,
        isPlaying = false,
        cells = new Cell[config.size * config.size].ToImmutableList(),
        x = 0,
        y = 0,
      };
      var store = new Store<State, Event>(
        (s, e) => Step(config, s, e),
        initialState
      );
      var sb = new StringBuilder();
      store.Subscribe(() => {
        sb.Clear();
        Draw(sb, config, store.GetState());
        Terminal.Draw(sb.ToString());
      });
      var nextTick = Time.Now() + TICK_INTERVAL;
      var isRunning = true;
      while(isRunning) {
        if (Terminal.ReadKey(out var key)) {
          switch(key.Key) {
            case ConsoleKey.H: store.Dispatch(new Event.Move{ x = -1, y =  0 }); break;
            case ConsoleKey.J: store.Dispatch(new Event.Move{ x =  0, y =  1 }); break;
            case ConsoleKey.K: store.Dispatch(new Event.Move{ x =  0, y = -1 }); break;
            case ConsoleKey.L: store.Dispatch(new Event.Move{ x =  1, y =  0 }); break;
            case ConsoleKey.Z: store.Dispatch(new Event.Check()); break;
            case ConsoleKey.X: store.Dispatch(new Event.Flag()); break;
            case ConsoleKey.S: store.Dispatch(new Event.NewGame()); break;
            case ConsoleKey.Q: isRunning = false; break;
          }
        }
        if (Time.Now() >= nextTick) {
          store.Dispatch(new Event.Tick());
          nextTick = Time.Now() + TICK_INTERVAL;
        }
        store.Process();
      }
    }
  }

  // Structs
  ////////////////////

  struct Config {
    public int size;
    public float mineChance;
  }

  struct State {
    public Random.State random;
    public int tick;
    public bool isPlaying;
    public ImmutableList<Cell> cells;
    public int x;
    public int y;
  }

  struct Cell {
    public int count;
    public bool isMine;
    public bool isRevealed;
    public bool isFlagged;
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

  static State Step(Config config, Event evt, State state) {
    switch(evt) {
      case Event.Tick e: {
        state.tick++;
        break;
      }
      case Event.NewGame e: {
        var cells = new List<Cell>(config.size * config.size);
        for (var i = 0; i < config.size * config.size; i++) {
          cells.Add(new Cell {
            isMine = state.random.Next(out state.random) < config.mineChance,
            isRevealed = false,
            isFlagged = false,
          });
        }
        for (var i = 0; i < config.size * config.size; i++) {
          var c = cells[i];
          foreach (var ni in Neighbors(config.size, i)) {
            if (cells[ni].isMine) c.count++;
          }
          cells[i] = c;
        }
        state.isPlaying = true;
        state.cells = cells.ToImmutableList();
        state.x = (int)(config.size / 2);
        state.y = (int)(config.size / 2);
        break;
      }
      case Event.Move e: {
        if (!state.isPlaying) break;
        state.x = Clamp(0, config.size - 1, state.x + e.x);
        state.y = Clamp(0, config.size - 1, state.y + e.y);
        break;
      }
      case Event.Check e: {
        if (!state.isPlaying) break;
        var i = state.x + state.y * config.size;
        var cell = state.cells[i];
        if (!cell.isFlagged) {
          if (cell.isMine) {
            state.cells = state.cells.ConvertAll(c => {
              c.isRevealed = true;
              return c;
            });
          } else {
            var cells = state.cells.ToBuilder();
            var front = new HashSet<int>();
            var visited = new HashSet<int>();
            front.Add(i);
            while (front.Count != 0) {
              var ci = front.First();
              front.Remove(ci);
              visited.Add(ci);
              var c = cells[ci];
              c.isRevealed = true;
              cells[ci] = c;
              if (c.count == 0) {
                foreach (var ni in Neighbors(config.size, ci)) {
                  if (!visited.Contains(ni)) front.Add(ni);
                }
              }
            }
            state.cells = cells.ToImmutable();
          }
          if (state.cells.All(c => c.isMine || c.isRevealed)) {
            state.isPlaying = false;
          }
        }
        break;
      }
      case Event.Flag e: {
        if (!state.isPlaying) break;
        var i = state.x + state.y * config.size;
        var cell = state.cells[i];
        cell.isFlagged = !cell.isFlagged;
        state.cells = state.cells.SetItem(i, cell);
        break;
      }
    }
    return state;
  }

  static void Draw(StringBuilder sb, Config config, State state) {
    var isToggleFrame = state.tick % 2 == 0;
    for (var y = 0; y < config.size; y++) {
      for (var x = 0; x < config.size; x++) {
        var cell = state.cells[x + y * config.size];
        if (state.isPlaying && isToggleFrame && state.x == x && state.y == y) {
          sb.Append("x");
        } else {
          sb.Append(RenderCell(cell));
        }
      }
      sb.AppendLine();
    }
    sb.Append(state.isPlaying ? "playing" : "game over");
  }

  static char RenderCell(Cell c) {
    if (c.isFlagged) return 'F';
    if (c.isRevealed) {
      if (c.isMine) return '*';
      if (c.count == 0) return ' ';
      return (char)(c.count + 48);
    }
    return '.';
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
