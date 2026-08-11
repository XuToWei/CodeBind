using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    [CustomEditor(typeof(NamedReferenceBindingHost))]
    internal sealed class NamedReferenceBindingHostInspector : UnityEditor.Editor
    {
        private SerializedProperty m_NameSeparatorProperty;

        private SerializedProperty m_ManualKeysProperty;
        private SerializedProperty m_ManualGameObjectsProperty;

        private SerializedProperty m_GeneratedReferenceKeysProperty;
        private SerializedProperty m_GeneratedReferencesProperty;

        private string m_NewManualKey;
        private GameObject m_NewManualGameObject;

        private bool m_ManualReferencesExpanded = true;
        private bool m_GeneratedReferencesExpanded = true;

        private void OnEnable()
        {
            m_NameSeparatorProperty = serializedObject.FindProperty("m_NameSeparator");
            if (m_NameSeparatorProperty.intValue == 0)
            {
                m_NameSeparatorProperty.intValue = BindingEditorPreferences.GetNameSeparator();
                serializedObject.ApplyModifiedProperties();
            }
            m_ManualKeysProperty = serializedObject.FindProperty("m_ManualKeys");
            m_ManualGameObjectsProperty = serializedObject.FindProperty("m_ManualGameObjects");
            m_GeneratedReferenceKeysProperty = serializedObject.FindProperty("m_GeneratedReferenceKeys");
            m_GeneratedReferencesProperty = serializedObject.FindProperty("m_GeneratedReferences");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                SirenixEditorGUI.BeginBox();
                SirenixEditorGUI.BeginBoxHeader();
                string manualReferencesLabel = "Manual GameObject References";
                m_ManualReferencesExpanded = SirenixEditorGUI.Foldout(m_ManualReferencesExpanded, manualReferencesLabel);
                SirenixEditorGUI.EndBoxHeader();
                if (SirenixEditorGUI.BeginFadeGroup(manualReferencesLabel, m_ManualReferencesExpanded))
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Key");
                    EditorGUILayout.LabelField("GameObject");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    m_NewManualKey = EditorGUILayout.TextField(m_NewManualKey);
                    m_NewManualGameObject = (GameObject)EditorGUILayout.ObjectField(m_NewManualGameObject, typeof(GameObject), true);
                    if (GUILayout.Button("+") && !string.IsNullOrEmpty(m_NewManualKey))
                    {
                        bool isRepeated = false;
                        for (int i = 0; i < m_ManualKeysProperty.arraySize; i++)
                        {
                            string key = m_ManualKeysProperty.GetArrayElementAtIndex(i).stringValue;
                            if (m_NewManualKey == key)
                            {
                                isRepeated = true;
                                m_ManualKeysProperty.GetArrayElementAtIndex(i).stringValue = m_NewManualKey;
                                m_ManualGameObjectsProperty.GetArrayElementAtIndex(i).objectReferenceValue = m_NewManualGameObject;
                                m_NewManualKey = string.Empty;
                                m_NewManualGameObject = null;
                                break;
                            }
                        }

                        if (!isRepeated)
                        {
                            m_ManualKeysProperty.InsertArrayElementAtIndex(0);
                            m_ManualKeysProperty.GetArrayElementAtIndex(0).stringValue = m_NewManualKey;
                            m_ManualGameObjectsProperty.InsertArrayElementAtIndex(0);
                            m_ManualGameObjectsProperty.GetArrayElementAtIndex(0).objectReferenceValue = m_NewManualGameObject;
                            m_NewManualKey = string.Empty;
                            m_NewManualGameObject = null;
                        }

                        serializedObject.ApplyModifiedProperties();
                    }
                    GUILayout.EndHorizontal();

                    for (int i = 0; i < m_ManualKeysProperty.arraySize; i++)
                    {
                        GUILayout.BeginHorizontal();
                        var keyProperty = m_ManualKeysProperty.GetArrayElementAtIndex(i);
                        keyProperty.stringValue = EditorGUILayout.TextField(keyProperty.stringValue);
                        var gameObjectProperty = m_ManualGameObjectsProperty.GetArrayElementAtIndex(i);
                        gameObjectProperty.objectReferenceValue = EditorGUILayout.ObjectField(gameObjectProperty.objectReferenceValue, typeof(GameObject), true);
                        if (GUILayout.Button("-"))
                        {
                            m_ManualKeysProperty.DeleteArrayElementAtIndex(i);
                            m_ManualGameObjectsProperty.DeleteArrayElementAtIndex(i);
                            serializedObject.ApplyModifiedProperties();
                        }
                        GUILayout.EndHorizontal();
                    }

                    if (GUILayout.Button("Clear Manual GameObject References"))
                    {
                        m_ManualKeysProperty.ClearArray();
                        m_ManualGameObjectsProperty.ClearArray();
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                SirenixEditorGUI.EndFadeGroup();
                SirenixEditorGUI.EndBox();

                if (((NamedReferenceBindingHost)target).HasMissingReferences())
                {
                    SirenixEditorGUI.MessageBox("Named references contain missing targets.", MessageType.Warning);
                }

                SirenixEditorGUI.BeginBox();
                SirenixEditorGUI.BeginBoxHeader();
                string generatedReferencesLabel = $"Generated References (count:{m_GeneratedReferencesProperty.arraySize})";
                m_GeneratedReferencesExpanded = SirenixEditorGUI.Foldout(m_GeneratedReferencesExpanded, generatedReferencesLabel);
                SirenixEditorGUI.EndBoxHeader();
                if (SirenixEditorGUI.BeginFadeGroup(generatedReferencesLabel, m_GeneratedReferencesExpanded))
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Generate Serialization"))
                    {
                        NamedReferenceBinder binder = new NamedReferenceBinder((NamedReferenceBindingHost)target, (char)m_NameSeparatorProperty.intValue);
                        binder.UpdateSerializedBindings();
                    }
                    if (GUILayout.Button("Clear Serialization"))
                    {
                        m_GeneratedReferenceKeysProperty.ClearArray();
                        m_GeneratedReferencesProperty.ClearArray();
                    }
                    GUILayout.EndHorizontal();

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(m_NameSeparatorProperty);
                    if (EditorGUI.EndChangeCheck())
                    {
                        BindingEditorPreferences.SetNameSeparator((char)m_NameSeparatorProperty.intValue);
                    }

                    EditorGUI.BeginDisabledGroup(true);
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Key");
                        EditorGUILayout.LabelField("Target");
                        GUILayout.EndHorizontal();
                        for (int i = 0; i < m_GeneratedReferencesProperty.arraySize; i++)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.TextField(m_GeneratedReferenceKeysProperty.GetArrayElementAtIndex(i).stringValue);
                            EditorGUILayout.ObjectField(m_GeneratedReferencesProperty.GetArrayElementAtIndex(i).objectReferenceValue, typeof (UnityEngine.Object), true);
                            GUILayout.EndHorizontal();
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                SirenixEditorGUI.EndFadeGroup();
                SirenixEditorGUI.EndBox();

                if (GUILayout.Button("Clear All Serialization"))
                {
                    m_ManualKeysProperty.ClearArray();
                    m_ManualGameObjectsProperty.ClearArray();
                    m_GeneratedReferenceKeysProperty.ClearArray();
                    m_GeneratedReferencesProperty.ClearArray();
                }
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
