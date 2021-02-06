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

  public static void Run() {
    App.Widget(
      init: Init,
      subs: Subs,
      step: Step,
      view: View,
      width: 30,
      height: 50,
      title: "Todo List"
    );
  }

  // Records
  ////////////////////

  record State {
    public Lst<Entry> entries  { get; init; }
    public int        selected { get; init; }
  }

  record Entry {
    public bool   completed   { get; init; }
    public string description { get; init; }
  }

  record Event {
    public record Quit()      : Event;
    public record Move(int o) : Event;
    public record Toggle()    : Event;
  }

  // Internal methods
  ////////////////////

   static State Init() {
    return new State {
      entries = Lst<Entry>.Empty.AddRange(new Entry[] {
        new Entry { completed = true, description = "get milk" },
        new Entry { completed = false, description = "workout (10a)" },
        new Entry { completed = false, description = "business meeting" },
      }),
      selected = 0,
    };
  }

  static Sub<Event> Subs(Terminal t) {
    return Sub.KeyDown(t, OnKeyDown);
  }

  static Event OnKeyDown(Key k) {
    switch (k) {
      case Key.Q:     return new Event.Quit();
      case Key.J:     return new Event.Move(1);
      case Key.K:     return new Event.Move(-1);
      case Key.Enter: return new Event.Toggle();
    }
    return null;
  }

  static (State, Cmd<Event>) Step(State state, Event evt) {
    switch(evt) {
      case Event.Quit e: {
        return (state, Cmd.Quit<Event>());
      }
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
    return Center(
      W.Column(
        state.entries.MapArr(e =>
          TodoEntry(e, state.selected == i++)
        )
      )
    );
  }

  static BaseWidget TodoEntry(Entry e, bool selected) =>
    W.Row(
      W.Text(selected ? ">" : " "),
      W.FixedWidth(1, null),
      W.Text(e.completed ? "[x]" : "[ ]"),
      W.FixedWidth(1, null),
      W.Text(e.description)
    );

  static BaseWidget Center(BaseWidget child) =>
    CenterHorizontal(
      CenterVertical(
        child
      )
    );

  static BaseWidget CenterHorizontal(BaseWidget child) =>
    W.Row(
      W.FillWidth(null),
      child,
      W.FillWidth(null)
    );

  static BaseWidget CenterVertical(BaseWidget child) =>
    W.Column(
      W.FillHeight(null),
      child,
      W.FillHeight(null)
    );

  static U[] MapArr<T, U>(this Lst<T> l, Func<T, U> fn) => l.Map(fn).ToArray();

  static int Clamp(int min, int max, int val) {
    if (val < min) return min;
    if (val > max) return max;
    return val;
  }

}

