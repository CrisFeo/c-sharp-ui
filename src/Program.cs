using System;

public static class Program {

  public static void Main() {
    try {
      Minesweeper.Play(20, 0.15f);
    } catch (Exception e) {
      throw e;
    }
  }

}
