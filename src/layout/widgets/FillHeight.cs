using System;
using System.Collections.Generic;
using Rendering;

namespace Layout.Widgets {

public class FillHeight : SingleChildWidget {

  public FillHeight(BaseWidget child) {
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
    PassInheritedProperties();
    if (child == null) {
      Geometry = new Geometry {
        w = c.xMin,
        h = c.yMax,
      };
    } else {
      child.Position = (0, 0);
      Geometry = child.Layout(c with {
        yMin = c.yMax,
      });
    }
    return Geometry;
  }

}

public static partial class Library {
  public static FillHeight FillHeight(BaseWidget child = null) {
    return new FillHeight(child);
  }
}

}
