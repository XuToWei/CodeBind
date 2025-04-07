using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// 非Mono类绑定代码创建窗口
    /// </summary>
    internal class CSCodeCreatorWindow : EditorWindow
    {
        private string m_CodePath;
        private string m_CodeName;
        private string m_CodeNamespace;
        private CSCodeBindMono m_CSCodeBindMono;
        
        internal static void ShowWindow()
        {
            GetWindow<CSCodeCreatorWindow>("CSCodeCreatorWindow");
        }

        private void OnEnable()
        {
            m_CodePath = EditorSetting.GetSaveCodePath();
            m_CodeNamespace = EditorSetting.GetSaveCodeNamespace();
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
            if(m_CSCodeBindMono == null)
            {
                m_CSCodeBindMono = Selection.activeGameObject.GetComponent<CSCodeBindMono>();
            }
            m_CSCodeBindMono = (CSCodeBindMono)EditorGUILayout.ObjectField("Selected Object", m_CSCodeBindMono, typeof(CSCodeBindMono), true);
            // 如果文件名和目录名为空，则创建文件的按钮不可用
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_CodePath) || string.IsNullOrEmpty(m_CodeName) || m_CSCodeBindMono == null || !Directory.Exists(m_CodePath));
            {
                if (GUILayout.Button("Create Code File"))
                {
                    CreateCodeFileAndAdd();
                    EditorSetting.SetSaveCodePath(m_CodePath);
                    EditorSetting.SetSaveCodeNamespace(m_CodeNamespace);
                    Close();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void CreateCodeFileAndAdd()
        {
            CSCodeCreator codeCreator = new CSCodeCreator(m_CodePath, m_CodeName, m_CodeNamespace, m_CSCodeBindMono.transform, m_CSCodeBindMono.SeparatorChar);
            codeCreator.TryCreateCodeFile();
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(Path.Combine(m_CodePath, $"{m_CodeName}.cs")) ;
            FieldInfo bindScriptFieldInfo = m_CSCodeBindMono.GetType().GetField("m_BindScript", BindingFlags.NonPublic | BindingFlags.Instance);
            bindScriptFieldInfo.SetValue(m_CSCodeBindMono, script);
        }
    }
}