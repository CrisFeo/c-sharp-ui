using System;

using K = Rendering.Key;

public static class Program {

  public static void Main() {
    try {
      MatrixOverload.Play(new MatrixOverload.Config {
      });
      /*
      Minesweeper.Play(new Minesweeper.Config {
        size = 20,
        mineChance = 0.15f,
      });
      */
    } catch (Exception e) {
      Console.WriteLine(e);
    }
  }

}
