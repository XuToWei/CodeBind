using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// Default binding source customization used when no higher-priority implementation exists.
    /// </summary>
    internal sealed class DefaultBindingCodeCustomizer : IBindingCodeCustomizer
    {
        public int Priority => 0;

        public string GetSerializedFieldName(string memberName)
        {
            return $"m_{memberName}";
        }

        public string GetPublicPropertyName(string memberName)
        {
            return memberName;
        }

        public string BuildAdditionalSource(string namespaceName, string className, List<BindingDescriptor> singleBindings, SortedDictionary<string, List<BindingDescriptor>> arrayBindingsByMemberName, string indentation)
        {
            return string.Empty;
        }
    }
}
