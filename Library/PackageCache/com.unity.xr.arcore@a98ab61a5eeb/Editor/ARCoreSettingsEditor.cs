using UnityEngine.XR.ARCore;

namespace UnityEditor.XR.ARCore
{
    /// <summary>
    /// This Editor renders to the XR Plug-in Management category of the Project Settings window.
    /// </summary>
    [CustomEditor(typeof(ARCoreSettings))]
    class ARCoreSettingsEditor : Editor
    {
        Editor m_Editor;

        void OnEnable()
        {
            CreateCachedEditor(ARCoreRuntimeSettings.Instance, typeof(ARCoreRuntimeSettingsEditor), ref m_Editor);
        }

        public override void OnInspectorGUI()
        {
            m_Editor.OnInspectorGUI();
        }
    }
}
