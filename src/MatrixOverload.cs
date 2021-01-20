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

  static readonly Dictionary<int, int> ROYAL_NEIGHBORS = new Dictionary<int, int> {
    [0 * 5 + 1] = 1 * 5 + 1,
    [0 * 5 + 2] = 1 * 5 + 2,
    [0 * 5 + 3] = 1 * 5 + 3,
    [1 * 5 + 0] = 1 * 5 + 1,
    [1 * 5 + 4] = 1 * 5 + 3,
    [2 * 5 + 0] = 2 * 5 + 1,
    [2 * 5 + 4] = 2 * 5 + 3,
    [3 * 5 + 0] = 3 * 5 + 1,
    [3 * 5 + 4] = 3 * 5 + 3,
    [4 * 5 + 1] = 3 * 5 + 1,
    [4 * 5 + 2] = 3 * 5 + 2,
    [4 * 5 + 3] = 3 * 5 + 3
  };

  // Public methods
  ////////////////////

  public static void Play(Config config) {
    App.Terminal(
      init: new State {
        random = new Random.State(12),
        isPlaying = false,
        deck = Lst<Card>.Empty,
        grid = Lst<Lst<Card>>.Empty,
        draw = null,
        x = 2,
        y = 2,
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

  // Enums
  ////////////////////

  // Note: Red are even, black are odd
  enum Suit {
    Hearts   = 0,
    Clubs    = 1,
    Diamonds = 2,
    Spades   = 3,
  }

  // Records
  ////////////////////

  public record Config {
  }

  record State {
    public Random.State   random    { get; init; }
    public bool           isPlaying { get; init; }
    public Lst<Card>      deck      { get; init; }
    public Lst<Lst<Card>> grid      { get; init; }
    public Card           draw      { get; init; }
    public int            x         { get; init; }
    public int            y         { get; init; }
  }

  record Card {
    public int  rank     { get; init; }
    public Suit suit     { get; init; }
    public bool disabled { get; init; }
  }

  record Event {
    public record NewGame()          : Event;
    public record Move(int x, int y) : Event;
    public record Place()            : Event;
  }

  // Internal methods
  ////////////////////

  static bool Input(Terminal t, Action<Event> dispatch) {
    if (t.KeyDown(Key.Q)) return false;
    if (t.KeyDown(Key.S)) dispatch(new Event.NewGame());
    if (t.KeyDown(Key.H)) dispatch(new Event.Move(-1,  0));
    if (t.KeyDown(Key.J)) dispatch(new Event.Move( 0,  1));
    if (t.KeyDown(Key.K)) dispatch(new Event.Move( 0, -1));
    if (t.KeyDown(Key.L)) dispatch(new Event.Move( 1,  0));
    if (t.KeyDown(Key.Y)) dispatch(new Event.Move(-1, -1));
    if (t.KeyDown(Key.U)) dispatch(new Event.Move( 1, -1));
    if (t.KeyDown(Key.B)) dispatch(new Event.Move(-1,  1));
    if (t.KeyDown(Key.N)) dispatch(new Event.Move( 1,  1));
    if (t.KeyDown(Key.Z)) dispatch(new Event.Place());
    return true;
  }

  static (State, Cmd<Event>) Step(State state, Event evt) {
    var random = state.random;
    switch(evt) {
      case Event.NewGame e: {
        var deck = ShuffleDeck(ref random);
        var grid = Lst<Lst<Card>>.Empty.ToBuilder();
        for (var i = 0; i < 25; i++) grid.Add(Lst<Card>.Empty);
        var royals = new List<Card>();
        for (var y = 0; y < 5; y++) {
          for (var x = 0; x < 5; x++) {
            if (y == 0 || y == 4) continue;
            if (x == 0 || x == 4) continue;
            if (x == 2 && y == 2) continue;
            while (true) {
              var card = DrawAnyCard(deck);
              if (card.rank > 10) {
                royals.Add(card);
              } else {
                PushCard(grid, x, y, card);
                break;
              }
            }
          }
        }
        foreach (var royal in royals) PlaceRoyal(grid, royal);
        var draw = DrawNonRoyalCard(deck, grid);
        return (state with {
          random = random,
          isPlaying = true,
          deck = Lst<Card>.Empty.AddRange(deck),
          grid = Lst<Lst<Card>>.Empty.AddRange(grid),
          draw = draw,
          x = 2,
          y = 2,
        }, null);
      }
      case Event.Move e: {
        if (!state.isPlaying) break;
        if (state.draw == null) break;
        return (state with {
          x = Clamp(1, 3, state.x + e.x),
          y = Clamp(1, 3, state.y + e.y),
        }, null);
      }
      case Event.Place e: {
        if (!state.isPlaying) break;
        var grid = state.grid.ToBuilder();
        var deck = state.deck.ToBuilder();
        // Prevent placing lower rank cards on a stack
        var at = TopCard(grid, state.x, state.y);
        if (at != null && state.draw.rank != 1 && at.rank > state.draw.rank) break;
        // Aces reset the stack
        if (state.draw.rank == 1) {
          var index = state.y * 5 + state.x;
          var stack = grid[index];
          for (var i = 0; i < stack.Count; i++) {
            deck.Insert(0, stack[i]);
          }
          grid[index] = Lst<Card>.Empty;
        }
        // Push the drawn card onto the (potentially cleared) stack
        PushCard(grid, state.x, state.y, state.draw);
        // Perform horizontal attacks if possible
        if (state.x != 2) {
          var royalPos = state.x == 1 ? 4 : 0;
          var royal = TopCard(grid, royalPos, state.y);
          var edgePos = state.x == 1 ? 3 : 1;
          var cardA = TopCard(grid, edgePos, state.y);
          var cardB = TopCard(grid, 2, state.y);
          if (CalculateAttack(royal, cardA, cardB)) {
            DisableCard(grid, royalPos, state.y);
          }
        }
        // Perform vertical attacks if possible
        if (state.y != 2) {
          var royalPos = state.y == 1 ? 4 : 0;
          var royal = TopCard(grid, state.x, royalPos);
          var edgePos = state.y == 1 ? 3 : 1;
          var cardA = TopCard(grid, state.x, edgePos);
          var cardB = TopCard(grid, state.x, 2);
          if (CalculateAttack(royal, cardA, cardB)) {
            DisableCard(grid, state.x, royalPos);
          }
        }
        // Draw another card, handling any royals that come up
        var draw = DrawNonRoyalCard(deck, grid);
        return (state with {
          deck = new Lst<Card>(deck),
          grid = new Lst<Lst<Card>>(grid),
          draw = draw,
          x = 2,
          y = 2,
        }, null);
      }
    }
    return (state, null);
  }

  static void View(Terminal t, State state) {
    t.Clear();
    if (state.isPlaying) {
      for (var y = 0; y < 5; y++) {
        for (var x = 0; x < 5; x++) {
          if ((x == 0 || x == 4) && (y == 0 || y == 4)) continue;
          var c = state.grid[y * 5 + x].Last;
          if (c != null) {
            RenderCard(t, x * 4, y * 4, c);
          } else {
            RenderBox(t, x * 4, y * 4, 3, 3, Colors.White, Colors.Black);
          }
        }
      }
      if (state.draw != null) {
        RenderCard(t, 4 * 4, 5 * 4, state.draw);
        RenderBox(t, state.x * 4 - 1, state.y * 4 - 1, 5, 5, Colors.Yellow, Colors.Black);
        var stack = state.grid[state.y * 5 + state.x];
        for (var i = 0; i < stack.Count; i++) {
          RenderCard(t, 5 * 4, i * 4 + 1,stack[stack.Count - 1 - i]);
        }
      }
    } else {
      t.Set(0, 0, "press 's' to start a new game");
    }
    t.Render();
  }

  static void RenderCard(Terminal t, int x, int y, Card c) {
    var fg = c.suit == Suit.Hearts || c.suit == Suit.Diamonds ? Colors.Red : Colors.Black;
    var bg = c.disabled ? Colors.Gray : Colors.White;
    var suitChar = default(char);
    switch (c.suit) {
      case Suit.Hearts:   { suitChar = (char)3; break; }
      case Suit.Diamonds: { suitChar = (char)4; break; }
      case Suit.Clubs:    { suitChar = (char)5; break; }
      case Suit.Spades:   { suitChar = (char)6; break; }
    }
    var rankStr = default(string);
    switch (c.rank) {
      case 1:  { rankStr = "A";               break; }
      case 11: { rankStr = "J";               break; }
      case 12: { rankStr = "Q";               break; }
      case 13: { rankStr = "K";               break; }
      default: { rankStr = c.rank.ToString(); break; }
    }
    t.Set(x, y + 0, rankStr.PadRight(3), fg, bg);
    t.Set(x, y + 1, $" {suitChar} ",     fg, bg);
    t.Set(x, y + 2, rankStr.PadLeft(3),  fg, bg);
  }

  static void RenderBox(Terminal t, int x, int y, int w, int h, Color fg, Color bg) {
    var left = x;
    var top = y;
    var right = left + w - 1;
    var bottom = top + h - 1;
    t.Set(left,     top, (char)218,                    fg, bg);
    t.Set(right,    top, (char)191,                    fg, bg);
    t.Set(left + 1, top, new String((char)196, w - 2), fg, bg);
    for (var i = 1; i < h; i++) {
      t.Set(left,  top + i, (char)179, fg, bg);
      t.Set(right, top + i, (char)179, fg, bg);
    }
    t.Set(left,     bottom, (char)192,                    fg, bg);
    t.Set(right,    bottom, (char)217,                    fg, bg);
    t.Set(left + 1, bottom, new String((char)196, w - 2), fg, bg);
  }

  static ImmutableList<Card>.Builder ShuffleDeck(ref Random.State random) {
    var deck = ImmutableList<Card>.Empty.ToBuilder();
    for (var i = 1; i <= 13; i++) {
      deck.Add(new Card { suit = Suit.Hearts,   rank = i, disabled = false });
      deck.Add(new Card { suit = Suit.Diamonds, rank = i, disabled = false });
      deck.Add(new Card { suit = Suit.Clubs,    rank = i, disabled = false });
      deck.Add(new Card { suit = Suit.Spades,   rank = i, disabled = false });
    }
    random.Shuffle(out random, deck);
    return deck;
  }

  static Card DrawAnyCard(ImmutableList<Card>.Builder deck) {
    var card = deck[deck.Count - 1];
    deck.RemoveAt(deck.Count - 1);
    return card;
  }

  static Card DrawNonRoyalCard(
    ImmutableList<Card>.Builder deck,
    ImmutableList<Lst<Card>>.Builder grid
  ) {
    var card = default(Card);
    while (deck.Count != 0) {
      card = DrawAnyCard(deck);
      if (card.rank > 10) {
        PlaceRoyal(grid, card);
      } else {
        return card;
      }
    }
    return null;
  }

  static void PushCard(ImmutableList<Lst<Card>>.Builder grid, int x, int y, Card card) {
    var index = y * 5 + x;
    PushCard(grid, index, card);
  }

  static void PushCard(ImmutableList<Lst<Card>>.Builder grid, int index, Card card) {
    grid[index] = grid[index].Add(card);
  }

  static Card TopCard(ImmutableList<Lst<Card>>.Builder grid, int x, int y) {
    var index = y * 5 + x;
    return TopCard(grid, index);
  }

  static Card TopCard(ImmutableList<Lst<Card>>.Builder grid, int index) {
    if (grid[index].Count == 0) return null;
    return grid[index].Last;
  }

  static void PlaceRoyal(ImmutableList<Lst<Card>>.Builder grid, Card royal) {
    var bestIndex = -1;
    foreach (var (royalIndex, neighborIndex) in ROYAL_NEIGHBORS) {
      if (TopCard(grid, royalIndex) != null) continue;
      if (bestIndex == -1) {
        bestIndex = royalIndex;
        continue;
      }
      var bestNeighbor = TopCard(grid, ROYAL_NEIGHBORS[bestIndex]);
      var neighbor = TopCard(grid, neighborIndex);
      var suitMatch = royal.suit == neighbor.suit;
      if (royal.suit != bestNeighbor.suit && suitMatch) {
        if (neighbor.rank > bestNeighbor.rank) {
          bestIndex = royalIndex;
          continue;
        }
      }
      var colorMatch = (int)royal.suit % 2 == (int)neighbor.suit % 2;
      if ((int)royal.suit % 2 != (int)bestNeighbor.suit % 2 && colorMatch) {
        if (neighbor.rank > bestNeighbor.rank) {
          bestIndex = royalIndex;
          continue;
        }
      }
    }
    PushCard(grid, bestIndex, royal);
  }

  static bool CalculateAttack(Card royal, Card a, Card b) {
    if (royal == null || a == null || b == null) return false;
    var strength = 0;
    switch (royal.rank) {
      case 13: {
        if (a.suit != royal.suit) break;
        if (b.suit != royal.suit) break;
        strength = a.rank + b.rank;
        break;
      }
      case 12: {
        if ((int)a.suit % 2 != (int)royal.suit % 2) break;
        if ((int)b.suit % 2 != (int)royal.suit % 2) break;
        strength = a.rank + b.rank;
        break;
      }
      case 11: {
        strength = a.rank + b.rank;
        break;
      }
    }
    return strength >= royal.rank;
  }

  static void DisableCard(ImmutableList<Lst<Card>>.Builder grid, int x, int y) {
    var index = y * 5 + x;
    var card = TopCard(grid, index);
    if (card == null) return;
    grid[index] = grid[index].Set(grid[index].Count - 1, card with {
      disabled = true,
    });
  }

  static int Clamp(int min, int max, int val) {
    if (val < min) return min;
    if (val > max) return max;
    return val;
  }

}
