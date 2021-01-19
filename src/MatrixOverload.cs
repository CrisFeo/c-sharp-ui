using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Rendering;

public static class MatrixOverload {

  // Constants
  ////////////////////

  const float TICK_INTERVAL = 0.3f;

  // Public methods
  ////////////////////

  public static void Play(Config config) {
    App.Terminal(
      init: new State {
      },
      input: Input,
      step: Step,
      view: View,
      subs: default(Sub<Event>),
      width: 60,
      height: 60,
      title: "Matrix Overload"
    );
  }

  // Records
  ////////////////////

  public record Config {
  }

  record State {
  }

  record Event {
  }

  // Internal methods
  ////////////////////

  static bool Input(Terminal t, Action<Event> dispatch) {
    return true;
  }

  static (State, Cmd<Event>) Step(State state, Event evt) {
    switch(evt) {
    }
    return (state, null);
  }

  static void View(Terminal t, State state) {
    t.Clear();
    for (var y = 0; y < 8; y++) {
      for (var x = 0; x < 8; x++) {
        if ((x == 0 || x == 8-1)  && (y == 0 || y == 8-1)) continue;
        t.Set(x, y, "C");
      }
    }
    t.Render();
  }

  static (string, Color, Color) RenderCard(int x, int y, int rank, Suit suit) {
    var fg = suit == Suit.Hearts || suit = Suit.Diamonds ? Colors.Red : Colors.Black;
    t.Set(x, y, rank.ToString());
    var bg = Colors.White;
    suitCharacter = 'X';
    switch (suit) {
      case Clubs:    { suitChar = 'C'; break; }
      case Diamonds: { suitChar = 'D'; break; }
      case Hearts:   { suitChar = 'H'; break; }
      case Spades:   { suitChar = 'S'; break; }
    }
    return ('.', fg, bg);
  }

  static string Pad(string s, int size) {
    var vs = new String(' ', size);
    String.CopyTo(s, vs, 0);
  }

}

