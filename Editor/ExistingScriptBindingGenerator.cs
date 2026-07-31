using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// Generates binding source for an existing script.
    /// </summary>
    internal abstract class ExistingScriptBindingGenerator : HierarchyBindingProcessor
    {
        protected readonly string m_GeneratedScriptPath;

        protected readonly string m_TargetNamespace;

        protected readonly string m_TargetClassName;

        protected ExistingScriptBindingGenerator(MonoScript script, Transform rootTransform, char nameSeparator) : base(rootTransform, nameSeparator)
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

            string targetScriptPath = AssetDatabase.GetAssetPath(script);
            m_GeneratedScriptPath = targetScriptPath.Insert(targetScriptPath.LastIndexOf('.'), ".Bind");
            m_TargetNamespace = script.GetClass().Namespace;
            m_TargetClassName = script.GetClass().Name;
        }

        public void TryGenerateBindingSource()
        {
            BindingTargetTokenRegistry.EnsureInitialized();
            NormalizeBindingNodeNames();
            if (!TryCollectBindings())
            {
                return;
            }
            string generatedSource = BuildBindingSource().Replace("\t", "    ");
            if (File.Exists(m_GeneratedScriptPath) && string.Equals(generatedSource, File.ReadAllText(m_GeneratedScriptPath)))
            {
                Debug.Log("[CodeBind] File content is identical, skip regeneration.");
                return;
            }
            using StreamWriter writer = new StreamWriter(m_GeneratedScriptPath);
            writer.Write(generatedSource);
            writer.Close();
            AssetDatabase.ImportAsset(m_GeneratedScriptPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log($"[CodeBind] Code generated successfully, path: {m_GeneratedScriptPath}");
        }

        protected abstract string BuildBindingSource();
    }
}
