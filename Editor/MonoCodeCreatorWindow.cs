using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// Mono类绑定代码生成器窗口
    /// </summary>
    internal class MonoCodeCreatorWindow : EditorWindow
    {
        private string m_CodePath;
        private string m_CodeName;
        private string m_CodeNamespace;
        private GameObject m_SelectedObject;
        private char m_SeparatorChar;
        
        private MethodInfo m_AddComponentMethod;

        [MenuItem("GameObject/CodeBind/Mono Code Creator", priority = -3)]
        private static void ShowWindow()
        {
            GetWindow<MonoCodeCreatorWindow>("MonoCodeCreatorWindow");
        }

        private void OnEnable()
        {
            m_CodePath = EditorSetting.GetSaveCodePath();
            m_CodeNamespace = EditorSetting.GetSaveCodeNamespace();
            m_SeparatorChar = EditorSetting.GetSaveSeparatorChar();
            if(m_AddComponentMethod == null)
            {
                m_AddComponentMethod = typeof(InternalEditorUtility).GetMethod("AddScriptComponentUncheckedUndoable", BindingFlags.Static | BindingFlags.NonPublic);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            m_CodePath = EditorGUILayout.TextField("Code Path", m_CodePath);

            if (GUILayout.Button("Select Code Path", GUILayout.MaxWidth(150)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Code Path", m_CodePath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    m_CodePath = path.Replace(Application.dataPath, "Assets");
                }
            }

            EditorGUILayout.EndHorizontal();

            m_CodeNamespace = EditorGUILayout.TextField("Code Namespace", m_CodeNamespace);
            m_CodeName = EditorGUILayout.TextField("Code Name", m_CodeName);
            string separatorChar = EditorGUILayout.TextField("Separator Char", m_SeparatorChar.ToString());
            if (!string.IsNullOrEmpty(separatorChar))
            {
                m_SeparatorChar = separatorChar[0];
            }
            if(m_SelectedObject == null)
            {
                m_SelectedObject = Selection.activeGameObject;
            }
            m_SelectedObject = (GameObject)EditorGUILayout.ObjectField("Selected Object", m_SelectedObject, typeof(GameObject), true);

            // 如果文件名和目录名为空，则创建文件的按钮不可用
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_CodePath) || string.IsNullOrEmpty(m_CodeName) || m_SelectedObject == null || !Directory.Exists(m_CodePath));
            {
                if (GUILayout.Button("Create Code File"))
                {
                    CreateCodeFileAndAdd();
                    EditorSetting.SetSaveCodePath(m_CodePath);
                    EditorSetting.SetSaveCodeNamespace(m_CodeNamespace);
                    EditorSetting.SetSaveSeparatorChar(m_SeparatorChar);
                    Close();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void CreateCodeFileAndAdd()
        {
            MonoCodeCreator codeCreator = new MonoCodeCreator(m_CodePath, m_CodeName, m_CodeNamespace, m_SelectedObject.transform, m_SeparatorChar);
            codeCreator.TryCreateCodeFile();
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(Path.Combine(m_CodePath, $"{m_CodeName}.cs"));
            m_AddComponentMethod.Invoke(null, new object[] { m_SelectedObject, script });
        }
    }
}