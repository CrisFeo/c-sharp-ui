using System;
using System.Collections.Generic;
using Rendering;

namespace Layout.Widgets {

public class Column : MultiChildWidget {

  bool reversed;

  public Column(bool reversed, BaseWidget[] children) {
    StateHash = (reversed).GetHashCode();
    this.reversed = reversed;
    this.children = new List<BaseWidget>(children);
  }

  public override Geometry Layout(Constraint c) {
    PassInheritedProperties();
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
        if (children[i].GetType() == typeof(FillHeight)) {
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
        var cy = y;
        if (reversed) cy = height - y - childrenGeometry[i].h;
        children[i].Position = (0, cy);
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

public static partial class Library {
  public static Column Column(params BaseWidget[] children) {
    return new Column(false, children);
  }

  public static Column Column(bool reversed, params BaseWidget[] children) {
    return new Column(reversed, children);
  }
}

}
