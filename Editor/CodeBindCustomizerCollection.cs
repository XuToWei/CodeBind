using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal static class CodeBindCustomizerCollection
    {
        private static ICodeBindCustomizer s_Customizer;

        internal static void Do()
        {
            if (s_Customizer != null)
            {
                return;
            }
            //默认实现 DefaultCodeBindCustomizer 也会被收集，其优先级为 0
            List<ICodeBindCustomizer> customizers = new List<ICodeBindCustomizer>();
            foreach (var type in TypeCache.GetTypesDerivedFrom<ICodeBindCustomizer>())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }
                customizers.Add((ICodeBindCustomizer)Activator.CreateInstance(type));
            }
            //优先级高的排在前面，优先级相同时按类型全名排序保证确定性
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
                Debug.LogError($"[CodeBind] Multiple ICodeBindCustomizer with the same priority {customizers[0].Priority}, use '{customizers[0].GetType().FullName}'!");
            }
            s_Customizer = customizers[0];
        }

        //数组绑定固定追加的后缀，由框架统一拼接，不再暴露给 ICodeBindCustomizer
        private const string ArraySuffix = "Array";

        internal static string GetFieldName(string bindName, string bindPrefix)
        {
            Do();
            return s_Customizer.GetFieldName($"{bindName}{bindPrefix}");
        }

        internal static string GetPropertyName(string bindName, string bindPrefix)
        {
            Do();
            return s_Customizer.GetPropertyName($"{bindName}{bindPrefix}");
        }

        internal static string GetArrayFieldName(string bindName, string bindPrefix)
        {
            Do();
            return s_Customizer.GetFieldName($"{bindName}{bindPrefix}{ArraySuffix}");
        }

        internal static string GetArrayPropertyName(string bindName, string bindPrefix)
        {
            Do();
            return s_Customizer.GetPropertyName($"{bindName}{bindPrefix}{ArraySuffix}");
        }

        internal static string GenerateExtraCode(string nameSpace, string className, List<CodeBindData> bindDatas, SortedDictionary<string, List<CodeBindData>> bindArrayDataDict, string indentation)
        {
            Do();
            string code = s_Customizer.GenerateExtraCode(nameSpace, className, bindDatas, bindArrayDataDict, indentation);
            return string.IsNullOrEmpty(code) ? string.Empty : code;
        }
    }
}
