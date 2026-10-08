using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;

namespace Unity.XR.CoreUtils.Collections
{
    /// <summary>
    /// Wraps a list, array, or `NativeArray` to provide a read-only view of some or all elements. Elements are not
    /// copied, so if the underlying collection changes, the `ReadOnlyStructSpan` will see the updated elements.
    /// </summary>
    /// <remarks>
    /// It is preferable to use this collection in API designs instead of `IReadOnlyCollection` because
    /// <see cref="GetEnumerator"/> returns a value-type enumerator and does not perform any heap allocations.
    ///
    /// This collection is not thread-safe.
    /// </remarks>
    /// <typeparam name="T">The element type. Must be a struct.</typeparam>
    public struct ReadOnlyStructSpan<T> : IReadOnlyList<T>, IEquatable<ReadOnlyStructSpan<T>> where T : struct
    {
        IReadOnlyList<T> m_List;
        NativeArray<T> m_NativeArray;

        /// <summary>
        /// The inclusive start index of a slice of the collection.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// The exclusive end index of a slice of the collection.
        /// </summary>
        public int End { get; }

        /// <summary>
        /// The number of elements in the read-only span.
        /// </summary>
        /// <value>The number of elements.</value>
        public int Count { get; }

        /// <summary>
        /// Returns the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index.</param>
        public T this[int index]
        {
            get
            {
                index += Start;
                if (index < Start || index >= End)
                    throw new ArgumentOutOfRangeException();

                return m_List == null ? m_NativeArray[index] : m_List[index];
            }
        }

        /// <summary>
        /// Constructs a new instance of this struct that is a read-only wrapper around the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/>.</exception>
        public ReadOnlyStructSpan(IReadOnlyList<T> list)
        {
            m_List = list ?? throw new ArgumentNullException(nameof(list));
            m_NativeArray = default;
            Start = 0;
            End = list.Count;
            Count = End - Start;
        }

        /// <summary>
        /// Constructs a new instance of this struct that is a read-only wrapper around a slice of the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        /// <param name="start">The zero-based index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown if
        /// start or length are outside the bounds of the list.</exception>
        public ReadOnlyStructSpan(IReadOnlyList<T> list, int start, int length)
        {
            m_List = list ?? throw new ArgumentNullException(nameof(list));

            if (start < 0 || start + length > list.Count)
                throw new ArgumentOutOfRangeException(nameof(list), "start and length must be within list bounds");

            m_List = list;
            m_NativeArray = default;
            Start = start;
            End = start + length;
            Count = End - Start;
        }

        /// <summary>
        /// Constructs a new instance of this struct that is a read-only wrapper around the specified NativeArray.
        /// </summary>
        /// <param name="nativeArray">The NativeArray to wrap.</param>
        public ReadOnlyStructSpan(NativeArray<T> nativeArray)
        {
            m_List = null;
            m_NativeArray = nativeArray;
            Start = 0;
            End = nativeArray.Length;
            Count = End - Start;
        }

        /// <summary>
        /// Constructs a new instance of this struct that is a read-only wrapper around a slice of the specified
        /// `NativeArray`.
        /// </summary>
        /// <param name="nativeArray">The `NativeArray` to wrap.</param>
        /// <param name="start">The zero-based index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown if <paramref name="start"/> or
        /// <paramref name="length"/> are outside the bounds of the `NativeArray`.</exception>
        public ReadOnlyStructSpan(NativeArray<T> nativeArray, int start, int length)
        {
            if (start < 0 || start + length > nativeArray.Length)
                throw new ArgumentOutOfRangeException(nameof(nativeArray), "start and length must be within array bounds");

            m_List = null;
            m_NativeArray = nativeArray;
            Start = start;
            End = start + length;
            Count = End - Start;
        }

        /// <summary>
        /// Create a new instance using a subset of the current span.
        ///
        /// Indices are mapped within the current instance's span, so index `0` refers to the start index of this
        /// span, etc.
        /// </summary>
        /// <param name="start">The zero-based index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <returns>A slice of the current instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown if <paramref name="start"/> or
        /// <paramref name="length"/> are outside the bounds of the current instance.</exception>
        public ReadOnlyStructSpan<T> Slice(int start, int length)
        {
            var newStart = Start + start;
            if (newStart < Start || newStart + length > End)
                throw new ArgumentOutOfRangeException();

            return m_List == null
                ? new ReadOnlyStructSpan<T>(m_NativeArray, Start + start, length)
                : new ReadOnlyStructSpan<T>(m_List, Start + start, length);
        }

        /// <summary>
        /// Returns an empty instance with the specified type argument.
        /// </summary>
        /// <returns>The empty read-only span.</returns>
        public static ReadOnlyStructSpan<T> Empty() => new();

        /// <summary>
        /// Returns an enumerator that iterates through the read-only span.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        /// Returns an enumerator that iterates through the read-only span.
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > This implementation performs a boxing operation and should be avoided.
        /// > Use the public <see cref="GetEnumerator"/> overload instead.
        /// </remarks>
        /// <returns>The boxed enumerator.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the read-only span.
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > This implementation performs a boxing operation and should be avoided.
        /// > Use the public <see cref="GetEnumerator"/> overload instead.
        /// </remarks>
        /// <returns>The boxed enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>`true` if the current object is equal to the `other` parameter. Otherwise, `false`.</returns>
        /// <remarks>
        /// Two instances compare equal if they are read-only views of the same collection with the same start and end
        /// indices.
        /// </remarks>
        public bool Equals(ReadOnlyStructSpan<T> other)
        {
            return ReferenceEquals(m_List, other.m_List)
                && m_NativeArray == other.m_NativeArray
                && Start == other.Start
                && End == other.End;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object, which must be of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>`true` if the current object is equal to the `other` parameter. Otherwise, `false`.</returns>
        public override bool Equals(object obj)
            => obj is ReadOnlyStructSpan<T> other && Equals(other);

        /// <summary>
        /// Returns `true` if objects are equal by <see cref="Equals(Unity.XR.CoreUtils.Collections.ReadOnlyStructSpan{T})"/>.
        /// Otherwise, `false`.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`true` if objects are equal. Otherwise, `false`.</returns>
        public static bool operator ==(ReadOnlyStructSpan<T> lhs, ReadOnlyStructSpan<T> rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Returns `false` if objects are equal by <see cref="Equals(Unity.XR.CoreUtils.Collections.ReadOnlyStructSpan{T})"/>.
        /// Otherwise, `true`.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`false` if objects are equal. Otherwise, `true`.</returns>
        public static bool operator !=(ReadOnlyStructSpan<T> lhs, ReadOnlyStructSpan<T> rhs) => !(lhs == rhs);

        /// <summary>
        /// Get a hash code for this object.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine(m_List, m_NativeArray, Start, End);

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            for (var i = Start; i < End; i++)
            {
                sb.AppendLine($"  {this[i].ToString()},");
            }
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// Provides an enumerator for the elements of `ReadOnlyStructSpan`.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            ReadOnlyStructSpan<T> m_Span;
            int m_CurrentIndex;

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">Thrown if the current position is outside the bounds of
            /// the ReadOnlyStructSpan.</exception>
            public T Current
            {
                get
                {
                    if (m_CurrentIndex < 0 || m_CurrentIndex >= m_Span.Count)
                        throw new ArgumentOutOfRangeException();

                    return m_Span[m_CurrentIndex];
                }
            }

            object IEnumerator.Current => Current;

            internal Enumerator(ReadOnlyStructSpan<T> span)
            {
                m_Span = span;
                m_CurrentIndex = -1;
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>`true` if the next position is within the bounds of the collection. Otherwise, `false`.</returns>
            public bool MoveNext()
            {
                m_CurrentIndex += 1;
                return m_CurrentIndex < m_Span.Count;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset() => m_CurrentIndex = -1;

            void IDisposable.Dispose() { }
        }
    }
}
