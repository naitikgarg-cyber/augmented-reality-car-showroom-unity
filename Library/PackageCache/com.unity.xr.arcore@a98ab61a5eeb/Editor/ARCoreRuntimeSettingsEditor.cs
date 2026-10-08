using UnityEngine;
using UnityEngine.XR.ARCore;

namespace UnityEditor.XR.ARCore
{
    [CustomEditor(typeof(ARCoreRuntimeSettings))]
    class ARCoreRuntimeSettingsEditor : Editor
    {
        SerializedObject m_ARCoreRuntimeSettingsSerializedObject;
        SerializedObject m_ARCoreSettingsSerializedObject;

        SerializedProperty m_RequirementProperty;
        SerializedProperty m_DepthProperty;
        SerializedProperty m_IgnoreGradleVersionProperty;
        SerializedProperty m_EnableCloudAnchorsProperty;
        SerializedProperty m_AuthorizationTypeProperty;
        SerializedProperty m_ApiKeyProperty;

        static readonly GUIContent k_KeylessAuthText = new("Follow the steps to configure Keyless authorization for your app.");
        static readonly GUIContent k_KeylessAuthUrlText = new("View Documentation");
        const string k_KeylessAuthUrl = "https://developers.google.com/ar/develop/authorization";

        static GUIStyle s_UrlLabelPersonal;
        static GUIStyle s_UrlLabelProfessional;

        void OnEnable()
        {
            var arCoreSettings = ARCoreSettings.GetOrCreateSettings();
            m_ARCoreSettingsSerializedObject = new SerializedObject(arCoreSettings);
            m_RequirementProperty = m_ARCoreSettingsSerializedObject.FindProperty("m_Requirement");
            m_DepthProperty = m_ARCoreSettingsSerializedObject.FindProperty("m_Depth");
            m_IgnoreGradleVersionProperty = m_ARCoreSettingsSerializedObject.FindProperty("m_IgnoreGradleVersion");

            m_ARCoreRuntimeSettingsSerializedObject = new SerializedObject(ARCoreRuntimeSettings.Instance);
            m_EnableCloudAnchorsProperty = m_ARCoreRuntimeSettingsSerializedObject.FindProperty("m_EnableCloudAnchors");
            m_AuthorizationTypeProperty = m_ARCoreRuntimeSettingsSerializedObject.FindProperty("m_AuthorizationType");
            m_ApiKeyProperty = m_ARCoreRuntimeSettingsSerializedObject.FindProperty("m_ApiKey");
        }

        public override void OnInspectorGUI()
        {
            m_ARCoreRuntimeSettingsSerializedObject.Update();
            m_ARCoreSettingsSerializedObject.Update();

            using var change = new EditorGUI.ChangeCheckScope();
            EditorGUILayout.PropertyField(m_RequirementProperty);
            EditorGUILayout.PropertyField(m_DepthProperty);
            EditorGUILayout.PropertyField(m_IgnoreGradleVersionProperty);
            EditorGUILayout.PropertyField(m_EnableCloudAnchorsProperty);

            if (m_EnableCloudAnchorsProperty.boolValue)
            {
                EditorGUILayout.PropertyField(m_AuthorizationTypeProperty);
                if (m_AuthorizationTypeProperty.intValue == (int)ARCoreRuntimeSettings.AuthorizationType.ApiKey)
                {
                    EditorGUILayout.PropertyField(m_ApiKeyProperty);
                }
                else if (m_AuthorizationTypeProperty.intValue == (int)ARCoreRuntimeSettings.AuthorizationType.Keyless)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.LabelField(k_KeylessAuthText);
                        DisplayLink(k_KeylessAuthUrlText, k_KeylessAuthUrl, 2);
                    }
                    GUILayout.EndVertical();
                }
            }

            if (change.changed)
            {
                m_ARCoreRuntimeSettingsSerializedObject.ApplyModifiedProperties();
                m_ARCoreSettingsSerializedObject.ApplyModifiedProperties();
            }
        }

        static void DisplayLink(GUIContent text, string url, int leftMargin)
        {
            s_UrlLabelPersonal ??= new(EditorStyles.label)
            {
                name = "url-label",
                richText = true,
                normal = new GUIStyleState { textColor = new(8 / 255f, 8 / 255f, 252 / 255f) },
            };

            s_UrlLabelProfessional ??= new(EditorStyles.label)
            {
                name = "url-label",
                richText = true,
                normal = new GUIStyleState { textColor = new(79 / 255f, 128 / 255f, 248 / 255f) },
            };

            var labelStyle = EditorGUIUtility.isProSkin ? s_UrlLabelProfessional : s_UrlLabelPersonal;
            var size = labelStyle.CalcSize(text);
            var uriRect = GUILayoutUtility.GetRect(text, labelStyle);
            uriRect.x += leftMargin;
            uriRect.width = size.x;
            if (GUI.Button(uriRect, text, labelStyle))
            {
                Application.OpenURL(url);
            }
            EditorGUIUtility.AddCursorRect(uriRect, MouseCursor.Link);
            EditorGUI.DrawRect(new Rect(uriRect.x, uriRect.y + uriRect.height - 1, uriRect.width, 1), labelStyle.normal.textColor);
        }
    }
}
