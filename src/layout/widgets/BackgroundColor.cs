using System;
using System.Collections.Generic;
using Rendering;

namespace Layout.Widgets {

public class BackgroundColor : SingleChildWidget {

  Color color;

  public BackgroundColor(Color color, BaseWidget child) {
    StateHash = (color).GetHashCode();
    this.color = color;
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
    Background = color;
    return base.Layout(c);
  }

}

public static partial class Library {
  public static BackgroundColor BackgroundColor(Color color, BaseWidget child = null) {
    return new BackgroundColor(color, child);
  }
}

}
