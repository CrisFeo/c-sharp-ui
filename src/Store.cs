using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class Store<State, Event> : IDisposable {

  // Internal vars
  ////////////////////

  Func<State, Event, (State, Cmd<Event>)> step;
  Action<State> view;
  Sub<Event> subs;
  BlockingCollection<Event> events;
  Stack<(Event, State)> history;

  // Constructors
  ////////////////////

  public Store(
    State init,
    Func<State, Event, (State, Cmd<Event>)> step,
    Action<State> view,
    Sub<Event> subs
  ) {
    this.step = step;
    this.view = view;
    this.subs = subs;
    events = new BlockingCollection<Event>();
    history = new Stack<(Event, State)>();
    history.Push((default(Event), init));
  }

  // Public methods
  ////////////////////

  public void Dispose() {
    subs?.stop();
  }

  public void Start() {
    view(GetState());
    subs?.start(Dispatch);
  }

  public void Dispatch(Event evt) {
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

  State GetState() {
    var ok = history.TryPeek(out var entry);
    if (!ok) throw new Exception("history was empty");
    var (_, state) = entry;
    return state;
  }

  void ProcessAndDrain(Event evt) {
    var previousState = GetState();
    var state = previousState;
    var cmd = default(Cmd<Event>);
    do {
      (state, cmd) = step(state, evt);
      history.Push((evt, state));
      cmd?.run(Dispatch);
    } while (events.TryTake(out evt));
    if (!state.Equals(previousState)) view(state);
  }

}
