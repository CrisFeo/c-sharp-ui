using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Rendering;
using Layout;

using W = Layout.Widgets.Library;

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

  public static void Run() {
    App.Widget(
      init: Init,
      subs: Subs,
      step: Step,
      view: View,
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

  record State {
    public Random.State    random    { get; init; }
    public bool            isPlaying { get; init; }
    public Lst<Card>       deck      { get; init; }
    public Lst<Lst<Card>?> grid      { get; init; }
    public Card            draw      { get; init; }
    public int             x         { get; init; }
    public int             y         { get; init; }
  }

  record Card {
    public int  rank     { get; init; }
    public Suit suit     { get; init; }
    public bool disabled { get; init; }
  }

  record Event {
    public record Quit()             : Event;
    public record NewGame()          : Event;
    public record Move(int x, int y) : Event;
    public record Place()            : Event;
  }

  // Internal methods
  ////////////////////

  static State Init() {
    return new State {
      random = new Random.State(12),
      isPlaying = false,
      deck = Lst<Card>.Empty,
      grid = Lst<Lst<Card>?>.Empty,
      draw = null,
      x = 2,
      y = 2,
    };
  }

  static Sub<Event> Subs(Terminal t) {
    return Sub.KeyDown(t, OnKeyDown);
  }

  static Event OnKeyDown(Key k) {
    switch (k) {
      case Key.Q:     return new Event.Quit();
      case Key.S:     return new Event.NewGame();
      case Key.Left:  return new Event.Move(-1,  0);
      case Key.Right: return new Event.Move( 1,  0);
      case Key.Down:  return new Event.Move( 0,  1);
      case Key.Up:    return new Event.Move( 0, -1);
      case Key.H:     return new Event.Move(-1,  0);
      case Key.J:     return new Event.Move( 0,  1);
      case Key.K:     return new Event.Move( 0, -1);
      case Key.L:     return new Event.Move( 1,  0);
      case Key.Y:     return new Event.Move(-1, -1);
      case Key.U:     return new Event.Move( 1, -1);
      case Key.B:     return new Event.Move(-1,  1);
      case Key.N:     return new Event.Move( 1,  1);
      case Key.Z:     return new Event.Place();
    }
    return null;
  }

  static (State, Cmd<Event>) Step(State state, Event evt) {
    var random = state.random;
    switch(evt) {
      case Event.Quit e: {
        return (state, Cmd.Quit<Event>());
      }
      case Event.NewGame e: {
        var deck = ShuffleDeck(ref random);
        var grid = Lst<Lst<Card>?>.Empty.ToBuilder();
        for (var i = 0; i < 25; i++) {
          if (i == 0 || i == 4 || i == 5 * 4 || i == 5 * 4 + 4) {
            grid.Add(null);
          } else {
            grid.Add(Lst<Card>.Empty);
          }
        }
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
          grid = Lst<Lst<Card>?>.Empty.AddRange(grid),
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
          var stack = grid[index].Value;
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
          grid = new Lst<Lst<Card>?>(grid),
          draw = draw,
          x = 2,
          y = 2,
        }, null);
      }
    }
    return (state, null);
  }

  static BaseWidget View(State state) {
    if (!state.isPlaying) return Center(
      W.Text("press 's' to start a new game")
    );
    var selectedIndex = state.y * 5 + state.x;
    var stack = state.grid[selectedIndex] ?? Lst<Card>.Empty;
    var index = 0;
    var cells = state.grid.Map(s => {
      var borderColor = index == selectedIndex ? Colors.Yellow : Colors.Black;
      index++;
      return W.ForegroundColor(borderColor,
        W.Border(
          CardStack(s)
        )
      );
    }).ToArray();
    return Center(
      W.Row(
        Grid(5, cells),
        W.FixedWidth(2),
          W.Column(
            W.Border(
              PlayingCard(state.draw)
            ),
            W.Border(
              stack.Count == 0 ? Fixed(3, 3) : W.Column(true,
                stack.Map(PlayingCard).ToArray()
              )
            )
          )
      )
    );
  }

  static BaseWidget CardStack(Lst<Card>? s) {
    if (s == null) return Fixed(3, 3);
    var c = s.Value.Last;
    return PlayingCard(c);
  }

  static BaseWidget PlayingCard(Card c) {
    if (c == null) return Fixed(3, 3,
      W.ForegroundColor(Colors.White,
        W.Border(
          Fill()
        )
      )
    );
    var fg = c.suit == Suit.Hearts || c.suit == Suit.Diamonds ? Colors.Red : Colors.Black;
    switch (c.suit) {
      case Suit.Hearts:   fg = Colors.Red;   break;
      case Suit.Clubs:    fg = Colors.Black; break;
      case Suit.Diamonds: fg = Colors.Red;   break;
      case Suit.Spades:   fg = Colors.Black; break;
    }
    var bg = c.disabled ? Colors.Gray : Colors.White;
    var suitStr = default(string);
    switch (c.suit) {
      case Suit.Hearts:   { suitStr += (char)3; break; }
      case Suit.Diamonds: { suitStr += (char)4; break; }
      case Suit.Clubs:    { suitStr += (char)5; break; }
      case Suit.Spades:   { suitStr += (char)6; break; }
    }
    var rankStr = default(string);
    switch (c.rank) {
      case 1:  { rankStr = "A";               break; }
      case 11: { rankStr = "J";               break; }
      case 12: { rankStr = "Q";               break; }
      case 13: { rankStr = "K";               break; }
      default: { rankStr = c.rank.ToString(); break; }
    }
    return Fixed(3, 3,
      W.ForegroundColor(fg,
        W.BackgroundColor(bg,
          W.Pane(
            W.Column(
              W.Row(
                W.Text(rankStr),
                W.FillWidth()
              ),
              W.Row(
                W.FillWidth(),
                W.Text(suitStr),
                W.FillWidth()
              ),
              W.Row(
                W.FillWidth(),
                W.Text(rankStr)
              )
            )
          )
        )
      )
    );
  }

  static BaseWidget Grid(int columns, params BaseWidget[] cells) {
    var rows = new BaseWidget[(int)(cells.Length / columns)];
    for (var y = 0; y < rows.Length; y++) {
      var rowSize = cells.Length - y;
      if (rowSize > columns) rowSize = columns;
      var row = new BaseWidget[rowSize];
      for (var x = 0; x < rowSize; x++) {
        row[x] = cells[columns * y + x];
      }
      rows[y] = W.Row(row);
    }
    return W.Column(rows);
  }

  static BaseWidget Fixed(int w, int h, BaseWidget child = null) =>
    W.FixedWidth(w, W.FixedHeight(h, child));

  static BaseWidget Fill(BaseWidget child = null) =>
    W.FillWidth(W.FillHeight(child));

  static BaseWidget Center(BaseWidget child = null) =>
    W.Row(
      W.FillWidth(),
      W.Column(
        W.FillHeight(),
        child,
        W.FillHeight()
      ),
      W.FillWidth()
    );

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
    ImmutableList<Lst<Card>?>.Builder grid
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

  static void PushCard(ImmutableList<Lst<Card>?>.Builder grid, int x, int y, Card card) {
    var index = y * 5 + x;
    PushCard(grid, index, card);
  }

  static void PushCard(ImmutableList<Lst<Card>?>.Builder grid, int index, Card card) {
    if (index >= grid.Count) return;
    grid[index] = grid[index]?.Add(card) ?? Lst<Card>.Empty.Add(card);
  }

  static Card TopCard(ImmutableList<Lst<Card>?>.Builder grid, int x, int y) {
    var index = y * 5 + x;
    return TopCard(grid, index);
  }

  static Card TopCard(ImmutableList<Lst<Card>?>.Builder grid, int index) {
    if (index >= grid.Count) return null;
    return grid[index]?.Last ?? null;
  }

  static void PlaceRoyal(ImmutableList<Lst<Card>?>.Builder grid, Card royal) {
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

  static void DisableCard(ImmutableList<Lst<Card>?>.Builder grid, int x, int y) {
    var index = y * 5 + x;
    var card = TopCard(grid, index);
    if (card == null) return;
    var stack = grid[index].Value;
    grid[index] = stack.Set(stack.Count - 1, card with {
      disabled = true,
    });
  }

  static int Clamp(int min, int max, int val) {
    if (val < min) return min;
    if (val > max) return max;
    return val;
  }

}
