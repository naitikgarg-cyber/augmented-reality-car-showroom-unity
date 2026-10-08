namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// Describes the quality of the visual features seen by ARCore in the preceding few seconds and visible from a desired camera ArPose.
    /// A higher quality indicates a Cloud Anchor hosted at the current time with the current set of recently seen features will generally be easier to resolve more accurately.
    /// Refer to Google's Cloud Anchors developer guide for more information: https://developers.google.com/ar/reference/c/group/ar-anchor#arfeaturemapquality
    /// </summary>
    public enum ArFeatureMapQuality
    {
        /// <summary>
        /// The quality of features seen from the pose in the preceding seconds is low.
        /// This state indicates that ARCore will likely have more difficulty resolving the Cloud Anchor.
        /// Encourage the user to move the device, so that the desired position of the Cloud Anchor to be hosted is seen from different angles.
        /// </summary>
        AR_FEATURE_MAP_QUALITY_INSUFFICIENT = 0,

        /// <summary>
        /// The quality of features seen from the pose in the preceding few seconds is likely sufficient for ARCore to successfully resolve a Cloud Anchor, although the accuracy of the resolved pose will likely be reduced.
        /// Encourage the user to move the device, so that the desired position of the Cloud Anchor to be hosted is seen from different angles.
        /// </summary>
        AR_FEATURE_MAP_QUALITY_SUFFICIENT = 1,

        /// <summary>
        /// The quality of features seen from the pose in the preceding few seconds is likely sufficient for ARCore to successfully resolve a Cloud Anchor with a high degree of accuracy.
        /// </summary>
        AR_FEATURE_MAP_QUALITY_GOOD = 2,
    }
}
