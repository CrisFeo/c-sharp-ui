using System;
using System.Collections.Generic;
using Rendering;

namespace Layout.Widgets {

public class FixedWidth : SingleChildWidget {

  int width;

  public FixedWidth(int width, BaseWidget child) {
    this.width = width;
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
    var w = LayoutUtils.Clamp(c.xMin, c.xMax, width);
    if (child == null) {
      Geometry = new Geometry {
        w = w,
        h = c.yMin,
      };
    } else {
      child.Position = (0, 0);
      Geometry = child.Layout(c with {
        xMin = w,
        xMax = w,
      });
    }
    return Geometry;
  }

}

  public static partial class Library {
    public static FixedWidth FixedWidth(int width, BaseWidget child) {
      return new FixedWidth(width, child);
    }
  }
}
