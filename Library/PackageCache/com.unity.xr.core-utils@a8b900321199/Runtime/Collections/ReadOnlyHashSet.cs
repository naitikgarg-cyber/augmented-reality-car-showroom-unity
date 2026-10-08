using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using UnityEngine.Assertions;

namespace Unity.XR.CoreUtils.Collections
{
    /// <summary>
    /// Wraps a <see cref="HashSet{T}"/> to provide a read-only view of its memory without copying any elements.
    /// It is preferable to use this collection in API designs instead of `IReadOnlyCollection` because
    /// <see cref="GetEnumerator"/> returns a value-type enumerator and does not perform any heap allocations.
    /// </summary>
    /// <remarks>
    /// You are responsible to ensure that the underlying `HashSet` used by this class remains in scope for the
    /// lifetime of this class. If the `HashSet` is destroyed, methods in this class will throw exceptions.
    ///
    /// This collection is not thread-safe.
    /// </remarks>
    /// <typeparam name="T">The element type.</typeparam>
    public class ReadOnlyHashSet<T> : IReadOnlySet<T>, IEquatable<ReadOnlyHashSet<T>>
    {
        static ReadOnlyHashSet<T> s_EmptySet;

        readonly HashSet<T> m_Set;

        /// <summary>
        /// The number of elements in the read-only set.
        /// </summary>
        /// <value>The number of elements.</value>
        public int Count => m_Set.Count;

        /// <summary>
        /// Constructs a new instance of this class that is a read-only wrapper around the specified set.
        /// </summary>
        /// <param name="set">The set to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="set"/> is `null`.</exception>
        public ReadOnlyHashSet(HashSet<T> set)
        {
            m_Set = set ?? throw new ArgumentNullException(nameof(set));
        }

        /// <summary>
        /// Returns an empty read-only set with the specified type argument.
        /// </summary>
        /// <returns>The empty read-only set.</returns>
        /// <remarks>
        /// This method caches an empty read-only set that you can re-use throughout the life cycle of your app.
        /// </remarks>
        public static ReadOnlyHashSet<T> Empty()
        {
            s_EmptySet ??= new ReadOnlyHashSet<T>(new HashSet<T>(0));
            return s_EmptySet;
        }

        /// <inheritdoc/>
        public bool Contains(T element) => m_Set.Contains(element);

        /// <summary>
        /// Returns an enumerator that iterates through the read-only set.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public HashSet<T>.Enumerator GetEnumerator() => m_Set.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the read-only set.
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > This implementation performs a boxing operation and should be avoided.
        /// > Use the public <see cref="GetEnumerator"/> overload instead.
        /// </remarks>
        /// <returns>The boxed enumerator.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the read-only set.
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
        /// Two `ReadOnlyHashSet` instances compare equal if they are read-only views of the same `HashSet` instance.
        /// </remarks>
        public bool Equals(ReadOnlyHashSet<T> other)
        {
            if (other is null)
                return false;
            return ReferenceEquals(this, other) || Equals(m_Set, other.m_Set);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>`true` if the current object is equal to the `other` parameter. Otherwise, `false`.</returns>
        /// <remarks>
        /// Two `ReadOnlyHashSet` instances compare equal if they are read-only views of the same `HashSet` instance.
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((ReadOnlyHashSet<T>)obj);
        }

        /// <summary>
        /// Get a hash code for this object.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return m_Set != null ? m_Set.GetHashCode() : 0;
        }

        /// <summary>
        /// Copies the elements of the set to an array.
        /// </summary>
        /// <param name="array">The array.</param>
        public void CopyTo(T[] array)
        {
            Assert.IsNotNull(array);
            Assert.IsTrue(array.Length >= m_Set.Count, "Array capacity is insufficient to copy this set.");

            m_Set.CopyTo(array);
        }

        /// <summary>
        /// Copies the elements of the set to a list.
        /// </summary>
        /// <param name="list">The list.</param>
        public void CopyTo(List<T> list)
        {
            Assert.IsNotNull(list);
            list.AddRange(m_Set);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            foreach (var element in m_Set)
            {
                sb.AppendLine(element == null ? "  null," : $"  {element.ToString()},");
            }
            sb.Append("}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Extension methods for <see cref="ReadOnlyHashSet{T}"/>, providing additional functionality for more
    /// restrictive constraints of type `T`.
    /// </summary>
    public static class ReadOnlyHashSetExtensions
    {
        /// <summary>
        /// Copies the elements of the set to a native array.
        /// </summary>
        /// <param name="set">This instance.</param>
        /// <param name="array">The array.</param>
        /// <typeparam name="T">The element type.</typeparam>
        public static void CopyTo<T>(this ReadOnlyHashSet<T> set, NativeArray<T> array) where T : struct
        {
            Assert.IsTrue(array.Length >= set.Count, "Array capacity is insufficient to copy this set.");

            var i = 0;
            foreach (var element in set)
            {
                array[i] = element;
                ++i;
            }
        }
    }
}
