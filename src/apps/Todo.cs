using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Rendering;
using Layout;

using W = Layout.Widgets.Library;

public static class Todo {

  // Public methods
  ////////////////////

  public static void Run(Config config) {
    App.Widget(
      init: new State {
        entries = Lst<Entry>.Empty.AddRange(new Entry[] {
          new Entry { completed = true, description = "get milk" },
          new Entry { completed = false, description = "workout (10a)" },
          new Entry { completed = false, description = "business meeting" },
        }),
        selected = 0,
      },
      input: Input,
      step: Step,
      view: View,
      subs: default(Sub<Event>),
      width: 30,
      height: 50,
      title: "Todo List"
    );
  }

  // Records
  ////////////////////

  public record Config { }

  record State {
    public Lst<Entry> entries  { get; init; }
    public int        selected { get; init; }
  }

  record Entry {
    public bool   completed   { get; init; }
    public string description { get; init; }
  }

  record Event {
    public record Move(int o) : Event;
    public record Toggle()    : Event;
  }

  // Internal methods
  ////////////////////

  static bool Input(Terminal t, Action<Event> dispatch) {
    if (t.KeyDown(Key.Q))     return false;
    if (t.KeyDown(Key.J))     dispatch(new Event.Move(1));
    if (t.KeyDown(Key.K))     dispatch(new Event.Move(-1));
    if (t.KeyDown(Key.Enter)) dispatch(new Event.Toggle());
    return true;
  }

  static (State, Cmd<Event>) Step(State state, Event evt) {
    switch(evt) {
      case Event.Move e: {
        if (state.entries.Count == 0) return (state, null);
        return (state with {
          selected = Clamp(0, state.entries.Count - 1, state.selected + e.o),
        }, null);
      }
      case Event.Toggle e: {
        var entry = state.entries[state.selected];
        entry = entry with { completed = !entry.completed };
        return (state with {
          entries = state.entries.Set(state.selected, entry),
        }, null);
      }
    }
    return (state, null);
  }

  static BaseWidget View(State state) {
    var i = 0;
    return W.Row(
      W.FillWidth(null),
      W.Column(
        state.entries.Map(e => W.Row(
          W.Text(state.selected == i++ ? ">" : " "),
          W.FixedWidth(1, null),
          W.Text(e.completed ? "[x]" : "[ ]"),
          W.FixedWidth(1, null),
          W.Text(e.description)
        )).ToArray()
      ),
      W.FillWidth(null)
    );
  }

  static int Clamp(int min, int max, int val) {
    if (val < min) return min;
    if (val > max) return max;
    return val;
  }

}

