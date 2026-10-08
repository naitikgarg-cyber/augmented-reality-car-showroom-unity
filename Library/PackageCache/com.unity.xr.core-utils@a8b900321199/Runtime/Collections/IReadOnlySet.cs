using System.Collections.Generic;

namespace Unity.XR.CoreUtils.Collections
{
    /// <summary>
    /// A subset of the `IReadOnlySet` interface added in .NET 5. Members that depend on the `IEnumerable` interface
    /// are omitted because of performance implications of boxing the enumerator.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public interface IReadOnlySet<T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// Determines if the set contains a specific element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>`true` if the set contains the element. Otherwise, `false`.</returns>
        bool Contains(T element);
    }
}
