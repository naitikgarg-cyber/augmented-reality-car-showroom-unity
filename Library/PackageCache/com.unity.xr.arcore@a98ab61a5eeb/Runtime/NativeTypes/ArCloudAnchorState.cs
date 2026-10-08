namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// Result of a Cloud Anchor hosting or resolving operation.
    /// Refer to Google's Cloud Anchors developer guide for more information: https://developers.google.com/ar/reference/c/group/ar-anchor#arcloudanchorstate
    /// </summary>
    public enum ArCloudAnchorState
    {
        /// <summary>
        /// Not a valid value for a Cloud Anchor operation.
        /// </summary>
        AR_CLOUD_ANCHOR_STATE_NONE = 0,

        /// <summary>
        /// A hosting/resolving task for the anchor is in progress.
        /// Once the task completes in the background, the anchor will get a new cloud state after the next ArSession_update call.
        /// </summary>
        AR_CLOUD_ANCHOR_STATE_TASK_IN_PROGRESS = 1,

        /// <summary>
        /// A hosting/resolving task for this anchor completed successfully.
        /// </summary>
        AR_CLOUD_ANCHOR_STATE_SUCCESS = 2,

        /// <summary>
        /// A hosting/resolving task for this anchor finished with an internal error.
        /// The app should not attempt to recover from this error.
        /// </summary>
        AR_CLOUD_ANCHOR_STATE_ERROR_INTERNAL = -1,

        /// <summary>
        /// The authorization provided by the application is not valid.
        /// - The Google Cloud project may not have enabled the ARCore API.
        /// - It may fail if the operation you are trying to perform is not allowed.
        /// - When using API key authentication, this will happen if the API key in the manifest is invalid, unauthorized or missing.
        ///     It may also fail if the API key is restricted to a set of apps not including the current one.
        /// - When using keyless authentication, this will happen if the developer fails to create OAuth client.
        ///     It may also fail if Google Play Services isn't installed, is too old, or is malfunctioning for some reason (e.g. services killed due to memory pressure).
        /// </summary>
        AR_CLOUD_ANCHOR_STATE_ERROR_NOT_AUTHORIZED = -2,

        /// <summary>
        /// The application has exhausted the request quota allotted to the given API key.
        /// The developer should request additional quota for the ARCore API for their API key from the Google Developers Console.
        /// </summary>
        AR_CLOUD_ANCHOR_STATE_ERROR_RESOURCE_EXHAUSTED = -4,

        /// <summary>
        /// Hosting failed, because the server could not successfully process the dataset for the given anchor.
        /// The developer should try again after the device has gathered more data from the environment.
        /// </summary>
        AR_CLOUD_ANCHOR_STATE_ERROR_HOSTING_DATASET_PROCESSING_FAILED = -5,

        /// <summary>
        /// Resolving failed, because the ARCore Cloud Anchor service could not find the provided Cloud Anchor ID.
        /// </summary>
        AR_CLOUD_ANCHOR_STATE_ERROR_CLOUD_ID_NOT_FOUND = -6,

        /// <summary>
        /// The Cloud Anchor could not be resolved because the SDK version used to resolve the anchor is older than and incompatible with the version used to host it.
        /// </summary>
        AR_CLOUD_ANCHOR_STATE_ERROR_RESOLVING_SDK_VERSION_TOO_OLD = -8,

        /// <summary>
        /// The Cloud Anchor could not be resolved because the SDK version used to resolve the anchor is newer than and incompatible with the version used to host it.
        /// </summary>
        AR_CLOUD_ANCHOR_STATE_ERROR_RESOLVING_SDK_VERSION_TOO_NEW = -9,

        /// <summary>
        /// The ARCore Cloud Anchor service was unreachable.
        /// This can happen for a number of reasons. The device might be in airplane mode or does not have a working internet connection.
        /// The request sent to the server might have timed out with no response, or there might be a bad network connection, DNS unavailability, firewall issues, or anything else that might affect the device's ability to connect to the ARCore Cloud Anchor service.
        /// </summary>
        AR_CLOUD_ANCHOR_STATE_ERROR_HOSTING_SERVICE_UNAVAILABLE = -10
    }
}
