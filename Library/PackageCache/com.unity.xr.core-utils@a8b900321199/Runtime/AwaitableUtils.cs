#if UNITY_6000_0_OR_NEWER
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Utils to help with the `Awaitable&lt;T&gt;` class in Unity 6.0 or newer.
    /// </summary>
    /// <typeparam name="T">The awaited result type.</typeparam>
    public static class AwaitableUtils<T>
    {
        static AwaitableCompletionSource<T> s_CompletionSource = new();

        /// <summary>
        /// An `Awaitable` equivalent to C#'s `Task.FromResult`.
        /// </summary>
        /// <remarks>
        /// This method isn't thread-safe.
        /// </remarks>
        public static Awaitable<T> FromResult(T result)
        {
            var awaitable = s_CompletionSource.Awaitable;
            s_CompletionSource.SetResult(result);
            s_CompletionSource.Reset();
            return awaitable;
        }
    }
}
#endif
