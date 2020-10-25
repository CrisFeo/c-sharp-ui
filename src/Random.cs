using System;
using System.Collections.Generic;

public static class Random {

  // Constants
  ////////////////////

  const int A = 16807;
  const int M = 2147483647;
  const int Q = 127773;
  const int R = 2836;

  // Structs
  ////////////////////

  public struct State {
    public int seed;
  }

  // Public methods
  ////////////////////

  public static State New(int seed) {
    return new State { seed = seed };
  }

  public static float Next(this State current, out State next) {
    var hi = current.seed / Q;
    var lo = current.seed % Q;
    next.seed = (A * lo) - (R * hi);
    if (next.seed <= 0) next.seed = next.seed + M;
    return (next.seed * 1f) / M;
  }

}
