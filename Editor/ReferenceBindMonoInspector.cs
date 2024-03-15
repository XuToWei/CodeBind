using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    [CustomEditor(typeof(ReferenceBindMono))]
    internal sealed class ReferenceBindMonoInspector : UnityEditor.Editor
    {
        private SerializedProperty m_BindNames;
        private SerializedProperty m_BindGameObjects;

        private string m_AddBindName;
        private GameObject m_AddBindGameObject;

        private void OnEnable()
        {
            m_BindNames = serializedObject.FindProperty("m_BindNames");
            m_BindGameObjects = serializedObject.FindProperty("m_BindGameObjects");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name");
                EditorGUILayout.LabelField("Component");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                m_AddBindName = EditorGUILayout.TextField(m_AddBindName);
                m_AddBindGameObject = (GameObject)EditorGUILayout.ObjectField(m_AddBindGameObject, typeof(GameObject), true);
                if (GUILayout.Button("+") && !string.IsNullOrEmpty(m_AddBindName))
                {
                    bool isRepeated = false;
                    for (int i = 0; i < m_BindNames.arraySize; i++)
                    {
                        string goName = m_BindNames.GetArrayElementAtIndex(i).stringValue;
                        if (m_AddBindName == goName)
                        {
                            isRepeated = true;
                            m_BindNames.GetArrayElementAtIndex(i).stringValue = m_AddBindName;
                            m_BindGameObjects.GetArrayElementAtIndex(i).objectReferenceValue = m_AddBindGameObject;
                            m_AddBindName = string.Empty;
                            m_AddBindGameObject = null;
                            break;
                        }
                    }

                    if (!isRepeated)
                    {
                        m_BindNames.InsertArrayElementAtIndex(0);
                        m_BindNames.GetArrayElementAtIndex(0).stringValue = m_AddBindName;
                        m_BindGameObjects.InsertArrayElementAtIndex(0);
                        m_BindGameObjects.GetArrayElementAtIndex(0).objectReferenceValue = m_AddBindGameObject;
                        m_AddBindName = string.Empty;
                        m_AddBindGameObject = null;
                    }

                    serializedObject.ApplyModifiedProperties();
                }

                GUILayout.EndHorizontal();

                for (int i = 0; i < m_BindNames.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();
                    string goName = m_BindNames.GetArrayElementAtIndex(i).stringValue;
                    EditorGUILayout.TextField(goName);
                    EditorGUILayout.ObjectField(m_BindGameObjects.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), true);
                    if (GUILayout.Button("-"))
                    {
                        m_BindNames.DeleteArrayElementAtIndex(i);
                        m_BindGameObjects.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                    }

                    GUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Clear Serialization"))
                {
                    m_BindNames.ClearArray();
                    m_BindGameObjects.ClearArray();
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
