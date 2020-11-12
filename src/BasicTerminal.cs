using System;
using System.Diagnostics;

public static class BasicTerminal {

  // Static vars
  ////////////////////

  static bool isRunning;

  // Public methods
  ////////////////////

  public static Disposable.Instance Initialize() {
    Debug.Assert(!isRunning);
    Startup();
    return Disposable.New(Shutdown);
  }

  public static bool ReadKey(out ConsoleKeyInfo key) {
    Debug.Assert(isRunning);
    var ok = Console.KeyAvailable;
    key = ok ? Console.ReadKey(false) : default(ConsoleKeyInfo);
    return ok;
  }

  public static void Draw(string buffer) {
    Debug.Assert(isRunning);
    var lines = buffer.Split(Environment.NewLine);
    Debug.Assert(lines.Length <= Console.WindowHeight);
    var blankLine = new string(' ', Console.WindowWidth);
    for (var i = 0; i < Console.WindowHeight; i++) {
      Console.SetCursorPosition(0, i);
      var line = blankLine;
      if (i < lines.Length) {
        line = lines[i].PadRight(Console.WindowWidth, ' ');
      }
      Debug.Assert(line.Length == Console.WindowWidth);
      if (i < Console.WindowHeight - 1) {
        Console.Write(line);
      } else {
        Console.Write(line[line.Length - 1]);
        Console.MoveBufferArea(0, i, 1, 1, Console.WindowWidth - 1, i);
        Console.CursorLeft = 0;
        Console.Write(line.Substring(0, line.Length - 1));
      }
    }
    Console.CursorLeft = 0;
  }

  // Internal methods
  ////////////////////

  static void Startup() {
    Console.CursorVisible = false;
    Console.CancelKeyPress += OnCancelKeyPress;
    isRunning = true;
    Console.Clear();
    Console.SetCursorPosition(0, 0);
  }

  static void Shutdown() {
    Console.CursorVisible = true;
    Console.CancelKeyPress -= OnCancelKeyPress;
    isRunning = false;
    Console.Clear();
    Console.SetCursorPosition(0, 0);
  }

  static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs evt) {
    Shutdown();
  }

}
