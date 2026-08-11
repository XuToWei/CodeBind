using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBind.Editor
{
    /// <summary>
    /// Inspector for plain class binding host data.
    /// </summary>
    [CustomEditor(typeof(PlainClassBindingHost))]
    [CanEditMultipleObjects]
    internal sealed class PlainClassBindingHostInspector : UnityEditor.Editor
    {
        private SerializedProperty m_NameSeparatorProperty;
        private SerializedProperty m_BindingClassScriptProperty;

        private SerializedProperty m_BindingTargetsProperty;
        private SerializedProperty m_BindingMemberNamesProperty;

        private bool m_ShowBindingTargets;

        private void OnEnable()
        {
            m_NameSeparatorProperty = serializedObject.FindProperty("m_NameSeparator");
            if (m_NameSeparatorProperty.intValue == 0)
            {
                m_NameSeparatorProperty.intValue = BindingEditorPreferences.GetNameSeparator();
                serializedObject.ApplyModifiedProperties();
            }
            m_BindingClassScriptProperty = serializedObject.FindProperty("m_BindingClassScript");
            m_BindingTargetsProperty = serializedObject.FindProperty("m_BindingTargets");
            m_BindingMemberNamesProperty = serializedObject.FindProperty("m_BindingMemberNames");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                if(m_BindingClassScriptProperty.objectReferenceValue == null)
                {
                    if (GUILayout.Button("Plain Class Binding Generator"))
                    {
                        PlainClassBindingGeneratorWindow.Open();
                    }
                }
                else
                {
                    if (GUILayout.Button("Generate Binding Source and Serialization"))
                    {
                        if (targets.Length > 1)
                        {
                            foreach (Object t in targets)
                            {
                                PlainClassBindingHost host = (PlainClassBindingHost)t;
                                PlainClassBinder binder = new PlainClassBinder(host.BindingClassScript, host.transform, host.NameSeparator);
                                binder.GenerateBindingSource();
                                binder.UpdateSerializedBindings();
                            }
                        }
                        else
                        {
                            PlainClassBinder binder = new PlainClassBinder((MonoScript)m_BindingClassScriptProperty.objectReferenceValue, ((MonoBehaviour)target).transform, (char)m_NameSeparatorProperty.intValue);
                            binder.GenerateBindingSource();
                            binder.UpdateSerializedBindings();
                        }
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_NameSeparatorProperty);
                if (EditorGUI.EndChangeCheck())
                {
                    BindingEditorPreferences.SetNameSeparator((char)m_NameSeparatorProperty.intValue);
                }

                EditorGUILayout.PropertyField(m_BindingClassScriptProperty);

                if (GUILayout.Button("Clear Serialization"))
                {
                    if (targets.Length > 1)
                    {
                        foreach (var t in targets)
                        {
                            ((PlainClassBindingHost)t).SetBindingTargets(null, null);
                        }
                    }
                    else
                    {
                        ((PlainClassBindingHost)target).SetBindingTargets(null, null);
                    }
                }

                if (targets.Length > 1)
                {
                    foreach (var t in targets)
                    {
                        if (((PlainClassBindingHost)t).HasMissingTargets())
                        {
                            SirenixEditorGUI.MessageBox("Binding targets contain missing references.", MessageType.Warning);
                            break;
                        }
                    }
                }
                else
                {
                    if (((PlainClassBindingHost)target).HasMissingTargets())
                    {
                        SirenixEditorGUI.MessageBox("Binding targets contain missing references.", MessageType.Warning);
                    }
                }

                SirenixEditorGUI.BeginBox();
                SirenixEditorGUI.BeginBoxHeader();
                string labelText = $"Binding Targets (count:{m_BindingTargetsProperty.arraySize})";
                m_ShowBindingTargets = SirenixEditorGUI.Foldout(m_ShowBindingTargets, labelText);
                SirenixEditorGUI.EndBoxHeader();
                if (SirenixEditorGUI.BeginFadeGroup(labelText, m_ShowBindingTargets))
                {
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Name");
                        EditorGUILayout.LabelField("Target");
                        GUILayout.EndHorizontal();
                        for (int i = 0; i < m_BindingTargetsProperty.arraySize; i++)
                        {
                            GUILayout.BeginHorizontal();
                            string memberName = m_BindingMemberNamesProperty.GetArrayElementAtIndex(i).stringValue;
                            EditorGUILayout.TextField(memberName);
                            EditorGUILayout.ObjectField(m_BindingTargetsProperty.GetArrayElementAtIndex(i).objectReferenceValue, typeof (Component), true);
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
