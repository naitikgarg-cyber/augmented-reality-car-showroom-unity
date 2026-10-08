#if UNITY_6000_0_OR_NEWER
using NUnit.Framework;

namespace Unity.XR.CoreUtils.Editor.Tests
{
    class AwaitableUtilsTests
    {
        [Test]
        public void FromResult_ReturnsResult()
        {
            var awaitable = AwaitableUtils<int>.FromResult(7);
            Assert.AreEqual(7, awaitable.GetAwaiter().GetResult());
        }
    }
}
#endif
