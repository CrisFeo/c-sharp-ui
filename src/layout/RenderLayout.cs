using System;
using System.Collections.Generic;
using Rendering;

namespace Layout {

static class RenderLayout {

  // Public methods
  ////////////////////

  public static void Tree(Terminal t, BaseWidget r, int x = 0, int y = 0) {
    var (xr, yr) = r.Position;
    x += xr;
    y += yr;
    r.Render(t, x, y);
    foreach (var c in r.Visit()) {
      Tree(t, c, x, y);
    }
  }

  public static void Diff(Terminal t, BaseWidget prev, BaseWidget next, int x = 0, int y = 0) {
    var changed = false;
    if (prev.GetType() != next.GetType()) {
      Console.WriteLine($"changed widget   {prev.GetType().Name} => {next.GetType().Name}");
      changed = true;
    } else if (prev.Position != next.Position) {
      Console.WriteLine($"changed position {next.GetType().Name}");
      changed = true;
    } else if (prev.Geometry != next.Geometry) {
      Console.WriteLine($"changed geometry {next.GetType().Name}");
      changed = true;
    } else if (prev.StateHash != next.StateHash) {
      Console.WriteLine($"changed state {next.GetType().Name}");
      changed = true;
    }
    if (changed) {
      Clear(t, prev, x, y);
      Tree(t, next, x, y);
    } else {
      var ePrev = prev.Visit().GetEnumerator();
      var eNext = next.Visit().GetEnumerator();
      while (ePrev.MoveNext() && eNext.MoveNext()) {
        var (xNext, yNext) = next.Position;
        Diff(t, ePrev.Current, eNext.Current, x + xNext, y + yNext);
      }
      while (ePrev.MoveNext()) {
        Console.WriteLine($"removed child {ePrev.Current.GetType().Name}");
        Clear(t, ePrev.Current, x, y);
      }
      while (eNext.MoveNext()) {
        Console.WriteLine($"added child   {eNext.Current.GetType().Name}");
        Tree(t, eNext.Current, x, y);
      }
    }
  }

  // Internal methods
  ////////////////////

  static void Clear(Terminal t, BaseWidget w, int x, int y) {
    var g = w.Geometry;
    if (g.w == 0 || g.h == 0) return;
    var (xw, yw) = w.Position;
    x += xw;
    y += yw;
    for (var yi = 0; yi < g.h; yi++) {
      for (var xi = 0; xi < g.w; xi++) {
        t.Set(x + xi, y + yi, ' ');
      }
    }
  }


}

}
