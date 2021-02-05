using System;
using System.Collections.Generic;
using Rendering;

namespace Layout {

public record Constraint {
  public int xMin { get; init; }
  public int xMax { get; init; }
  public int yMin { get; init; }
  public int yMax { get; init; }
}

}
