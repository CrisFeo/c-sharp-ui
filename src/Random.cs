using System;
using System.Collections.Generic;
using System.Collections.Immutable;

public static class Random {

  // Constants
  ////////////////////

  const int A = 16807;
  const int M = 2147483647;
  const int Q = 127773;
  const int R = 2836;

  // Structs
  ////////////////////

  public record State (int seed);

  // Public methods
  ////////////////////

  public static float Next(this State current, out State next) {
    var hi = current.seed / Q;
    var lo = current.seed % Q;
    var seed = (A * lo) - (R * hi);
    if (seed <= 0) seed = seed + M;
    next = new State(seed);
    return (seed * 1f) / M;
  }

  public static int NextInt(this State current, out State next, int min, int max) {
    var t = (max - min) * current.Next(out next);
    return min + (int)t;
  }

  public static void Shuffle<T>(this State current, out State next, ImmutableList<T>.Builder list) {
    next = current;
    var n = list.Count;
    while (n > 1) {
      n--;
      var k = next.NextInt(out next, 0, n + 1);
      var v = list[k];
      list[k] = list[n];
      list[n] = v;
    }
  }

}
