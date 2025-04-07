using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// 基础代码创建器
    /// </summary>
    internal abstract class BaseCodeCreator : BaseBinder
    {
        private readonly string m_ScriptFullPath;
        private readonly string m_BindScriptFullPath;
        protected readonly string m_ScriptNameSpace;
        protected readonly string m_ScriptClassName;
        
        protected BaseCodeCreator(string codePath, string codeName, string codeNamespace, Transform rootTransform, char separatorChar) : base(rootTransform, separatorChar)
        {
            m_ScriptFullPath = Path.Combine(codePath, $"{codeName}.cs");
            m_BindScriptFullPath = Path.Combine(codePath, $"{codeName}.Bind.cs");
            m_ScriptNameSpace = codeNamespace;
            m_ScriptClassName = codeName;
        }
        
        public void TryCreateCodeFile()
        {
            if (File.Exists(m_ScriptFullPath))
            {
                Debug.Log("文件已存在，不能重复生成。");
                return;
            }
            if (File.Exists(m_BindScriptFullPath))
            {
                File.Delete(m_BindScriptFullPath);
            }
            CodeBindNameTypeCollection.Do();
            AutoFixChildBindName();
            if (!TryGenerateNameMapTypeData())
            {
                return;
            }
            string codeClassStr = GetClassCode().Replace("\t", "    ");
            using StreamWriter classSw = new StreamWriter(m_ScriptFullPath);
            classSw.Write(codeClassStr);
            classSw.Close();
            
            string codeBindStr = GetBindCode().Replace("\t", "    ");
            using StreamWriter bindSw = new StreamWriter(m_BindScriptFullPath);
            bindSw.Write(codeBindStr);
            bindSw.Close();
            AssetDatabase.ImportAsset(m_BindScriptFullPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log($"代码生成成功,生成路径: {m_ScriptFullPath}");
        }

        protected abstract string GetBindCode();
        protected abstract string GetClassCode();
    }
}