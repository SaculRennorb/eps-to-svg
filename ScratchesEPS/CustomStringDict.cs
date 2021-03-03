﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ScratchesEPS {
  class CustomStringDict<T> : IDictionary<string, T> {
    private Entry[] _buckets;
    private int     _bucketsUsed;

    public CustomStringDict(int initialSize) {
      _buckets = new Entry[initialSize];
      for (int i = 0; i < _buckets.Length; i++) {
        _buckets[i] = new Entry(initialSize: 2);
      }
    }

    public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => new Enumerator(_buckets);
    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    private struct Enumerator : IEnumerator<KeyValuePair<string, T>> {
      private readonly Entry[] _buckets;
      KeyValuePair<string, T> _current;
      int _bucketIndex, _slotIndex;

      public Enumerator(Entry[] buckets) {
        _buckets   = buckets;
        _current = default;
        _bucketIndex = 0;
        _slotIndex   = 0;
      }

      public bool MoveNext() {
        for (; _bucketIndex < _buckets.Length; _bucketIndex++) {
          ref var bucket = ref _buckets[_bucketIndex];
          if(_slotIndex < bucket.SlotsUsed) {
            _current = bucket.Slots[_slotIndex++];
            return true;
          }
        }

        return false;
      }
      public void Reset() {
        _current = default;
        _bucketIndex = 0;
        _slotIndex   = 0;
      }
      public KeyValuePair<string, T> Current => _current;

      object? IEnumerator.Current => Current;

      public void Dispose() {}
    }

    public void Add(KeyValuePair<string, T> item) {
      Add(item.Key, item.Value);
    }
    public void Clear() {
      throw new NotImplementedException();
    }
    public bool Contains(KeyValuePair<string, T> item) {
      throw new NotImplementedException();
    }
    public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) {
      throw new NotImplementedException();
    }
    public bool Remove(KeyValuePair<string, T> item) {
      throw new NotImplementedException();
    }
    public int Count { get; }
    public bool IsReadOnly { get; }
    public void Add(string key, T value) {
      if(_bucketsUsed > _buckets.Length * 0.75) {
        Resize();
      }

      var index = Mod(key.GetHashCode(), _buckets.Length);
      _buckets[index].Add(key, value);

      _bucketsUsed++;
    }

    int Mod(int a, int b) {
      return ((a % b) + b) % b;
    }

    private void Resize() {
      var oldSlots = _buckets;
      _buckets = new Entry[_buckets.Length * 2];
      for (int i = 0; i < _buckets.Length; i++) {
        _buckets[i] = new Entry(initialSize: 2);
      }

      for (var i = 0; i < oldSlots.Length; i++) {
        for (int j = 0; j < oldSlots[i].SlotsUsed; j++) {
          Add(oldSlots[i].Slots[j]);
        }
      }
    }

    public bool ContainsKey(string key) {
      throw new NotImplementedException();
    }
    public bool ContainsValue(T value) {
      foreach (var bucket in _buckets) {
        for (int i = 0; i < bucket.SlotsUsed; i++) {
          if(bucket.Slots[i].Value.Equals(value))
            return true;
        }
      }

      return false;
    }
    public bool Remove(string key) {
      throw new NotImplementedException();
    }
    public bool TryGetValue(string key, out T value) {
      return TryGetValue((ReadOnlySpan<char>)key, out value);
    }
    public bool TryGetValue(ReadOnlySpan<char> key, out T value) {
      var index = Mod(string.GetHashCode(key), _buckets.Length);
      return _buckets[index].Find(key, out value);
    }

    public T this[string key] {
      get => throw new NotImplementedException();
      set => throw new NotImplementedException();
    }

    public ICollection<string> Keys { get; }
    public ICollection<T> Values { get; }

    struct Entry {
      public KeyValuePair<string, T>[] Slots;
      public int                       SlotsUsed;

      public Entry(int initialSize = 2) {
        Slots     = new KeyValuePair<string, T>[initialSize];
        SlotsUsed = 0;
      }

      public void Add(string key, T value) {
        if(SlotsUsed == Slots.Length) {
          Resize();
        }

        Slots[SlotsUsed] = new KeyValuePair<string, T>(key, value);
        SlotsUsed++;
      }
      
      public bool Find(ReadOnlySpan<char> key, out T val) {
        for (int i = 0; i < SlotsUsed; i++) {
          ref var e = ref Slots[i];
          if(MemoryExtensions.Equals(e.Key, key, StringComparison.Ordinal)) {
            val = e.Value;
            return true;
          }
        }

        val = default;
        return false;
      }

      private void Resize() {
        Array.Resize(ref Slots, Slots.Length * 2);
      }

      public override string ToString() {
        return $"Bucket [{SlotsUsed:D2} / {Slots.Length:D2}]";
      }
    }
  }
}
