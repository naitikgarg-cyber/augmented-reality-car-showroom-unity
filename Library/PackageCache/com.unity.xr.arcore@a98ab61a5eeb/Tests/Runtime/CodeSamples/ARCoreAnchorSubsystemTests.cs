using System.Threading;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARCore.CodeSamples.Tests
{
    /// <summary>
    /// Use this class to write sample code for <see cref="ARCoreAnchorSubsystem"/> to be rendered to the documentation manual.
    /// </summary>
    class ARCoreAnchorSubsystemTests
    {
        #region CheckPersistentAnchorsSupport
        void CheckPersistentAnchorsSupport(ARAnchorManager manager)
        {
            var descriptor = manager.descriptor;
            if (descriptor.supportsSaveAnchor && descriptor.supportsLoadAnchor)
            {
                // Save and load anchors are both supported and enabled.
            }
        }
        #endregion

        #region CheckQualityAndSaveAnchor
        void CheckQualityAndSaveAnchor(ARAnchorManager manager, ARAnchor anchor)
        {
            if (manager.subsystem is ARCoreAnchorSubsystem arCoreAnchorSubsystem)
            {
                var quality = ArFeatureMapQuality.AR_FEATURE_MAP_QUALITY_SUFFICIENT;

                XRResultStatus resultStatus = arCoreAnchorSubsystem.EstimateFeatureMapQualityForHosting(anchor.trackableId, ref quality);

                if (!resultStatus.IsSuccess())
                {
                    // An error occurred while attempting to check the feature map quality.
                    return;
                }

                if (quality == ArFeatureMapQuality.AR_FEATURE_MAP_QUALITY_INSUFFICIENT)
                {
                    // Anchor map quality is insufficient. Save the anchor when the quality improves.
                    return;
                }
            }

            // Proceed with saving the anchor
        }
        #endregion

        #region TrySaveAnchorWithLifespanAsync
        async void TrySaveAnchorWithLifespanAsync(ARAnchorManager manager, ARAnchor anchor)
        {
            if (manager.subsystem is ARCoreAnchorSubsystem arCoreAnchorSubsystem)
            {
                // Save the anchor for 180 days
                var result = await arCoreAnchorSubsystem.TrySaveAnchorWithLifespanAsync(anchor.trackableId, 180);

                if (result.status.IsError())
                {
                    // handle error
                    return;
                }

                // Save this value, then use it as an input parameter
                // to TryLoadAnchorAsync or TryEraseAnchorAsync
                SerializableGuid guid = result.value;
            }
        }
        #endregion

        #region LoadAndCheckNativeStatusCode
        async void LoadAndCheckNativeStatusCode(ARAnchorManager manager, SerializableGuid anchorId)
        {
            var result = await manager.TryLoadAnchorAsync(anchorId);

            // Interpreting the status code
            var cloudAnchorState = (ArCloudAnchorState)result.status.nativeStatusCode;
            switch (cloudAnchorState)
            {
                case ArCloudAnchorState.AR_CLOUD_ANCHOR_STATE_SUCCESS:
                    // Load was successful
                    break;
                case ArCloudAnchorState.AR_CLOUD_ANCHOR_STATE_ERROR_NOT_AUTHORIZED:
                    // Authorization to Google Cloud failed.
                    // As a developer, ensure that you have a Google Cloud project and that you have
                    // authorized your application with an API Key or with Keyless authorization.
                    break;
                case ArCloudAnchorState.AR_CLOUD_ANCHOR_STATE_ERROR_RESOURCE_EXHAUSTED:
                    // Google Cloud resource exhausted. Ensure that your Google Cloud project has enough
                    // resources to support your application's needs.
                    break;
                case ArCloudAnchorState.AR_CLOUD_ANCHOR_STATE_ERROR_CLOUD_ID_NOT_FOUND:
                    // Anchor was not found. You may have specified the wrong anchor ID, or the anchor
                    // may have expired on the server.
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region CancelAndCheckNativeStatusCode
        async void CancelAndCheckNativeStatusCode(ARAnchorManager manager, SerializableGuid anchorId)
        {
            // Create a CancellationTokenSource to serve our CancellationToken
            var cts = new CancellationTokenSource();

            // Try to load an anchor
            var awaitable = manager.TryLoadAnchorAsync(anchorId, cts.Token);

            // Cancel the async operation before it completes
            cts.Cancel();

            // Wait for and obtain the result from TryLoadAnchorAsync
            var result = await awaitable;

            // Interpreting the status code.
            // The nativeStatusCode is an ArFutureState because the operation was cancelled.
            var futureState = (ArFutureState)result.status.nativeStatusCode;
            switch (futureState)
            {
                case ArFutureState.AR_FUTURE_STATE_DONE:
                    // The operation is complete and the result is available.
                    break;
                case ArFutureState.AR_FUTURE_STATE_CANCELLED:
                    // The operation has been cancelled.
                    break;
                case ArFutureState.AR_FUTURE_STATE_PENDING:
                    // The operation is still pending.
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
