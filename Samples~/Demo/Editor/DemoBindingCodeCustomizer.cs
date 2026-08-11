using System.Collections.Generic;
using System.Text;
using CodeBind.Editor;

namespace CodeBind.Demo.Editor
{
    /// <summary>
    /// IBindingCodeCustomizer example that customizes member names and appends source.
    /// </summary>
    public sealed class DemoBindingCodeCustomizer : IBindingCodeCustomizer
    {
        public int Priority => 1;

        public string GetSerializedFieldName(string memberName)
        {
            return $"_{memberName}";
        }

        public string GetPublicPropertyName(string memberName)
        {
            return memberName;
        }

        public string BuildAdditionalSource(string namespaceName, string className,
            List<BindingDescriptor> singleBindings,
            SortedDictionary<string, List<BindingDescriptor>> arrayBindingsByMemberName,
            string indentation)
        {
            StringBuilder sourceBuilder = new StringBuilder();
            foreach (BindingDescriptor binding in singleBindings)
            {
                sourceBuilder.AppendLine($"{indentation}// member: {GetPublicPropertyName($"{binding.MemberNamePrefix}{binding.TargetToken}")} ({binding.TargetType.Name})");
            }
            foreach (KeyValuePair<string, List<BindingDescriptor>> kv in arrayBindingsByMemberName)
            {
                BindingDescriptor firstArrayBinding = kv.Value[0];
                sourceBuilder.AppendLine($"{indentation}// array member: {GetPublicPropertyName($"{firstArrayBinding.MemberNamePrefix}{firstArrayBinding.TargetToken}Array")} ({firstArrayBinding.TargetType.Name}[{kv.Value.Count}])");
            }
            return sourceBuilder.ToString();
        }
    }
}
