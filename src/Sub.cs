using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public record Sub<E>(Action<Action<E>> start, Action stop);

public static partial class Sub {

  public static Sub<E> Many<E>(params Sub<E>[] subs) {
    return new Sub<E>(
      dispatch => {
        foreach (var s in subs) s.start(dispatch);
      },
      () => {
        foreach (var s in subs) s.stop();
      }
    );
  }

  public static Sub<E> Every<E>(float interval, Func<E> map) {
    var ts = TimeSpan.FromSeconds(interval);
    var cts = new CancellationTokenSource();
    return new Sub<E>(
      async dispatch => {
        var ct = cts.Token;
        while (!ct.IsCancellationRequested) {
          await Task.Delay(ts, ct).ContinueWith(t => {});
          dispatch(map());
        }
      },
      () => cts.Cancel()
    );
  }

}
