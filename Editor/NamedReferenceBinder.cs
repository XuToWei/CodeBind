using System;
using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// Serializes automatically discovered targets for a named reference host.
    /// </summary>
    internal sealed class NamedReferenceBinder : HierarchyBindingProcessor
    {
        private readonly NamedReferenceBindingHost m_Host;

        public NamedReferenceBinder(NamedReferenceBindingHost host, char nameSeparator) : base(host.transform, nameSeparator)
        {
            m_Host = host;
        }

        protected override void SerializeBindings()
        {
            List<string> keys = new List<string>();
            List<UnityEngine.Object> targets = new List<UnityEngine.Object>();
            foreach (BindingDescriptor binding in m_SingleBindings)
            {
                keys.Add(BindingCodeCustomizerRegistry.GetPublicPropertyName(binding.VariableName, binding.TargetToken));
                if(!TryGetBindingTarget(binding.SourceTransform, binding.TargetType, out var target))
                {
                    throw new Exception($"Bind '{binding.SourceTransform} - {binding.TargetType}' fail!");
                }
                targets.Add(target);
            }
            foreach (BindingDescriptor binding in m_ArrayBindingElements)
            {
                keys.Add(BindingCodeCustomizerRegistry.GetPublicArrayPropertyName(binding.VariableName, binding.TargetToken));
                if(!TryGetBindingTarget(binding.SourceTransform, binding.TargetType, out var target))
                {
                    throw new Exception($"Bind '{binding.SourceTransform} - {binding.TargetType}' fail!");
                }
                targets.Add(target);
            }
            m_Host.SetAutoTargets(keys.ToArray(), targets.ToArray());
        }
    }
}
