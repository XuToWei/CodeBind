using System;
using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// Serializes generated references for a named reference host.
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
            List<string> generatedReferenceKeys = new List<string>();
            List<UnityEngine.Object> generatedReferences = new List<UnityEngine.Object>();
            foreach (BindingDescriptor binding in m_SingleBindings)
            {
                generatedReferenceKeys.Add(BindingCodeCustomizerRegistry.GetPublicPropertyName(binding.MemberNamePrefix, binding.TargetToken));
                if(!TryGetBindingTarget(binding.SourceTransform, binding.TargetType, out var target))
                {
                    throw new Exception($"Bind '{binding.SourceTransform} - {binding.TargetType}' fail!");
                }
                generatedReferences.Add(target);
            }
            foreach (BindingDescriptor binding in m_ArrayBindingElements)
            {
                generatedReferenceKeys.Add(BindingCodeCustomizerRegistry.GetPublicArrayPropertyName(binding.MemberNamePrefix, binding.TargetToken));
                if(!TryGetBindingTarget(binding.SourceTransform, binding.TargetType, out var target))
                {
                    throw new Exception($"Bind '{binding.SourceTransform} - {binding.TargetType}' fail!");
                }
                generatedReferences.Add(target);
            }
            m_Host.SetGeneratedReferences(generatedReferenceKeys.ToArray(), generatedReferences.ToArray());
        }
    }
}
