using System;
using System.Collections.Generic;
using Rendering;

namespace Layout {

public abstract class SingleChildWidget : BaseWidget {

  protected BaseWidget child;

  public override IEnumerable<BaseWidget> Visit() {
    if (child == null) yield break;
    yield return child;
  }

  public override Geometry Layout(Constraint c) {
    PassInheritedProperties();
    if (child == null) {
      Geometry = new Geometry {
        w = c.xMin,
        h = c.yMin,
      };
    } else {
      child.Position = (0, 0);
      Geometry = child.Layout(c);
    }
    return Geometry;
  }

}

}
