using System.Collections;

namespace Cavok.Parsing;

// The list type behind every list property of the public records: an immutable copy with structural equality
// and a readable ToString, so records holding lists compare by content (Metar.Parse(s) == Metar.Parse(s)).
// It wraps a private array, so a caller cannot downcast a record's list to T[] and change it.
internal sealed class EquatableArray<T> : IReadOnlyList<T>, IEquatable<EquatableArray<T>>
{
    public static readonly EquatableArray<T> Empty = new EquatableArray<T>(Array.Empty<T>());

    private readonly T[] _items;

    private EquatableArray(T[] items)
    {
        _items = items;
    }

    public int Count => _items.Length;

    public T this[int index] => _items[index];

    public bool Equals(EquatableArray<T>? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null || other._items.Length != _items.Length)
        {
            return false;
        }

        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < _items.Length; i++)
        {
            if (!comparer.Equals(_items[i], other._items[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as EquatableArray<T>);

    // netstandard2.0 has no HashCode type, so the item hashes are combined by hand.
    public override int GetHashCode()
    {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        int hash = 17;
        foreach (T item in _items)
        {
            hash = unchecked((hash * 31) + (item is null ? 0 : comparer.GetHashCode(item)));
        }

        return hash;
    }

    public override string ToString() => "[" + string.Join(", ", _items) + "]";

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Copies the items; an EquatableArray is returned as is (it is immutable) and null becomes the empty list.
    internal static EquatableArray<T> From(IEnumerable<T>? items)
    {
        if (items is EquatableArray<T> array)
        {
            return array;
        }

        T[] copy = items?.ToArray() ?? Array.Empty<T>();
        return copy.Length == 0 ? Empty : new EquatableArray<T>(copy);
    }
}

// Type inference for EquatableArray<T>.From.
internal static class EquatableArray
{
    public static EquatableArray<T> From<T>(IEnumerable<T>? items) => EquatableArray<T>.From(items);
}
