using System;
using System.Collections.Generic;
using Rendering;

namespace Layout.Widgets {

public class ForegroundColor : SingleChildWidget {

  Color color;

  public ForegroundColor(Color color, BaseWidget child) {
    StateHash = (color).GetHashCode();
    this.color = color;
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
    Foreground = color;
    return base.Layout(c);
  }

}

public static partial class Library {
  public static ForegroundColor ForegroundColor(Color color, BaseWidget child = null) {
    return new ForegroundColor(color, child);
  }
}

}

