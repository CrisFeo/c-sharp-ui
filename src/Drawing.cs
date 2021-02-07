using System;
using Rendering;

public static class Drawing {

  public static void Box(Terminal t, int x, int y, int w, int h, Color fg, Color bg) {
    if (w == 0 || h == 0) return;
    var left = x;
    var top = y;
    var right = left + w - 1;
    var bottom = top + h - 1;
    t.Set(left,     top, (char)218,  fg, bg);
    t.Set(right,    top, (char)191,  fg, bg);
    for (var i = 1; i < w - 1; i++) {
      t.Set(left + i, top, (char)196, fg, bg);
    }
    for (var i = 1; i < h; i++) {
      t.Set(left,  top + i, (char)179, fg, bg);
      t.Set(right, top + i, (char)179, fg, bg);
    }
    t.Set(left,     bottom, (char)192,  fg, bg);
    t.Set(right,    bottom, (char)217,  fg, bg);
    for (var i = 1; i < w - 1; i++) {
      t.Set(left + i, bottom, (char)196, fg, bg);
    }
  }

  public static void Fill(Terminal t, int x, int y, int w, int h, Color fg, Color bg) {
    if (w == 0 || h == 0) return;
    for (var yi = 0; yi < h; yi++) {
      for (var xi = 0; xi < w; xi++) {
        t.Set(x + xi, y + yi, ' ', fg, bg);
      }
    }
  }

}
