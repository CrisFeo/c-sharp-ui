using System;
using System.Collections.Generic;
using Rendering;
using Layout;

using W = Layout.Widgets.Library;

public static partial class App {

  public static void Widget<State, Event>(
    Func<State> init,
    Func<Terminal, Sub<Event>> subs,
    Func<State, Event, (State, Cmd<Event>)> step,
    Func<State, BaseWidget> view,
    int width,
    int height,
    string title
  ) {
    var prev = default(BaseWidget);
    var next = default(BaseWidget);
    Terminal(
      init,
      subs,
      step,
      (t, s) => {
        prev = next;
        next = W.BackgroundColor(Colors.Black,
          W.ForegroundColor(Colors.White,
            view(s)
          )
        );
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
      width,
      height,
      title
    );
  }

}
