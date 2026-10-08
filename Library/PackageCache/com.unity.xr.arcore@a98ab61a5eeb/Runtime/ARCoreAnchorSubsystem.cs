using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// The ARCore implementation of the
    /// [XRAnchorSubsystem](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem).
    /// Do not create this directly. Use the
    /// [SubsystemManager](xref:UnityEngine.SubsystemManager)
    /// instead.
    /// </summary>
    [Preserve]
    public sealed class ARCoreAnchorSubsystem : XRAnchorSubsystem
    {
        class ARCoreProvider : Provider
        {
            const uint k_MaxLifespanApiKey = 1;

            const uint k_MaxLifespanKeyless = 365;

            static readonly Dictionary<TrackableId, AwaitableCompletionSource<Result<SerializableGuid>>> s_SaveAsyncPendingRequests = new();

            static readonly Dictionary<SerializableGuid, AwaitableCompletionSource<Result<XRAnchor>>> s_LoadAsyncPendingRequests = new();

            static readonly Pool.ObjectPool<AwaitableCompletionSource<Result<SerializableGuid>>> s_SaveAsyncCompletionSources = new(
                createFunc: () => new AwaitableCompletionSource<Result<SerializableGuid>>(),
                actionOnGet: null,
                actionOnRelease: null,
                actionOnDestroy: null,
                collectionCheck: false,
                defaultCapacity: 8,
                maxSize: 1024);

            static readonly Pool.ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>> s_LoadAsyncCompletionSources = new(
                createFunc: () => new AwaitableCompletionSource<Result<XRAnchor>>(),
                actionOnGet: null,
                actionOnRelease: null,
                actionOnDestroy: null,
                collectionCheck: false,
                defaultCapacity: 8,
                maxSize: 1024);

            protected override bool TryInitialize()
            {
                UnityARCore_anchors_create(s_SaveAsyncCallback, s_LoadAsyncCallback);
                return true;
            }

            public override void Start() => UnityARCore_anchors_start();

            public override void Stop() => UnityARCore_anchors_stop();

            public override void Destroy() => UnityARCore_anchors_onDestroy();

            public override unsafe TrackableChanges<XRAnchor> GetChanges(
                XRAnchor defaultAnchor,
                Allocator allocator)
            {
                int addedCount, updatedCount, removedCount, elementSize;
                void* addedPtr, updatedPtr, removedPtr;
                var context = UnityARCore_anchors_acquireChanges(
                    out addedPtr, out addedCount,
                    out updatedPtr, out updatedCount,
                    out removedPtr, out removedCount,
                    out elementSize);

                try
                {
                    return new TrackableChanges<XRAnchor>(
                        addedPtr, addedCount,
                        updatedPtr, updatedCount,
                        removedPtr, removedCount,
                        defaultAnchor, elementSize,
                        allocator);
                }
                finally
                {
                    UnityARCore_anchors_releaseChanges(context);
                }

            }

            public override bool TryAddAnchor(
                Pose pose,
                out XRAnchor anchor)
            {
                return UnityARCore_anchors_tryAdd(pose, out anchor);
            }

            public override bool TryAttachAnchor(
                TrackableId attachedToId,
                Pose pose,
                out XRAnchor anchor)
            {
                return UnityARCore_anchors_tryAttach(attachedToId, pose, out anchor);
            }

            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                return UnityARCore_anchors_tryRemove(anchorId);
            }

            internal XRResultStatus EstimateFeatureMapQualityForHosting(TrackableId anchorId, ref ArFeatureMapQuality quality)
            {
                return UnityARCore_anchors_estimateFeatureMapQualityForHosting(anchorId, ref quality);
            }

            public override Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(
                TrackableId anchorId, CancellationToken cancellationToken = default)
            {
                var usingKeyless = ARCoreRuntimeSettings.Instance.authorizationType == ARCoreRuntimeSettings.AuthorizationType.Keyless;
                var lifespan = usingKeyless ? k_MaxLifespanKeyless : k_MaxLifespanApiKey;
                return TrySaveAnchorWithLifespanAsync(anchorId, lifespan, cancellationToken);
            }

            internal Awaitable<Result<SerializableGuid>> TrySaveAnchorWithLifespanAsync(
                TrackableId anchorId, uint lifespan, CancellationToken cancellationToken = default)
            {
                if (lifespan == 0)
                {
                    throw new ArgumentException("Lifespan must be greater than 0");
                }

                var completionSource = s_SaveAsyncCompletionSources.Get();
                var wasAddedToMap = s_SaveAsyncPendingRequests.TryAdd(anchorId, completionSource);

                if (!wasAddedToMap)
                {
                    Debug.LogError($"Cannot save anchor with trackableId [{anchorId}] while saving for it is already in progress!");
                    var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.ValidationFailure);
                    var result = new Result<SerializableGuid>(resultStatus, default);
                    return AwaitableUtils<Result<SerializableGuid>>.FromResult(completionSource, result);
                }

                var usingKeyless = ARCoreRuntimeSettings.Instance.authorizationType == ARCoreRuntimeSettings.AuthorizationType.Keyless;
                if (usingKeyless && lifespan > k_MaxLifespanKeyless)
                {
                    Debug.LogWarning("ARCore anchor lifespan is too long. Using default lifespan.");
                    lifespan = k_MaxLifespanKeyless;
                }
                else if (!usingKeyless && lifespan > k_MaxLifespanApiKey)
                {
                    Debug.LogWarning("ARCore anchor lifespan is too long. Using default lifespan.");
                    lifespan = k_MaxLifespanApiKey;
                }

                var synchronousResultStatus = UnityARCore_anchors_trySaveAnchorAsync(anchorId, (int)lifespan);
                if (synchronousResultStatus.IsError())
                {
                    var result = new Result<SerializableGuid>(synchronousResultStatus, default);
                    return AwaitableUtils<Result<SerializableGuid>>.FromResult(completionSource, result);
                }

                cancellationToken.Register(() =>
                {
                    var resultStatus = UnityARCore_anchors_cancelSaveAnchor(anchorId);
                    if (!s_SaveAsyncPendingRequests.Remove(anchorId))
                    {
                        Debug.LogError($"An unknown error occurred during a system callback for {nameof(TrySaveAnchorAsync)}.");
                    }
                    completionSource.SetResult(new Result<SerializableGuid>(resultStatus, default));
                    completionSource.Reset();
                    s_SaveAsyncCompletionSources.Release(completionSource);
                });

                return completionSource.Awaitable;
            }

            public override Awaitable<Result<XRAnchor>> TryLoadAnchorAsync(
                SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
            {
                var completionSource = s_LoadAsyncCompletionSources.Get();
                var wasAddedToMap = s_LoadAsyncPendingRequests.TryAdd(savedAnchorGuid, completionSource);

                if (!wasAddedToMap)
                {
                    Debug.LogError($"Cannot load persistent anchor GUID [{savedAnchorGuid}] while loading for it is already in progress!");
                    var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.ValidationFailure);
                    var result = new Result<XRAnchor>(resultStatus, XRAnchor.defaultValue);
                    return AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, result);
                }

                var synchronousResultStatus = UnityARCore_anchors_tryLoadAnchorAsync(savedAnchorGuid);
                if (synchronousResultStatus.IsError())
                {
                    var result = new Result<XRAnchor>(synchronousResultStatus, XRAnchor.defaultValue);
                    return AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, result);
                }

                cancellationToken.Register(() =>
                {
                    var resultStatus = UnityARCore_anchors_cancelLoadAnchor(savedAnchorGuid);
                    if (!s_LoadAsyncPendingRequests.Remove(savedAnchorGuid))
                    {
                        Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryLoadAnchorAsync)}.");
                    }
                    completionSource.SetResult(new Result<XRAnchor>(resultStatus, default));
                    completionSource.Reset();
                    s_LoadAsyncCompletionSources.Release(completionSource);
                });

                return completionSource.Awaitable;
            }

            /// <summary>
            /// Function pointer marshalled to native API to call when <see cref="TrySaveAnchorAsync"/> is complete.
            /// </summary>
            static readonly IntPtr s_SaveAsyncCallback = Marshal.GetFunctionPointerForDelegate((SaveAsyncDelegate)OnSaveAsyncComplete);

            /// <summary>
            /// Function pointer marshalled to native API to call when <see cref="TryLoadAnchorAsync"/> is complete.
            /// </summary>
            static readonly IntPtr s_LoadAsyncCallback = Marshal.GetFunctionPointerForDelegate((LoadAsyncDelegate)OnLoadAsyncComplete);

            /// <summary>
            /// Delegate method type for <see cref="ARCoreProvider.s_SaveAsyncCallback"/>.
            /// </summary>
            delegate void SaveAsyncDelegate(TrackableId anchorId, TrackableId cloudAnchorId, XRResultStatus resultStatus);

            /// <summary>
            /// Delegate method type for <see cref="ARCoreProvider.s_LoadAsyncCallback"/>.
            /// </summary>
            delegate void LoadAsyncDelegate(XRAnchor anchor, TrackableId cloudAnchorId, XRResultStatus resultStatus);

            [MonoPInvokeCallback(typeof(SaveAsyncDelegate))]
            static async void OnSaveAsyncComplete(TrackableId anchorId, TrackableId cloudAnchorId, XRResultStatus resultStatus)
            {
                await Awaitable.MainThreadAsync();

                if (!s_SaveAsyncPendingRequests.Remove(anchorId, out var completionSource))
                {
                    Debug.LogError($"An unknown error occurred during a system callback for {nameof(TrySaveAnchorAsync)}.");
                }

                completionSource.SetResult(new Result<SerializableGuid>(resultStatus, cloudAnchorId));
                completionSource.Reset();
                s_SaveAsyncCompletionSources.Release(completionSource);
            }

            [MonoPInvokeCallback(typeof(LoadAsyncDelegate))]
            static void OnLoadAsyncComplete(XRAnchor anchor, TrackableId cloudAnchorId, XRResultStatus resultStatus)
            {
                if (!s_LoadAsyncPendingRequests.Remove(cloudAnchorId, out var completionSource))
                {
                    Debug.LogError($"An unknown error occurred during a system callback for {nameof(TryLoadAnchorAsync)}.");
                }

                completionSource.SetResult(new Result<XRAnchor>(resultStatus, anchor));
                completionSource.Reset();
                s_LoadAsyncCompletionSources.Release(completionSource);
            }

            [DllImport(Constants.k_LibraryName)]
            static extern void UnityARCore_anchors_create(IntPtr saveCallback, IntPtr loadCallback);

            [DllImport(Constants.k_LibraryName)]
            static extern void UnityARCore_anchors_start();

            [DllImport(Constants.k_LibraryName)]
            static extern void UnityARCore_anchors_stop();

            [DllImport(Constants.k_LibraryName)]
            static extern void UnityARCore_anchors_onDestroy();

            [DllImport(Constants.k_LibraryName)]
            static extern unsafe void* UnityARCore_anchors_acquireChanges(
                out void* addedPtr, out int addedCount,
                out void* updatedPtr, out int updatedCount,
                out void* removedPtr, out int removedCount,
                out int elementSize);

            [DllImport(Constants.k_LibraryName)]
            static extern unsafe void UnityARCore_anchors_releaseChanges(
                void* changes);

            [DllImport(Constants.k_LibraryName)]
            static extern bool UnityARCore_anchors_tryAdd(
                Pose pose,
                out XRAnchor anchor);

            [DllImport(Constants.k_LibraryName)]
            static extern bool UnityARCore_anchors_tryAttach(
                TrackableId trackableToAffix,
                Pose pose,
                out XRAnchor anchor);

            [DllImport(Constants.k_LibraryName)]
            static extern bool UnityARCore_anchors_tryRemove(TrackableId anchorId);

            [DllImport(Constants.k_LibraryName)]
            static extern XRResultStatus UnityARCore_anchors_estimateFeatureMapQualityForHosting(TrackableId anchorId, ref ArFeatureMapQuality quality);

            [DllImport(Constants.k_LibraryName)]
            static extern XRResultStatus UnityARCore_anchors_trySaveAnchorAsync(TrackableId anchorId, int lifespan);

            [DllImport(Constants.k_LibraryName)]
            static extern XRResultStatus UnityARCore_anchors_tryLoadAnchorAsync(TrackableId anchorId);

            [DllImport(Constants.k_LibraryName)]
            static extern XRResultStatus UnityARCore_anchors_cancelLoadAnchor(TrackableId anchorId);

            [DllImport(Constants.k_LibraryName)]
            static extern XRResultStatus UnityARCore_anchors_cancelSaveAnchor(TrackableId anchorId);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            if (!Api.platformAndroid || !Api.loaderPresent)
                return;

            // If cloud anchors are not enabled, don't allow users to access the save, load, and cancel features
            var cloudAnchorsEnabled = ARCoreRuntimeSettings.Instance.enableCloudAnchors;

            var cinfo = new XRAnchorSubsystemDescriptor.Cinfo
            {
                id = "ARCore-Anchor",
                providerType = typeof(ARCoreAnchorSubsystem.ARCoreProvider),
                subsystemTypeOverride = typeof(ARCoreAnchorSubsystem),
                supportsTrackableAttachments = true,
                supportsSynchronousAdd = true,
                supportsSaveAnchor = cloudAnchorsEnabled,
                supportsLoadAnchor = cloudAnchorsEnabled,
                supportsEraseAnchor = false,
                supportsGetSavedAnchorIds = false,
                supportsAsyncCancellation = true,
            };

            XRAnchorSubsystemDescriptor.Register(cinfo);
        }

        /// <summary>
        /// Returns the quality of feature points seen in the preceding few seconds from a given anchor.
        /// Refer to ARCore docs for more information:
        /// https://developers.google.com/ar/develop/c/cloud-anchors/developer-guide#check_the_mapping_quality_of_feature_points
        /// </summary>
        /// <param name="anchorId">The ID of the anchor</param>
        /// <param name="quality">The feature map quality of the anchor</param>
        /// <returns>The result status</returns>
        public XRResultStatus EstimateFeatureMapQualityForHosting(TrackableId anchorId, ref ArFeatureMapQuality quality)
        {
            var p = (ARCoreProvider)provider;
            if (provider != null)
            {
                return p.EstimateFeatureMapQualityForHosting(anchorId, ref quality);
            }

            Debug.LogError($"{nameof(ARCoreProvider)} not found. Unable to estimate feature map quality.");
            return new XRResultStatus(XRResultStatus.StatusCode.ValidationFailure);
        }

        /// <summary>
        /// Attempts to persistently save the given anchor so that it can be loaded in a future AR session.
        /// This method takes a lifespan parameter that indicates how long the anchor should persist for.
        /// The platform may have a maximum lifespan that cannot be exceeded.
        /// </summary>
        /// <param name="anchorId">The TrackableId of the anchor to save.</param>
        /// <param name="lifespan">The lifespan (in days) of the anchor.</param>
        /// <param name="cancellationToken">An optional `CancellationToken` that you can use to cancel the operation
        /// in progress if the loaded provider <see cref="XRAnchorSubsystemDescriptor.supportsAsyncCancellation"/>.</param>
        /// <returns>The result of the async operation, containing a new persistent anchor GUID if the operation
        /// succeeded. You are responsible to <see langword="await"/> this result.</returns>
        /// <seealso cref="XRAnchorSubsystemDescriptor.supportsSaveAnchor"/>
        public Awaitable<Result<SerializableGuid>> TrySaveAnchorWithLifespanAsync(
            TrackableId anchorId, uint lifespan, CancellationToken cancellationToken = default)
        {
            var p = (ARCoreProvider)provider;
            return p.TrySaveAnchorWithLifespanAsync(anchorId, lifespan, cancellationToken);
        }
    }
}
