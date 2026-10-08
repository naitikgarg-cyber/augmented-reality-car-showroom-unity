using System;

namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// A set of flags that represent features available in ARCore.
    /// </summary>
    [Flags]
    public enum ARCoreFeatures
    {
        /// <summary>
        /// No features are selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// ARCore Cloud Anchors
        /// </summary>
        CloudAnchors = 1 << 0,
    }
}
