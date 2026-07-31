using System;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// Generates a new MonoBehaviour binding script and its generated partial source.
    /// </summary>
    internal sealed class MonoBehaviourBindingGenerator : NewScriptBindingGenerator
    {
        public MonoBehaviourBindingGenerator(string outputPath, string className, string namespaceName, Transform rootTransform, char nameSeparator) : base(outputPath, className, namespaceName, rootTransform, nameSeparator)
        {
        }

        protected override void SerializeBindings()
        {
            throw new Exception("MonoBehaviourBindingGenerator does not support serialization!");
        }

        protected override string BuildBindingSource()
        {
            return BindingSourceBuilder.BuildMonoBehaviourBindingSource(m_TargetNamespace, m_TargetClassName, m_SingleBindings, m_ArrayBindingsByMemberName);
        }

        protected override string BuildTargetSource()
        {
            if (!string.IsNullOrEmpty(m_TargetNamespace))
            {
                return $@"using UnityEngine;
using CodeBind;

namespace {m_TargetNamespace}
{{
    [MonoBehaviourBinding('{m_NameSeparator}')]
    public partial class {m_TargetClassName} : MonoBehaviour
    {{

    }}
}}";
            }
            else
            {
                return $@"using UnityEngine;
using CodeBind;

[MonoBehaviourBinding('{m_NameSeparator}')]
public partial class {m_TargetClassName} : MonoBehaviour
{{

}}";
            }
        }
    }
}
