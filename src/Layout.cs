using System;
using System.Collections.Generic;
using Rendering;

public record Constraint {
  public int xMin { get; init; }
  public int xMax { get; init; }
  public int yMin { get; init; }
  public int yMax { get; init; }
}

public record Geometry {
  public int w { get; init; }
  public int h { get; init; }
}

public abstract class BaseWidget {
  public Geometry Geometry { get; protected set; }
  public (int, int) Position { get; set; }
  public int StateHash { get; protected set; }
  public abstract Geometry Layout(Constraint c);
  public abstract IEnumerable<BaseWidget> Visit();
  public virtual void Render(Terminal t, int x, int y) { }
}

public abstract class SingleChildWidget : BaseWidget {

  protected BaseWidget child;

  public override IEnumerable<BaseWidget> Visit() {
    if (child == null) yield break;
    yield return child;
  }

}


public abstract class MultiChildWidget : BaseWidget {

  protected List<BaseWidget> children;

  public override IEnumerable<BaseWidget> Visit() {
    for (var i = 0; i < children.Count; i++) {
      yield return children[i];
    }
  }

}

public class FixedWidthWidget : SingleChildWidget {

  int width;

  public FixedWidthWidget(int width, BaseWidget child) {
    this.width = width;
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
    var w = LayoutHelper.Clamp(c.xMin, c.xMax, width);
    if (child == null) {
      Geometry = new Geometry {
        w = w,
        h = c.yMin,
      };
    } else {
      child.Position = (0, 0);
      Geometry = child.Layout(c with {
        xMin = w,
        xMax = w,
      });
    }
    return Geometry;
  }

}

public class FixedHeightWidget : SingleChildWidget {

  int height;

  public FixedHeightWidget(int height, BaseWidget child) {
    this.height = height;
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
    var h = LayoutHelper.Clamp(c.yMin, c.yMax, height);
    if (child == null) {
      Geometry = new Geometry {
        w = c.xMin,
        h = h,
      };
    } else {
      child.Position = (0, 0);
      Geometry = child.Layout(c with {
        yMin = h,
        yMax = h,
      });
    }
    return Geometry;
  }

}

public class FillWidthWidget : SingleChildWidget {

  public FillWidthWidget(BaseWidget child) {
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
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

public class FillHeightWidget : SingleChildWidget {

  public FillHeightWidget(BaseWidget child) {
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
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

public class ColumnWidget : MultiChildWidget {

  public ColumnWidget(BaseWidget[] children) {
    this.children = new List<BaseWidget>(children);
  }

  public override Geometry Layout(Constraint c) {
    if (children.Count == 0) {
      Geometry = new Geometry {
        w = c.xMin,
        h = c.yMin,
      };
    } else {
      var flexible = new List<int>();
      var inflexible = new List<int>();
      var childrenGeometry = new List<Geometry>(children.Count);
      for(var i = 0; i < children.Count; i++) {
        if (children[i].GetType() == typeof(FillHeightWidget)) {
          flexible.Add(i);
        } else {
          inflexible.Add(i);
        }
        childrenGeometry.Add(null);
      }
      var height = 0;
      foreach (var i in inflexible) {
       childrenGeometry[i] = children[i].Layout(c with {
          yMin = 0,
          yMax = c.yMax - height,
        });
        height += childrenGeometry[i].h;
      }
      if (flexible.Count > 0) {
        var remaining = c.yMax - height;
        var perChild = remaining / flexible.Count;
        foreach (var i in flexible) {
          remaining -= perChild;
          if (remaining < perChild) perChild += remaining;
         childrenGeometry[i] = children[i].Layout(c with {
            yMin = 0,
            yMax = perChild,
          });
          height += childrenGeometry[i].h;
        }
      }
      var y = 0;
      var width = 0;
      for(var i = 0; i < children.Count; i++) {
        if (childrenGeometry[i].w > width) width = childrenGeometry[i].w;
        children[i].Position = (0, y);
        y += childrenGeometry[i].h;
      }
      Geometry = new Geometry {
        w = width,
        h = height,
      };
    }
    return Geometry;
  }

}

public class RowWidget : MultiChildWidget {

  public RowWidget(BaseWidget[] children) {
    this.children = new List<BaseWidget>(children);
  }

  public override Geometry Layout(Constraint c) {
    if (children.Count == 0) {
      Geometry = new Geometry {
        w = c.xMin,
        h = c.yMin,
      };
    } else {
      var flexible = new List<int>();
      var inflexible = new List<int>();
      var childrenGeometry = new List<Geometry>(children.Count);
      for(var i = 0; i < children.Count; i++) {
        if (children[i].GetType() == typeof(FillWidthWidget)) {
          flexible.Add(i);
        } else {
          inflexible.Add(i);
        }
        childrenGeometry.Add(null);
      }
      var width = 0;
      foreach (var i in inflexible) {
       childrenGeometry[i] = children[i].Layout(c with {
          xMin = 0,
          xMax = c.xMax - width,
        });
        width += childrenGeometry[i].w;
      }
      if (flexible.Count > 0) {
        var remaining = c.xMax - width;
        var perChild = remaining / flexible.Count;
        foreach (var i in flexible) {
          remaining -= perChild;
          if (remaining < perChild) perChild += remaining;
         childrenGeometry[i] = children[i].Layout(c with {
            xMin = 0,
            xMax = perChild,
          });
          width += childrenGeometry[i].w;
        }
      }
      var x = 0;
      var height = 0;
      for(var i = 0; i < children.Count; i++) {
        if (childrenGeometry[i].h > height) height = childrenGeometry[i].h;
        children[i].Position = (x, 0);
        x += childrenGeometry[i].w;
      }
      Geometry = new Geometry {
        w = width,
        h = height,
      };
    }
    return Geometry;
  }

}

public class TextWidget : BaseWidget {

  string[] lines;

  public TextWidget(string text) {
    StateHash = (text).GetHashCode();
    this.lines = text.Split('\n');
  }

  public override Geometry Layout(Constraint c) {
    var maxLength = 0;
    foreach (var l in lines) {
      if (l.Length > maxLength) maxLength = l.Length;
    }
    Geometry = new Geometry {
      w = LayoutHelper.Clamp(c.xMin, c.xMax, maxLength),
      h = LayoutHelper.Clamp(c.yMin, c.yMax, lines.Length),
    };
    return Geometry;
  }

  public override IEnumerable<BaseWidget> Visit() { yield break; }

  public override void Render(Terminal t, int x, int y) {
    for (var i = 0; i < lines.Length; i++) {
      t.Set(x, y + i, lines[i]);
    }
  }

}

public class BorderWidget : SingleChildWidget {

  public BorderWidget(BaseWidget child) {
    this.child = child;
  }

  public override Geometry Layout(Constraint c) {
    if (child == null) {
      Geometry = new Geometry {
        w = c.xMin,
        h = c.yMin,
      };
    } else {
      child.Position = (1, 1);
      var g = child.Layout(c with {
        xMax = c.xMax - 2,
        yMax = c.yMax - 2,
      });
      Geometry = new Geometry {
        w = g.w + 2,
        h = g.h + 2,
      };
    }
    return Geometry;
  }

  public override void Render(Terminal t, int x, int y) {
    LayoutHelper.Border(t, x, y, Geometry.w, Geometry.h, Colors.White, Colors.Black);
  }

}

static class LayoutHelper {

  public static int Clamp(int min, int max, int value) {
    if (value < min) return min;
    if (value > max) return max;
    return value;
  }

  public static void Border(Terminal t, int x, int y, int w, int h, Color fg, Color bg) {
    if (w == 0 || h == 0) return;
    var left = x;
    var top = y;
    var right = left + w - 1;
    var bottom = top + h - 1;
    var horizontal = new String((char)196, w - 2);
    t.Set(left,     top, (char)218,  fg, bg);
    t.Set(right,    top, (char)191,  fg, bg);
    t.Set(left + 1, top, horizontal, fg, bg);
    for (var i = 1; i < h; i++) {
      t.Set(left,  top + i, (char)179, fg, bg);
      t.Set(right, top + i, (char)179, fg, bg);
    }
    t.Set(left,     bottom, (char)192,  fg, bg);
    t.Set(right,    bottom, (char)217,  fg, bg);
    t.Set(left + 1, bottom, horizontal, fg, bg);
  }

  public static void Blank(Terminal t, int x, int y, int w, int h) {
    if (w == 0 || h == 0) return;
    var line = new String(' ', w);
    for (var i = 0; i < h; i++) {
      t.Set(x, y + i, line);
    }
  }

  public static IEnumerable<(BaseWidget, int)> Tree(BaseWidget r,  int depth = 0) {
    yield return (r, depth);
    depth++;
    foreach (var c in r.Visit()) {
      foreach (var e in Tree(c, depth)) {
        yield return e;
      }
    }
  }

  public static void PrintTree(BaseWidget r, int x, int y) {
    foreach (var (w, d) in Tree(r)) {
      var indent = new String(' ', 2 * d);
      var (xw, yw) = w.Position;
      x += xw;
      y += yw;
      var g = w.Geometry;
      Console.WriteLine($"{indent}{w.GetType().Name} {x},{y} {g.w}x{g.h}");
    }
  }

  public static void RenderSubTree(Terminal t, BaseWidget r, int x, int y) {
    var (xr, yr) = r.Position;
    x += xr;
    y += yr;
    r.Render(t, x, y);
    foreach (var c in r.Visit()) {
      RenderSubTree(t, c, x, y);
    }
  }

  public static void RenderDiff(Terminal t, BaseWidget prev, BaseWidget next, int x, int y) {
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
      var (xPrev, yPrev) = prev.Position;
      Blank(t, x + xPrev, y + yPrev, prev.Geometry.w, prev.Geometry.h);
      RenderSubTree(t, next, x, y);
    } else {
      var ePrev = prev.Visit().GetEnumerator();
      var eNext = next.Visit().GetEnumerator();
      while (ePrev.MoveNext() && eNext.MoveNext()) {
        var (xNext, yNext) = next.Position;
        RenderDiff(t, ePrev.Current, eNext.Current, x + xNext, y + yNext);
      }
      while (ePrev.MoveNext()) {
        Console.WriteLine($"removed child {ePrev.Current.GetType().Name}");
        var (xPrev, yPrev) = ePrev.Current.Position;
        Blank(t, x + xPrev, y + yPrev, ePrev.Current.Geometry.w, ePrev.Current.Geometry.h);
      }
      while (eNext.MoveNext()) {
        Console.WriteLine($"added child {eNext.Current.GetType().Name}");
        RenderSubTree(t, eNext.Current, x, y);
      }
    }
  }

}

public static class Widgets {

  public static FixedWidthWidget FixedWidth(int width, BaseWidget child) {
    return new FixedWidthWidget(width, child);
  }

  public static FixedHeightWidget FixedHeight(int height, BaseWidget child) {
    return new FixedHeightWidget(height, child);
  }

  public static FillWidthWidget FillWidth(BaseWidget child) {
    return new FillWidthWidget(child);
  }

  public static FillHeightWidget FillHeight(BaseWidget child) {
    return new FillHeightWidget(child);
  }

  public static ColumnWidget Column(params BaseWidget[] children) {
    return new ColumnWidget(children);
  }

  public static RowWidget Row(params BaseWidget[] children) {
    return new RowWidget(children);
  }

  public static TextWidget Text(string text) {
    return new TextWidget(text);
  }

  public static BorderWidget Border(BaseWidget child) {
    return new BorderWidget(child);
  }

}
