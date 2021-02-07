using System;
using System.Collections.Generic;
using Rendering;

namespace Layout.Widgets {

public class Text : BaseWidget {

  string[] lines;

  public Text(string text) {
    StateHash = (text).GetHashCode();
    this.lines = text.Split('\n');
  }

  public override Geometry Layout(Constraint c) {
    PassInheritedProperties();
    var maxLength = 0;
    foreach (var l in lines) {
      if (l.Length > maxLength) maxLength = l.Length;
    }
    Geometry = new Geometry {
      w = LayoutUtils.Clamp(c.xMin, c.xMax, maxLength),
      h = LayoutUtils.Clamp(c.yMin, c.yMax, lines.Length),
    };
    return Geometry;
  }

  public override IEnumerable<BaseWidget> Visit() { yield break; }

  public override void Render(Terminal t, int x, int y) {
    for (var i = 0; i < lines.Length; i++) {
      t.Set(x, y + i, lines[i], Foreground, Background);
    }
  }

}

  public static partial class Library {
    public static Text Text(string text) {
      return new Text(text);
    }
  }
}
