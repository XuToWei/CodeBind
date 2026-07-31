using System;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// Generates a new plain class binding script and its generated partial source.
    /// </summary>
    internal sealed class PlainClassBindingGenerator : NewScriptBindingGenerator
    {
        public PlainClassBindingGenerator(string outputPath, string className, string namespaceName, Transform rootTransform, char nameSeparator) : base(outputPath, className, namespaceName, rootTransform, nameSeparator)
        {
        }

        protected override void SerializeBindings()
        {
            throw new Exception("PlainClassBindingGenerator does not support serialization!");
        }

        protected override string BuildBindingSource()
        {
            return BindingSourceBuilder.BuildPlainClassBindingSource(m_TargetNamespace, m_TargetClassName, m_SingleBindings, m_ArrayBindingsByMemberName, m_ArrayBindingElements);
        }

        protected override string BuildTargetSource()
        {
            if (!string.IsNullOrEmpty(m_TargetNamespace))
            {
                return $@"namespace {m_TargetNamespace}
{{
    public partial class {m_TargetClassName}
    {{

    }}
}}";
            }
            else
            {
                return $@"public partial class {m_TargetClassName}
{{

}}";
            }
        }
    }
}
