using System;

public static class Program {

  public static void Main() {
    try {
      LayoutTest.Run();
      /*
      MatrixOverload.Play(new MatrixOverload.Config {
      });
      */
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
