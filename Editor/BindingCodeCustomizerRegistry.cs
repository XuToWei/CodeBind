using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal static class BindingCodeCustomizerRegistry
    {
        private static IBindingCodeCustomizer s_ActiveCustomizer;

        internal static void EnsureInitialized()
        {
            if (s_ActiveCustomizer != null)
            {
                return;
            }
            List<IBindingCodeCustomizer> customizers = new List<IBindingCodeCustomizer>();
            foreach (var type in TypeCache.GetTypesDerivedFrom<IBindingCodeCustomizer>())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }
                customizers.Add((IBindingCodeCustomizer)Activator.CreateInstance(type));
            }
            customizers.Sort((a, b) =>
            {
                int compare = b.Priority.CompareTo(a.Priority);
                if (compare != 0)
                {
                    return compare;
                }
                return string.CompareOrdinal(a.GetType().FullName, b.GetType().FullName);
            });
            if (customizers.Count > 1 && customizers[0].Priority == customizers[1].Priority)
            {
                Debug.LogError($"[CodeBind] Multiple IBindingCodeCustomizer with the same priority {customizers[0].Priority}, use '{customizers[0].GetType().FullName}'!");
            }
            s_ActiveCustomizer = customizers[0];
        }

        private const string ArraySuffix = "Array";

        internal static string GetSerializedFieldName(string variableName, string targetToken)
        {
            EnsureInitialized();
            return s_ActiveCustomizer.GetSerializedFieldName($"{variableName}{targetToken}");
        }

        internal static string GetPublicPropertyName(string variableName, string targetToken)
        {
            EnsureInitialized();
            return s_ActiveCustomizer.GetPublicPropertyName($"{variableName}{targetToken}");
        }

        internal static string GetSerializedArrayFieldName(string variableName, string targetToken)
        {
            EnsureInitialized();
            return s_ActiveCustomizer.GetSerializedFieldName($"{variableName}{targetToken}{ArraySuffix}");
        }

        internal static string GetPublicArrayPropertyName(string variableName, string targetToken)
        {
            EnsureInitialized();
            return s_ActiveCustomizer.GetPublicPropertyName($"{variableName}{targetToken}{ArraySuffix}");
        }

        internal static string BuildAdditionalSource(string namespaceName, string className, List<BindingDescriptor> singleBindings, SortedDictionary<string, List<BindingDescriptor>> arrayBindingsByMemberName, string indentation)
        {
            EnsureInitialized();
            string source = s_ActiveCustomizer.BuildAdditionalSource(namespaceName, className, singleBindings, arrayBindingsByMemberName, indentation);
            return string.IsNullOrEmpty(source) ? string.Empty : source;
        }
    }
}
