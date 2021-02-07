using System;
using System.Collections.Generic;
using Rendering;

namespace Layout.Widgets {

public class FillWidth : SingleChildWidget {

  public FillWidth(BaseWidget child) {
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
    PassInheritedProperties();
    if (child == null) {
      Geometry = new Geometry {
        w = c.xMax,
        h = c.yMin,
      };
    } else {
      child.Position = (0, 0);
      Geometry = child.Layout(c with {
        xMin = c.xMax,
      });
    }
    return Geometry;
  }

}

public static partial class Library {
  public static FillWidth FillWidth(BaseWidget child = null) {
    return new FillWidth(child);
  }
}

}
