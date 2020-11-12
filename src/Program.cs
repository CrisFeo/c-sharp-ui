using System;

using K = Rendering.Key;

public static class Program {

  public static void Main() {
    try {
      //Minesweeper.Play(new Minesweeper.Config {
        //size = 20,
        //mineChance = 0.15f,
      //});
      using (var t = new Rendering.Terminal(60, 60, "minesweeper")) {
        var isRunning = true;
        while (isRunning && !t.ShouldClose) {
          t.Clear();
          if (t.Input.Contains(K.Enter)) isRunning = false;
          t.Set(0, 0, "time " + Time.Now().ToString("F1"));
          t.Render();
          t.Poll();
        }
      }
    } catch (Exception e) {
      Console.WriteLine(e);
    }
  }

}
