using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// Window for generating a new plain class binding script.
    /// </summary>
    internal class PlainClassBindingGeneratorWindow : EditorWindow
    {
        private string m_OutputPath;
        private string m_ClassName;
        private string m_NamespaceName;
        private PlainClassBindingHost m_Host;

        [MenuItem("GameObject/CodeBind/Plain Class Binding Generator", priority = -3)]
        internal static void Open()
        {
            GetWindow<PlainClassBindingGeneratorWindow>("Plain Class Binding Generator");
        }

        private void OnEnable()
        {
            m_OutputPath = BindingEditorPreferences.GetOutputPath();
            m_NamespaceName = BindingEditorPreferences.GetDefaultNamespace();
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

            if(m_Host == null)
            {
                GameObject selectedObject = Selection.activeGameObject;
                if (selectedObject != null)
                {
                    m_Host = selectedObject.GetComponent<PlainClassBindingHost>();
                    if (m_Host != null)
                    {
                        m_ClassName = m_Host.name.Replace("_", "").Replace(".", "").Replace(" ", "");
                    }
                }
            }
            m_NamespaceName = EditorGUILayout.TextField("Namespace", m_NamespaceName);
            m_ClassName = EditorGUILayout.TextField("Class Name", m_ClassName);
            m_Host = (PlainClassBindingHost)EditorGUILayout.ObjectField("Binding Host", m_Host, typeof(PlainClassBindingHost), true);
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_OutputPath) || string.IsNullOrEmpty(m_ClassName) || m_Host == null || !Directory.Exists(m_OutputPath));
            {
                if (GUILayout.Button("Generate and Assign Script"))
                {
                    GenerateAndAssignScript();
                    BindingEditorPreferences.SetOutputPath(m_OutputPath);
                    BindingEditorPreferences.SetDefaultNamespace(m_NamespaceName);
                    Close();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void GenerateAndAssignScript()
        {
            PlainClassBindingGenerator generator = new PlainClassBindingGenerator(m_OutputPath, m_ClassName, m_NamespaceName, m_Host.transform, m_Host.NameSeparator);
            generator.GenerateScripts();
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(Path.Combine(m_OutputPath, $"{m_ClassName}.cs"));
            FieldInfo bindingClassScriptField = m_Host.GetType().GetField("m_BindingClassScript", BindingFlags.NonPublic | BindingFlags.Instance);
            bindingClassScriptField.SetValue(m_Host, script);
        }
    }
}
