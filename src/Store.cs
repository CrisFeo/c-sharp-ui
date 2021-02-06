using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class Store<State, Event> : IDisposable {

  // Internal vars
  ////////////////////

  Sub<Event> subs;
  Func<State, Event, (State, Cmd<Event>)> step;
  Action<State> view;
  BlockingCollection<Event> events;
  Stack<(Event, State)> history;

  // Public properties
  ////////////////////

  public bool ShouldQuit { get; private set; }

  // Constructors
  ////////////////////

  public Store(
    State init,
    Sub<Event> subs,
    Func<State, Event, (State, Cmd<Event>)> step,
    Action<State> view
  ) {
    this.subs = subs;
    this.step = step;
    this.view = view;
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
    if (evt == null) return;
    events.Add(evt);
  }

  public void Process() {
    if (events.TryTake(out var evt)) ProcessAndDrain(evt);
  }

  public void ProcessBlocking() {
    var evt = events.Take();
    ProcessAndDrain(evt);
  }

  public void ForceRedraw() {
    view(GetState());
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
      if (cmd != null) {
        if (cmd.GetType() == typeof(QuitCmd<Event>)) ShouldQuit = true;
        cmd.run?.Invoke(Dispatch);
      }
    } while (events.TryTake(out evt));
    if (!state.Equals(previousState)) view(state);
  }

}

public record QuitCmd<E>() : Cmd<E>(default(Action<Action<E>>));

public static partial class Cmd {

  public static Cmd<E> Quit<E>() {
    return new QuitCmd<E>();
  }

}

