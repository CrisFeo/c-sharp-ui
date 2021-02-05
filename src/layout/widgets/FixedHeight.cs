using System;
using System.Collections.Generic;
using Rendering;

namespace Layout.Widgets {

public class FixedHeight : SingleChildWidget {

  int height;

  public FixedHeight(int height, BaseWidget child) {
    this.height = height;
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
    var h = LayoutUtils.Clamp(c.yMin, c.yMax, height);
    if (child == null) {
      Geometry = new Geometry {
        w = c.xMin,
        h = h,
      };
    } else {
      child.Position = (0, 0);
      Geometry = child.Layout(c with {
        yMin = h,
        yMax = h,
      });
    }
    return Geometry;
  }

}

public static partial class Library {
  public static FixedHeight FixedHeight(int height, BaseWidget child) {
    return new FixedHeight(height, child);
  }
}

}
