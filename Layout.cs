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

public interface IWidget {
  Geometry Layout(Constraint c);
  void Render(Terminal t, int x, int y);
}

public abstract class SingleChildWidget : IWidget {
  protected string name;
  protected Geometry geometry;
  protected IWidget child;
  protected (int, int) childPosition;
  public abstract Geometry Layout(Constraint c);
  public virtual void Render(Terminal t, int x, int y) {
    LayoutHelper.Debug(t, name, x, y, geometry);
    if (child == null) return;
    var (childX, childY) = childPosition;
    child.Render(t, x + childX, y + childY);
  }
}


public abstract class MultiChildWidget : IWidget {
  protected string name;
  protected Geometry geometry;
  protected List<IWidget> children;
  protected List<(int, int)> childrenPositions;
  public abstract Geometry Layout(Constraint c);
  public virtual void Render(Terminal t, int x, int y) {
    LayoutHelper.Debug(t, name, x, y, geometry);
    for (var i = 0; i < children.Count; i++) {
      var (childX, childY) = childrenPositions[i];
      children[i].Render(t, x + childX, y + childY);
    }
  }
}

public class FixedWidthWidget : SingleChildWidget {
  int width;
  public FixedWidthWidget(string name, int width, IWidget child) {
    this.name = name;
    this.width = width;
    this.child = child;
  }
  public override Geometry Layout(Constraint c) {
    var w = LayoutHelper.Clamp(c.xMin, c.xMax, width);
    if (child == null) {
      geometry = new Geometry {
        w = w,
        h = c.yMin,
      };
    } else {
      childPosition = (0, 0);
      geometry = child.Layout(c with {
        xMin = w,
        xMax = w,
      });
    }
    return geometry;
  }
}

public class FixedHeightWidget : SingleChildWidget {
  int height;
  public FixedHeightWidget(string name, int height, IWidget child) {
    this.name = name;
    this.height = height;
    this.child = child;
  }
  public override Geometry Layout(Constraint c) {
    var h = LayoutHelper.Clamp(c.yMin, c.yMax, height);
    if (child == null) {
      geometry = new Geometry {
        w = c.xMin,
        h = h,
      };
    } else {
      childPosition = (0, 0);
      geometry = child.Layout(c with {
        yMin = h,
        yMax = h,
      });
    }
    return geometry;
  }
}

public class FillWidthWidget : SingleChildWidget {
  public FillWidthWidget(string name, IWidget child) {
    this.name = name;
    this.child = child;
  }
  public override Geometry Layout(Constraint c) {
    if (child == null) {
      geometry = new Geometry {
        w = c.xMax,
        h = c.yMin,
      };
    } else {
      childPosition = (0, 0);
      geometry = child.Layout(c with {
        xMin = c.xMax,
      });
    }
    return geometry;
  }
}

public class FillHeightWidget : SingleChildWidget {
  public FillHeightWidget(string name, IWidget child) {
    this.name = name;
    this.child = child;
  }
  public override Geometry Layout(Constraint c) {
    if (child == null) {
      geometry = new Geometry {
        w = c.xMin,
        h = c.yMax,
      };
    } else {
      childPosition = (0, 0);
      geometry = child.Layout(c with {
        yMin = c.yMax,
      });
    }
    return geometry;
  }
}

public class ColumnWidget : MultiChildWidget {
  public ColumnWidget(string name, IWidget[] children) {
    this.name = name;
    this.children = new List<IWidget>(children);
    childrenPositions = new List<(int, int)>(children.Length);
    foreach (var c in children) childrenPositions.Add((0, 0));
  }
  public override Geometry Layout(Constraint c) {
    if (children.Count == 0) {
      geometry = new Geometry {
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
        childrenPositions[i] = (0, y);
        y += childrenGeometry[i].h;
      }
      geometry = new Geometry {
        w = width,
        h = height,
      };
    }
    return geometry;
  }
}

public class RowWidget : MultiChildWidget {
  public RowWidget(string name, IWidget[] children) {
    this.name = name;
    this.children = new List<IWidget>(children);
    childrenPositions = new List<(int, int)>(children.Length);
    foreach (var c in children) childrenPositions.Add((0, 0));
  }
  public override Geometry Layout(Constraint c) {
    if (children.Count == 0) {
      geometry = new Geometry {
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
        childrenPositions[i] = (x, 0);
        x += childrenGeometry[i].w;
      }
      geometry = new Geometry {
        w = width,
        h = height,
      };
    }
    return geometry;
  }
}

public class TextWidget : IWidget {
  string name;
  Geometry geometry;
  string[] lines;
  public TextWidget(string name, string text) {
    this.name = name;
    this.lines = text.Split('\n');
  }
  public Geometry Layout(Constraint c) {
    var maxLength = 0;
    foreach (var l in lines) {
      if (l.Length > maxLength) maxLength = l.Length;
    }
    geometry = new Geometry {
      w = LayoutHelper.Clamp(c.xMin, c.xMax, maxLength),
      h = LayoutHelper.Clamp(c.yMin, c.yMax, lines.Length),
    };
    return geometry;
  }
  public virtual void Render(Terminal t, int x, int y) {
    LayoutHelper.Debug(t, name, x, y, geometry);
    for (var i = 0; i < lines.Length; i++) {
      t.Set(x, y + i, lines[i]);
    }
  }
}

public class BorderWidget : SingleChildWidget {
  public BorderWidget(string name, IWidget child) {
    this.name = name;
    this.child = child;
  }
  public override Geometry Layout(Constraint c) {
    if (child == null) {
      geometry = new Geometry {
        w = c.xMin,
        h = c.yMin,
      };
    } else {
      childPosition = (1, 1);
      var g = child.Layout(c with {
        xMax = c.xMax - 2,
        yMax = c.yMax - 2,
      });
      geometry = new Geometry {
        w = g.w + 2,
        h = g.h + 2,
      };
    }
    return geometry;
  }
  public override void Render(Terminal t, int x, int y) {
    base.Render(t, x, y);
    LayoutHelper.Border(t, x, y, geometry.w, geometry.h, Colors.White, Colors.Black);
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

  public static void Debug(Terminal t, string n, int x, int y, Geometry g) {
    Console.WriteLine($"{n} {x},{y} {g.w}x{g.h}");
    //Border(t, x, y, g.w, g.h, Colors.White, Colors.Black);
  }

}

public static class Widgets {

  public static FixedWidthWidget FixedWidth(int width, IWidget child) {
    return new FixedWidthWidget("FixedWidth", width, child);
  }

  public static FixedHeightWidget FixedHeight(int height, IWidget child) {
    return new FixedHeightWidget("FixedHeight", height, child);
  }

  public static FillWidthWidget FillWidth(IWidget child) {
    return new FillWidthWidget("FillWidth", child);
  }

  public static FillHeightWidget FillHeight(IWidget child) {
    return new FillHeightWidget("FillHeight", child);
  }

  public static ColumnWidget Column(params IWidget[] children) {
    return new ColumnWidget("Column", children);
  }

  public static RowWidget Row(params IWidget[] children) {
    return new RowWidget("Row", children);
  }

  public static TextWidget Text(string text) {
    return new TextWidget("Text", text);
  }

  public static BorderWidget Border(IWidget child) {
    return new BorderWidget("Border", child);
  }

}
