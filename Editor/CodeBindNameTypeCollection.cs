using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal static class CodeBindNameTypeCollection
    {
        internal static readonly Dictionary<string, Type> BindNameTypeDict = new Dictionary<string, Type>();
        internal static readonly Dictionary<Type, string> BindTypeNameDict = new Dictionary<Type, string>();

        internal static void Do()
        {
            if (BindNameTypeDict.Count > 0)
                return;
            var types = TypeCache.GetTypesWithAttribute<CodeBindNameAttribute>();
            foreach (var type in types)
            {
                if (!type.IsSubclassOf(typeof(Component)))
                {
                    Debug.LogError($"[CodeBind] Add BindNameType Fail! Type:{type} error! Only can bind sub class of 'Component'!");
                    continue;
                }
                CodeBindNameAttribute attribute = (CodeBindNameAttribute)type.GetCustomAttributes(typeof(CodeBindNameAttribute), false)[0];
                if (BindNameTypeDict.TryGetValue(attribute.BindName, out Type bindType))
                {
                    Debug.LogError($"[CodeBind] Add BindNameType Fail! Type name:{attribute.BindName}({bindType}) exist!");
                    continue;
                }
                if (BindTypeNameDict.TryGetValue(type, out string bindName))
                {
                    Debug.LogError($"[CodeBind] Add BindNameType Fail! Type name:{bindName}({type}) exist!");
                    continue;
                }
                BindNameTypeDict.Add(attribute.BindName, type);
                BindTypeNameDict.Add(type, attribute.BindName);
            }

            //缺省配置 DefaultCodeBindNameTypeConfig 也会被收集，其优先级为 0
            List<ICodeBindNameTypeConfig> configs = new List<ICodeBindNameTypeConfig>();
            foreach (var type in TypeCache.GetTypesDerivedFrom<ICodeBindNameTypeConfig>())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }
                configs.Add((ICodeBindNameTypeConfig)Activator.CreateInstance(type));
            }
            //优先级高的先处理，已存在则跳过，从而高优先级覆盖低优先级
            configs.Sort((a, b) =>
            {
                int compare = b.Priority.CompareTo(a.Priority);
                if (compare != 0)
                {
                    return compare;
                }
                return string.CompareOrdinal(a.GetType().FullName, b.GetType().FullName);
            });
            foreach (var config in configs)
            {
                if (config.BindNameTypeDict == null)
                {
                    continue;
                }
                foreach (var pair in config.BindNameTypeDict)
                {
                    if (pair.Value == null || !pair.Value.IsSubclassOf(typeof(Component)) && pair.Value != typeof(GameObject))
                    {
                        Debug.LogError($"[CodeBind] Add BindNameType Fail! Type:{pair.Value} error! Only can bind sub class of 'Component'!");
                        continue;
                    }
                    if (!BindNameTypeDict.ContainsKey(pair.Key) && !BindTypeNameDict.ContainsKey(pair.Value))
                    {
                        BindNameTypeDict.Add(pair.Key, pair.Value);
                        BindTypeNameDict.Add(pair.Value, pair.Key);
                    }
                }
            }
        }
    }
}
