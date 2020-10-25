using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class Store<S, E> {

  // Internal vars
  ////////////////////

  Func<E, S, S> step;
  List<Action> callbacks;
  BlockingCollection<E> events;
  ConcurrentStack<(E, S)> history;

  // Constructors
  ////////////////////

  public Store(Func<E, S, S> step, S initialState) {
    this.step = step;
    callbacks = new List<Action>();
    events = new BlockingCollection<E>();
    history = new ConcurrentStack<(E, S)>();
    history.Push((default(E), initialState));
  }

  // Public methods
  ////////////////////

  public Action Subscribe(Action callback) {
    callbacks.Add(callback);
    return () => callbacks.Remove(callback);
  }

  public S GetState() {
    var ok = history.TryPeek(out var entry);
    if (!ok) throw new Exception("history was empty");
    var (_, state) = entry;
    return state;
  }

  public void Dispatch(E evt) {
    events.Add(evt);
  }

  public void Process() {
    if (events.TryTake(out var evt)) ProcessAndDrain(evt);
  }

  public void ProcessBlocking() {
    var evt = events.Take();
    ProcessAndDrain(evt);
  }

  // Internal methods
  ////////////////////

  void ProcessAndDrain(E evt) {
    var state = GetState();
    do {
      state = step(evt, state);
      history.Push((evt, state));
    } while (events.TryTake(out evt));
    foreach (var cbk in callbacks) cbk();
  }

}

