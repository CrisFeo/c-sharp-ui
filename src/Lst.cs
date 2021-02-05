using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public struct Lst<T> : IEquatable<Lst<T>>, IEnumerable<T> {

  public static readonly Lst<T> Empty = new Lst<T>(ImmutableList<T>.Empty);

  // Internal vars
  ////////////////////

  readonly ImmutableList<T> data;

  // Constructors
  ////////////////////

  public Lst(int size) => data = ImmutableList<T>.Empty.AddRange(new T[size]);

  public Lst(Lst<T> initial) => data = initial.data;

  public Lst(ImmutableList<T>.Builder initial) => data = initial.ToImmutable();

  Lst(ImmutableList<T> initial) => data = initial;

  // Public properties
  ////////////////////

  public T this[int i] { get => data[i]; }

  public int Count { get => data.Count; }

  public T First { get => Count == 0 ? default(T) : this[0]; }

  public T Last { get => Count == 0 ? default(T) : this[Count - 1]; }

  // Public methods
  ////////////////////

  public Lst<T> Add(T v) => new Lst<T>(data.Add(v));

  public Lst<T> AddRange(IEnumerable<T> vs) => new Lst<T>(data.AddRange(vs));

  public Lst<T> Set(int i, T v) => new Lst<T>(data.SetItem(i, v));

  public Lst<U> Map<U>(Func<T, U> fn) => new Lst<U>(data.ConvertAll<U>(fn));

  public ImmutableList<T>.Builder ToBuilder() => data.ToBuilder();

  public bool Equals(Lst<T> l) => data.SequenceEqual(l.data);

  public override int GetHashCode() => this.GetHashCode();

  public static bool operator==(Lst<T> a, Lst<T> b) => a.Equals(b);

  public static bool operator!=(Lst<T> a, Lst<T> b) => !a.Equals(b);

  public override bool Equals(object o) => (o is Lst<T> e) ? Equals(e) : false;

  public IEnumerator<T> GetEnumerator() {
    for (var i = 0; i < Count; i++) {
      yield return this[i];
    }
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }

}
