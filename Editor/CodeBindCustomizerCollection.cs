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

        internal static string GetFieldName(string bindName, string bindPrefix)
        {
            Do();
            return s_Customizer.GetFieldName(bindName, bindPrefix);
        }

        internal static string GetPropertyName(string bindName, string bindPrefix)
        {
            Do();
            return s_Customizer.GetPropertyName(bindName, bindPrefix);
        }

        internal static string GetArrayFieldName(string bindName, string bindPrefix)
        {
            Do();
            return s_Customizer.GetArrayFieldName(bindName, bindPrefix);
        }

        internal static string GetArrayPropertyName(string bindName, string bindPrefix)
        {
            Do();
            return s_Customizer.GetArrayPropertyName(bindName, bindPrefix);
        }

        internal static string GenerateExtraCode(string nameSpace, string className, List<CodeBindData> bindDatas, SortedDictionary<string, List<CodeBindData>> bindArrayDataDict, string indentation)
        {
            Do();
            List<CodeBindMemberInfo> members = new List<CodeBindMemberInfo>();
            foreach (CodeBindData bindData in bindDatas)
            {
                string name = GetPropertyName(bindData.BindName, bindData.BindPrefix);
                members.Add(new CodeBindMemberInfo(name, bindData.BindType, bindData.BindTransform));
            }
            List<CodeBindArrayMemberInfo> arrayMembers = new List<CodeBindArrayMemberInfo>();
            foreach (KeyValuePair<string, List<CodeBindData>> kv in bindArrayDataDict)
            {
                CodeBindData firstBindData = kv.Value[0];
                string name = GetArrayPropertyName(firstBindData.BindName, firstBindData.BindPrefix);
                List<Transform> transforms = new List<Transform>();
                foreach (CodeBindData bindData in kv.Value)
                {
                    transforms.Add(bindData.BindTransform);
                }
                arrayMembers.Add(new CodeBindArrayMemberInfo(name, firstBindData.BindType, transforms));
            }
            string code = s_Customizer.GenerateExtraCode(nameSpace, className, members, arrayMembers, indentation);
            return string.IsNullOrEmpty(code) ? string.Empty : code;
        }
    }
}
