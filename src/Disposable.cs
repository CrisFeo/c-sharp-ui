using System;

public static class Disposable {

  // Classes
  ////////////////////

  public class Instance : IDisposable {
    Action action;
    public Instance(Action action) => this.action = action;
    public void Dispose() => action();
  }

  // Public methods
  ////////////////////

  public static Instance New(Action action) => new Instance(action);

}
