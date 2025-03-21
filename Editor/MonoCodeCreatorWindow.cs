using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal class MonoCodeCreatorWindow : EditorWindow
    {
        private const string DEFAULT_TARGET_DIRECTORY = "Assets/Scripts/Game";

        private string m_CodePath;
        private string m_CodeName;
        private string m_CodeNamespace;
        private GameObject m_SelectedObject;
        private char m_SeparatorChar = '_';

        [MenuItem("Assets/CodeBind/Mono Code Creator")]
        private static void ShowWindow()
        {
            GetWindow<MonoCodeCreatorWindow>("MonoCodeCreatorWindow");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Select Code Path"))
            {
                string path = EditorUtility.OpenFolderPanel("Select Code Path", DEFAULT_TARGET_DIRECTORY, "");
                if (!string.IsNullOrEmpty(path))
                {
                    m_CodePath = path.Replace(Application.dataPath, "Assets");
                }
            }

            m_CodePath = EditorGUILayout.TextField("Code Path", m_CodePath);

            m_CodeNamespace = EditorGUILayout.TextField("Code Namespace", m_CodeNamespace);
            m_CodeName = EditorGUILayout.TextField("Code Name", m_CodeName);
            m_SeparatorChar = EditorGUILayout.TextField("Separator Char", m_SeparatorChar.ToString())[0];
            if(m_SelectedObject == null)
            {
                m_SelectedObject = Selection.activeGameObject;
            }
            m_SelectedObject = (GameObject)EditorGUILayout.ObjectField("Selected Object", m_SelectedObject, typeof(GameObject), true);

            // 如果文件名和目录名为空，则创建文件的按钮不可用
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_CodePath) || string.IsNullOrEmpty(m_CodeName) || m_SelectedObject == null);
            {
                if (GUILayout.Button("Create Code File"))
                {
                    CreateCodeFile();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void CreateCodeFile()
        {
            MonoCodeCreator codeCreator = new MonoCodeCreator(m_CodePath, m_CodeName, m_CodeNamespace, m_SelectedObject.transform, m_SeparatorChar);
            codeCreator.TryCreateCodeFile();
        }
    }
}
