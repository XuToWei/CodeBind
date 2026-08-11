using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// Window for generating a new MonoBehaviour binding script.
    /// </summary>
    internal class MonoBehaviourBindingGeneratorWindow : EditorWindow
    {
        private string m_OutputPath;
        private string m_ClassName;
        private string m_NamespaceName;
        private GameObject m_SelectedGameObject;
        private char m_NameSeparator;

        private MethodInfo m_AddScriptComponentMethod;

        [MenuItem("GameObject/CodeBind/MonoBehaviour Binding Generator", priority = -4)]
        private static void Open()
        {
            GetWindow<MonoBehaviourBindingGeneratorWindow>("MonoBehaviour Binding Generator");
        }

        private void OnEnable()
        {
            m_OutputPath = BindingEditorPreferences.GetOutputPath();
            m_NamespaceName = BindingEditorPreferences.GetDefaultNamespace();
            m_NameSeparator = BindingEditorPreferences.GetNameSeparator();
            if(m_AddScriptComponentMethod == null)
            {
                m_AddScriptComponentMethod = typeof(InternalEditorUtility).GetMethod("AddScriptComponentUncheckedUndoable", BindingFlags.Static | BindingFlags.NonPublic);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            m_OutputPath = EditorGUILayout.TextField("Output Path", m_OutputPath);

            if (GUILayout.Button("Select Output Path", GUILayout.MaxWidth(150)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Output Path", m_OutputPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    m_OutputPath = path.Replace(Application.dataPath, "Assets");
                }
            }

            EditorGUILayout.EndHorizontal();

            if(m_SelectedGameObject == null)
            {
                m_SelectedGameObject = Selection.activeGameObject;
                if (m_SelectedGameObject != null)
                {
                    m_ClassName = m_SelectedGameObject.name.Replace("_", "").Replace(".", "").Replace(" ", "");
                }
            }
            m_NamespaceName = EditorGUILayout.TextField("Namespace", m_NamespaceName);
            m_ClassName = EditorGUILayout.TextField("Class Name", m_ClassName);
            string nameSeparator = EditorGUILayout.TextField("Name Separator", m_NameSeparator.ToString());
            if (!string.IsNullOrEmpty(nameSeparator))
            {
                m_NameSeparator = nameSeparator[0];
            }
            m_SelectedGameObject = (GameObject)EditorGUILayout.ObjectField("Selected GameObject", m_SelectedGameObject, typeof(GameObject), true);

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_OutputPath) || string.IsNullOrEmpty(m_ClassName) || m_SelectedGameObject == null || !Directory.Exists(m_OutputPath));
            {
                if (GUILayout.Button("Generate and Attach Script"))
                {
                    GenerateAndAttachScript();
                    BindingEditorPreferences.SetOutputPath(m_OutputPath);
                    BindingEditorPreferences.SetDefaultNamespace(m_NamespaceName);
                    BindingEditorPreferences.SetNameSeparator(m_NameSeparator);
                    Close();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void GenerateAndAttachScript()
        {
            MonoBehaviourBindingGenerator generator = new MonoBehaviourBindingGenerator(m_OutputPath, m_ClassName, m_NamespaceName, m_SelectedGameObject.transform, m_NameSeparator);
            generator.GenerateScripts();
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(Path.Combine(m_OutputPath, $"{m_ClassName}.cs"));
            m_AddScriptComponentMethod.Invoke(null, new object[] { m_SelectedGameObject, script });
        }
    }
}
