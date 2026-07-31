using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// Generates a new target script and its binding source.
    /// </summary>
    internal abstract class NewScriptBindingGenerator : HierarchyBindingProcessor
    {
        private readonly string m_TargetScriptPath;
        private readonly string m_GeneratedScriptPath;
        protected readonly string m_TargetNamespace;
        protected readonly string m_TargetClassName;

        protected NewScriptBindingGenerator(string outputPath, string className, string namespaceName, Transform rootTransform, char nameSeparator) : base(rootTransform, nameSeparator)
        {
            m_TargetScriptPath = Path.Combine(outputPath, $"{className}.cs");
            m_GeneratedScriptPath = Path.Combine(outputPath, $"{className}.Bind.cs");
            m_TargetNamespace = namespaceName;
            m_TargetClassName = className;
        }

        public void TryGenerateScripts()
        {
            if (File.Exists(m_TargetScriptPath))
            {
                Debug.Log("[CodeBind] File already exists, skip generation.");
                return;
            }
            if (File.Exists(m_GeneratedScriptPath))
            {
                File.Delete(m_GeneratedScriptPath);
            }
            BindingTargetTokenRegistry.EnsureInitialized();
            NormalizeBindingNodeNames();
            if (!TryCollectBindings())
            {
                return;
            }
            string targetSource = BuildTargetSource().Replace("\t", "    ");
            using StreamWriter targetWriter = new StreamWriter(m_TargetScriptPath);
            targetWriter.Write(targetSource);
            targetWriter.Close();

            string bindingSource = BuildBindingSource().Replace("\t", "    ");
            using StreamWriter bindingWriter = new StreamWriter(m_GeneratedScriptPath);
            bindingWriter.Write(bindingSource);
            bindingWriter.Close();
            AssetDatabase.ImportAsset(m_GeneratedScriptPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log($"[CodeBind] Code generated successfully, path: {m_TargetScriptPath}");
        }

        protected abstract string BuildBindingSource();
        protected abstract string BuildTargetSource();
    }
}
