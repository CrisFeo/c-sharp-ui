using System;
using System.Collections.Generic;
using Rendering;

namespace Layout.Widgets {

public class Row : MultiChildWidget {

  bool reversed;

  public Row(bool reversed, BaseWidget[] children) {
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
        if (children[i].GetType() == typeof(FillWidth)) {
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
        var cx = x;
        if (reversed) cx = width - x - childrenGeometry[i].w;
        children[i].Position = (cx, 0);
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

  public static partial class Library {
    public static Row Row(params BaseWidget[] children) {
      return new Row(false, children);
    }
    public static Row Row(bool reversed, params BaseWidget[] children) {
      return new Row(reversed, children);
    }
  }
}
