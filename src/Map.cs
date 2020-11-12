using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public struct Map<K, V> : IEquatable<Map<K, V>> {

  public static readonly Map<K, V> Empty = new Map<K, V>(ImmutableDictionary<K, V>.Empty);

  // Internal vars
  ////////////////////

  readonly ImmutableDictionary<K, V> data;

  // Constructors
  ////////////////////

  public Map(Map<K, V> initial) => data = initial.data;

  public Map(ImmutableDictionary<K, V>.Builder initial) => data = initial.ToImmutable();

  Map(ImmutableDictionary<K, V> initial) => data = initial;

  // Public properties
  ////////////////////

  public V this[K k] { get => data[k]; }

  // Public methods
  ////////////////////

  public Map<K, V> Set(K k, V v) => new Map<K, V>(data.SetItem(k, v));

  public ImmutableDictionary<K, V>.Builder ToBuilder() => data.ToBuilder();

  public bool Equals(Map<K, V> m) => data.SequenceEqual(m.data);

  public override int GetHashCode() => this.GetHashCode();

  public static bool operator==(Map<K, V> a, Map<K, V> b) => a.Equals(b);

  public static bool operator!=(Map<K, V> a, Map<K, V> b) => !a.Equals(b);

  public override bool Equals(object o) => (o is Map<K, V> e) ? Equals(e) : false;

}
