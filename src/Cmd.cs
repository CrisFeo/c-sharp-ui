using System;
using System.Collections.Generic;

public record Cmd<E>(Action<Action<E>> run);

public static partial class Cmd {

  public static Cmd<E> Many<E>(params Cmd<E>[] cmds) {
    return new Cmd<E>(dispatch => {
      foreach (var c in cmds) c.run(dispatch);
    });
  }

  public static Cmd<E> CurrentTime<E>(Func<float, E> map) {
    return new Cmd<E>(dispatch => dispatch(map(Time.Now())));
  }

}
