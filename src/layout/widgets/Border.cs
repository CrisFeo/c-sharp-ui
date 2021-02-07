using System;
using System.Collections.Generic;
using Rendering;

namespace Layout.Widgets {

public class Border : SingleChildWidget {

  public Border(BaseWidget child) {
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
    PassInheritedProperties();
    if (child == null) {
      Geometry = new Geometry {
        w = c.xMin,
        h = c.yMin,
      };
    } else {
      child.Position = (1, 1);
      var g = child.Layout(c with {
        xMax = c.xMax - 2,
        yMax = c.yMax - 2,
      });
      Geometry = new Geometry {
        w = g.w + 2,
        h = g.h + 2,
      };
    }
    return Geometry;
  }

  public override void Render(Terminal t, int x, int y) {
    Drawing.Box(t, x, y, Geometry.w, Geometry.h, Foreground, Background);
  }

}

public static partial class Library {
  public static Border Border(BaseWidget child) {
    return new Border(child);
  }
}

}
