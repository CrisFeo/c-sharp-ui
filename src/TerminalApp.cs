using System;
using System.Collections.Generic;
using Rendering;

public static partial class App {

  public static void Terminal<State, Event>(
    Func<State> init,
    Func<Terminal, Sub<Event>> subs,
    Func<State, Event, (State, Cmd<Event>)> step,
    Action<Terminal, State> view,
    int width,
    int height,
    string title
  ) {
    using (var terminal = new Rendering.Terminal(width, height, title))
    using (var store = new Store<State, Event>(
      init(),
      subs(terminal),
      step,
      s => view(terminal, s)
    )) {
      store.Start();
      var size = terminal.Size;
      while (!store.ShouldQuit && !terminal.ShouldClose) {
        var newSize = terminal.Size;
        if (newSize != size) {
          size = newSize;
          store.ForceRedraw();
        }
        store.Process();
        terminal.Poll();
      }
    }
  }

}

public static partial class Sub {

  public static Sub<E> KeyDown<E>(Terminal t, Func<Key, E> map) {
    var onKeyDown = default(Action<Key>);
    return new Sub<E>(
      dispatch => {
        onKeyDown = k => dispatch(map(k));
        t.OnKeyDown += onKeyDown;
      },
      () => t.OnKeyDown -= onKeyDown
    );
  }

  public static Sub<E> MouseMove<E>(Terminal t, Func<int, int, E> map) {
    var onMouseMove = default(Action<int, int>);
    return new Sub<E>(
      dispatch => {
        onMouseMove = (x, y) => dispatch(map(x, y));
        t.OnMouseMove += onMouseMove;
      },
      () => t.OnMouseMove -= onMouseMove
    );
  }

}
