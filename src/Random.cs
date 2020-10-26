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

}
