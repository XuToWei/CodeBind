using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// 非Mono类绑定数据的Mono编辑器界面
    /// </summary>
    [CustomEditor(typeof(CSCodeBindMono))]
    internal sealed class CSCodeBindMonoInspector : UnityEditor.Editor
    {
        private SerializedProperty m_SeparatorChar;
        private SerializedProperty m_BindScript;
        
        private SerializedProperty m_BindComponents;
        private SerializedProperty m_BindComponentNames;

        private bool m_ShowBindComponents;
        
        private void OnEnable()
        {
            m_SeparatorChar = serializedObject.FindProperty("m_SeparatorChar");
            if (m_SeparatorChar.intValue == 0)
            {
                m_SeparatorChar.intValue = EditorSetting.GetSaveSeparatorChar();
                serializedObject.ApplyModifiedProperties();
            }
            m_BindScript = serializedObject.FindProperty("m_BindScript");
            m_BindComponents = serializedObject.FindProperty("m_BindComponents");
            m_BindComponentNames = serializedObject.FindProperty("m_BindComponentNames");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                if(m_BindScript.objectReferenceValue == null)
                {
                    if (GUILayout.Button("CodeBind Creator"))
                    {
                        CSCodeCreatorWindow.ShowWindow();
                    }
                }
                else
                {
                    if (GUILayout.Button("Generate BindCode and Serialization"))
                    {
                        CSCodeBinder codeBinder = new CSCodeBinder((MonoScript)m_BindScript.objectReferenceValue, ((MonoBehaviour)target).transform, (char)m_SeparatorChar.intValue);
                        codeBinder.TryGenerateBindCode();
                        codeBinder.TrySetSerialization();
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_SeparatorChar);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorSetting.SetSaveSeparatorChar((char)m_SeparatorChar.intValue);
                }

                EditorGUILayout.PropertyField(m_BindScript);

                if (GUILayout.Button("Clear Serialization"))
                {
                    m_BindComponentNames.ClearArray();
                    m_BindComponents.ClearArray();
                }

                SirenixEditorGUI.BeginBox();
                SirenixEditorGUI.BeginBoxHeader();
                string labelText = $"Bind Data (count:{m_BindComponents.arraySize})";
                m_ShowBindComponents = SirenixEditorGUI.Foldout(m_ShowBindComponents, labelText);
                SirenixEditorGUI.EndBoxHeader();
                if (SirenixEditorGUI.BeginFadeGroup(labelText, m_ShowBindComponents))
                {
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Name");
                        EditorGUILayout.LabelField("Component");
                        GUILayout.EndHorizontal();
                        for (int i = 0; i < m_BindComponents.arraySize; i++)
                        {
                            GUILayout.BeginHorizontal();
                            string cName = m_BindComponentNames.GetArrayElementAtIndex(i).stringValue;
                            EditorGUILayout.TextField(cName);
                            EditorGUILayout.ObjectField(m_BindComponents.GetArrayElementAtIndex(i).objectReferenceValue, typeof (Component), true);
                            GUILayout.EndHorizontal();
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                SirenixEditorGUI.EndFadeGroup();
                SirenixEditorGUI.EndBox();
            }
            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
