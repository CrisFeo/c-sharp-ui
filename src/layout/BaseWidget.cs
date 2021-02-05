using System;
using System.Collections.Generic;
using Rendering;

namespace Layout {

public abstract class BaseWidget {

  // Public properties
  ////////////////////

  public Geometry Geometry { get; protected set; }

  public (int, int) Position { get; set; }

  public int StateHash { get; protected set; }

  // Public methods
  ////////////////////

  public abstract Geometry Layout(Constraint c);

  public abstract IEnumerable<BaseWidget> Visit();

  public virtual void Render(Terminal t, int x, int y) { }

}

}
