using System;
using System.Collections.Generic;
using Rendering;
using Layout;

public static partial class App {

  public static void Widget<State, Event>(
    State init,
    Func<Terminal, Action<Event>, bool> input,
    Func<State, Event, (State, Cmd<Event>)> step,
    Func<State, BaseWidget> view,
    Sub<Event> subs,
    int width,
    int height,
    string title
  ) {
    var prev = default(BaseWidget);
    var next = default(BaseWidget);
    Terminal(
      init,
      input,
      step,
      (t, s) => {
        prev = next;
        next = view(s);
        var (width, height) = t.Size;
        next.Layout(new Constraint {
          xMin = 0,
          xMax = width,
          yMin = 0,
          yMax = height,
        });
        if (prev == null) {
          LayoutUtils.PrintTree(next);
          RenderLayout.Tree(t, next);
        } else {
          RenderLayout.Diff(t, prev, next);
        }
        t.Render();
      },
      subs,
      width,
      height,
      title
    );
  }

}
