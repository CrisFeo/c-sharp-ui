using System;
using System.Collections.Generic;
using Rendering;

namespace Layout.Widgets {

public class Pane : SingleChildWidget {

  public Pane(BaseWidget child) {
    this.child = child;
  }

  public override void Render(Terminal t, int x, int y) {
    Drawing.Fill(t, x, y, Geometry.w, Geometry.h, Foreground, Background);
  }

}

public static partial class Library {
  public static Pane Pane(BaseWidget child) {
    return new Pane(child);
  }
}

}

