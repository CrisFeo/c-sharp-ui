using System;
using System.Diagnostics;
using System.Threading;

public static class Time {

  // Constants
  ////////////////////

  const float BUSY_WAIT_THRESHOLD = 0.016f;

  // Internal vars
  ////////////////////

  static Stopwatch stopwatch;

  // Constructor
  ////////////////////

  static Time() {
    stopwatch = new Stopwatch();
    stopwatch.Start();
  }

  // Public methods
  ////////////////////

  public static float Now() {
    return (float)stopwatch.Elapsed.TotalSeconds;
  }

  public static void Yield() {
    Thread.Yield();
  }

  public static float Sleep(float duration) {
    var start = Now();
    var end = start + duration;
    var remaining = end - Now();
    while (remaining > 0) {
      if (remaining < BUSY_WAIT_THRESHOLD) {
        while (Now() < end);
      } else {
        var sleepTime = (int)((remaining - BUSY_WAIT_THRESHOLD) * 1000);
        Thread.Sleep(sleepTime);
      }
      remaining = end - Now();
    }
    return Now() - start;
  }

}
