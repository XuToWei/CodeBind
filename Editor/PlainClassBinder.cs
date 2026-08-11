using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal sealed class PlainClassBinder : ExistingScriptBindingGenerator
    {
        private readonly PlainClassBindingHost m_Host;

        public PlainClassBinder(MonoScript script, Transform rootTransform, char nameSeparator): base(script, rootTransform, nameSeparator)
        {
            m_Host = rootTransform.GetComponent<PlainClassBindingHost>();
            if (m_Host == null)
            {
                throw new Exception($"PlainClassBinder init fail! {rootTransform} has no PlainClassBindingHost!");
            }
        }

        protected override string BuildBindingSource()
        {
            return BindingSourceBuilder.BuildPlainClassBindingSource(m_TargetNamespace, m_TargetClassName, m_SingleBindings, m_ArrayBindingsByMemberName, m_ArrayBindingElements);
        }

        protected override void SerializeBindings()
        {
            List<string> memberNames = new List<string>();
            List<UnityEngine.Object> targets = new List<UnityEngine.Object>();
            foreach (BindingDescriptor binding in m_SingleBindings)
            {
                memberNames.Add(BindingCodeCustomizerRegistry.GetPublicPropertyName(binding.MemberNamePrefix, binding.TargetToken));
                if(!TryGetBindingTarget(binding.SourceTransform, binding.TargetType, out var target))
                {
                    throw new Exception($"Bind '{binding.SourceTransform} - {binding.TargetType}' fail!");
                }
                targets.Add(target);
            }
            foreach (BindingDescriptor binding in m_ArrayBindingElements)
            {
                memberNames.Add(BindingCodeCustomizerRegistry.GetPublicArrayPropertyName(binding.MemberNamePrefix, binding.TargetToken));
                if(!TryGetBindingTarget(binding.SourceTransform, binding.TargetType, out var target))
                {
                    throw new Exception($"Bind '{binding.SourceTransform} - {binding.TargetType}' fail!");
                }
                targets.Add(target);
            }
            m_Host.SetBindingTargets(memberNames.ToArray(), targets.ToArray());
        }
    }
}
