using System;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// Runtime settings for the Google ARCore XR Plug-in.
    /// </summary>
    [Serializable]
    [ScriptableSettingsPath(runtimeSettingsPath)]
    class ARCoreRuntimeSettings : ScriptableSettings<ARCoreRuntimeSettings>
    {
        internal const string runtimeSettingsPath = "Assets/XR";

        [Header("Google Cloud Anchors")]

        [SerializeField, Tooltip("Enable or disable Google Cloud Anchors")]
        bool m_EnableCloudAnchors;

        internal bool enableCloudAnchors
        {
            get => m_EnableCloudAnchors;
            set => m_EnableCloudAnchors = value;
        }

        internal enum AuthorizationType
        {
            ApiKey = 0,
            Keyless = 1
        }

        [SerializeField, Tooltip("Authorization Type for Google Cloud Anchors.")]
        AuthorizationType m_AuthorizationType;

        public AuthorizationType authorizationType
        {
            get => m_AuthorizationType;
            set => m_AuthorizationType = value;
        }

        [SerializeField, Tooltip("API Key for Google Cloud")]
        string m_ApiKey;

        public string apiKey
        {
            get => m_ApiKey;
            set => m_ApiKey = value;
        }
    }
}
