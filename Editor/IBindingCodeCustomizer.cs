using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// Customizes generated binding member names and additional source.
    /// </summary>
    public interface IBindingCodeCustomizer
    {
        int Priority { get; }

        string GetSerializedFieldName(string memberName);

        string GetPublicPropertyName(string memberName);

        string BuildAdditionalSource(string namespaceName, string className, List<BindingDescriptor> singleBindings, SortedDictionary<string, List<BindingDescriptor>> arrayBindingsByMemberName, string indentation);
    }
}
