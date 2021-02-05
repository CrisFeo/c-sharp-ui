using System;
using System.Collections.Generic;
using Rendering;

using W = Layout.Widgets.Library;

namespace Layout.Tests {

static class Runner {

  // Public methods
  ////////////////////

  public static void Run() {
    TestRenderDiff();
    //TestRenderResponsive();
  }

  // Internal methods
  ////////////////////

  static void TestRenderDiff() {
    var ar = W.Row(
      W.Text("hello"),
      W.FillWidth(null),
      W.Border(
        W.Text("world")
      )
    );
    var br = W.Row(
      W.Text("halo"),
      W.FillWidth(null),
      W.Border(
        W.Text("world")
      )
    );
    var c = new Constraint {
      xMin = 0,
      xMax = 50,
      yMin = 0,
      yMax = 50,
    };
    ar.Layout(c);
    br.Layout(c);
    using (var t = new Rendering.Terminal(50, 50, "layout test")) {
      RenderLayout.Tree(t, ar);
      RenderLayout.Diff(t, ar, br);
      t.Render();
      while (!t.ShouldClose) {
        t.Poll();
      }
    }
  }

  static void TestRenderResponsive() {
    Func<BaseWidget> build = () => W.Row(
      W.FillWidth(
        W.FillHeight(
          W.Border(null))),
      W.FixedWidth(
        20,
        W.Column(
          W.Row(
            W.Text("Blacksmith"),
            W.FillWidth(null),
            W.Text("friendly")
          ),
          W.Row(
            W.Text("Mouse"),
            W.FillWidth(null),
            W.Text("neutral")
          ),
          W.Row(
            W.Text("Fire Imp"),
            W.FillWidth(null),
            W.Text("hostile")
          )
        )
      )
    );
    using (var t = new Rendering.Terminal(50, 50, "layout test")) {
      var c = new Constraint {
        xMin = 0,
        xMax = -1,
        yMin = 0,
        yMax = -1,
      };
      var prev = default(BaseWidget);
      var next = build();
      while (!t.ShouldClose) {
        var (newWidth, newHeight) = t.Size;
        if (c.xMax != newWidth || c.yMax != newHeight) {
          c = c with {
            xMax = newWidth,
            yMax = newHeight,
          };
          next.Layout(c);
          if (prev == null) {
            LayoutUtils.PrintTree(next);
            RenderLayout.Tree(t, next);
          } else {
            RenderLayout.Diff(t, prev, next);
          }
          t.Render();
          prev = next;
          next = build();
        }
        t.Poll();
      }
    }
  }

}

}
