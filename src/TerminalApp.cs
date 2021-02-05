using System;
using System.Collections.Generic;
using Rendering;

public static partial class App {

  public static void Terminal<State, Event>(
    State init,
    Func<Terminal, Action<Event>, bool> input,
    Func<State, Event, (State, Cmd<Event>)> step,
    Action<Terminal, State> view,
    Sub<Event> subs,
    int width,
    int height,
    string title
  ) {
    using (var terminal = new Rendering.Terminal(width, height, title))
    using (var store = new Store<State, Event>(
      init,
      step,
      s => view(terminal, s),
      subs
    )) {
      store.Start();
      var isRunning = true;
      var size = terminal.Size;
      while (isRunning && !terminal.ShouldClose) {
        var newSize = terminal.Size;
        if (newSize != size) {
          size = newSize;
          store.ForceRedraw();
        }
        isRunning = input(terminal, store.Dispatch);
        store.Process();
        terminal.Poll();
      }
    }
  }

}
