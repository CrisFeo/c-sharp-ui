using System;
using System.Collections.Generic;
using Rendering;

namespace Layout {

public abstract class BaseWidget {

  // Public properties
  ////////////////////

  public Geometry Geometry { get; protected set; }

  public (int, int) Position { get; set; }

  public Color Foreground { get; set; }

  public Color Background { get; set; }

  public int StateHash { get; protected set; }

  // Public methods
  ////////////////////

  public abstract IEnumerable<BaseWidget> Visit();

  public abstract Geometry Layout(Constraint c);

  public virtual void Render(Terminal t, int x, int y) { }

  // Internal methods
  ////////////////////

  protected void PassInheritedProperties() {
    foreach (var c in Visit()) {
      c.Foreground = Foreground;
      c.Background = Background;
    }
  }

}

}
