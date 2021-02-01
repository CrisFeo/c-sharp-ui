using System;

using W = Widgets;

static class LayoutTest {
  public static void Run() {
    using (var t = new Rendering.Terminal(50, 50, "layout test")) {
      var c = W.Row(
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
      var width = -1;
      var height = -1;
      while (!t.ShouldClose) {
        var (newWidth, newHeight) = t.Size;
        if (width != newWidth || height != newHeight) {
          width = newWidth;
          height = newHeight;
          c.Layout(new Constraint {
            xMin = 0,
            xMax = width,
            yMin = 0,
            yMax = height,
          });
          t.Clear();
          c.Render(t, 0, 0);
          t.Render();
        }
        t.Poll();
      }
    }
  }
}

