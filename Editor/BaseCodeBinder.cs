using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal abstract class BaseCodeBinder : BaseBinder
    {
        private readonly string m_BindScriptFullPath;

        protected readonly string m_ScriptNameSpace;

        protected readonly string m_ScriptClassName;

        protected BaseCodeBinder(MonoScript script, Transform rootTransform, char separatorChar) : base(rootTransform, separatorChar)
        {
            if (script == null)
            {
                throw new Exception("请设置需要绑定的脚本！");
            }
            if (script.name.EndsWith(".Bind"))
            {
                throw new Exception("不可以绑定“.Bind”结尾的脚本！");
            }
            if (!script.text.Contains("partial"))
            {
                throw new Exception($"please add key word 'partial' into {script.GetClass().FullName}!");
            }

            string scriptFullPath = AssetDatabase.GetAssetPath(script);
            m_BindScriptFullPath = scriptFullPath.Insert(scriptFullPath.LastIndexOf('.'), ".Bind");
            m_ScriptNameSpace = script.GetClass().Namespace;
            m_ScriptClassName = script.GetClass().Name;
        }

        public void TryGenerateBindCode()
        {
            CodeBindNameTypeCollection.Do();
            AutoFixChildBindName();
            if (!TryGenerateNameMapTypeData())
            {
                return;
            }
            string codeStr = GetGeneratedCode().Replace("\t", "    ");
            if (File.Exists(m_BindScriptFullPath) && string.Equals(codeStr, File.ReadAllText(m_BindScriptFullPath)))
            {
                Debug.Log("文件内容相同。不需要重新生成。");
                return;
            }
            using StreamWriter sw = new StreamWriter(m_BindScriptFullPath);
            sw.Write(codeStr);
            sw.Close();
            AssetDatabase.ImportAsset(m_BindScriptFullPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log($"代码生成成功,生成路径: {m_BindScriptFullPath}");
        }

        protected abstract string GetGeneratedCode();
    }
}
