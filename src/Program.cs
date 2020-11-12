using System;

using T = Rendering.Terminal;
using K = Rendering.Key;

public static class Program {

  public static void Main() {
    try {
      //Minesweeper.Play(new Minesweeper.Config {
        //size = 20,
        //mineChance = 0.15f,
      //});
      T.Startup(60, 60, "minesweeper");
      var isRunning = true;
      while (isRunning && !T.ShouldClose()) {
        T.Clear();
        var input = T.FetchInput();
        if (input.Contains(K.Enter)) isRunning = false;
        T.Set(0, 0, "time " + Time.Now().ToString("F1"));
        T.Render();
        T.Poll();
      }
      T.Shutdown();
    } catch (Exception e) {
      Console.WriteLine(e);
    }
  }

}
