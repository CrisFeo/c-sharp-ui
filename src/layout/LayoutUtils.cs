using System;
using System.Collections.Generic;
using Rendering;

namespace Layout {

static class LayoutUtils {

  // Public methods
  ////////////////////

  public static int Clamp(int min, int max, int value) {
    if (value < min) return min;
    if (value > max) return max;
    return value;
  }

  public static IEnumerable<(BaseWidget, int)> VisitTree(BaseWidget r, int depth = 0) {
    yield return (r, depth);
    depth++;
    foreach (var c in r.Visit()) {
      foreach (var e in VisitTree(c, depth)) {
        yield return e;
      }
    }
  }

  public static void PrintTree(BaseWidget r, int x = 0, int y = 0) {
    foreach (var (w, d) in VisitTree(r)) {
      var indent = new String(' ', 2 * d);
      var (xw, yw) = w.Position;
      x += xw;
      y += yw;
      var g = w.Geometry;
      Console.WriteLine($"{indent}{w.GetType().Name} {x},{y} {g.w}x{g.h}");
    }
  }

}

}
