using System;

public static class Program {

  public static void Main() {
    try {
      Todo.Run();
      //MatrixOverload.Run();
      //Minesweeper.Run(new Minesweeper.Config { size = 20, mineChance = 0.15f });
      //Layout.Tests.Runner.Run();
    } catch (Exception e) {
      Console.WriteLine(e);
    }
  }

}
