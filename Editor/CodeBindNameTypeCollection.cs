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
            var fieldInfos = TypeCache.GetFieldsWithAttribute<CodeBindNameTypeAttribute>();
            Type fieldType = typeof(Dictionary<string, Type>);
            foreach (var fieldInfo in fieldInfos)
            {
                if (!fieldInfo.IsStatic)
                {
                    Debug.LogError($"Get BindNameType Fail! {fieldInfo.Name} is not static!");
                    continue;
                }
                if (fieldInfo.FieldType != fieldType)
                {
                    Debug.LogError($"Get BindNameType Fail! {fieldInfo.Name} is not {fieldType}!");
                    continue;
                }
                object value = fieldInfo.GetValue(null);
                if (value == null)
                {
                    Debug.LogError($"Get BindNameType Fail! {fieldInfo.Name} is null!");
                    continue;
                }
                Dictionary<string, Type> bindNameTypeDict = (Dictionary<string, Type>)value;
                foreach (var kv in bindNameTypeDict)
                {
                    if (kv.Value == null || !kv.Value.IsSubclassOf(typeof(Component)) && kv.Value != typeof(GameObject))
                    {
                        Debug.LogError($"Add BindNameType Fail! Type:{kv.Value} error! Only can bind sub class of 'Component'!");
                        continue;
                    }
                    if (BindNameTypeDict.TryGetValue(kv.Key, out Type type))
                    {
                        Debug.LogError($"Add BindNameType Fail! Type name:{kv.Key}({type}) exist!");
                        continue;
                    }
                    if (BindTypeNameDict.TryGetValue(kv.Value, out string name))
                    {
                        Debug.LogError($"Add BindNameType Fail! Type name:{name}({kv.Value}) exist!");
                        continue;
                    }
                    BindNameTypeDict.Add(kv.Key, kv.Value);
                    BindTypeNameDict.Add(kv.Value, kv.Key);
                }
            }

            var types = TypeCache.GetTypesWithAttribute<CodeBindNameAttribute>();
            foreach (var type in types)
            {
                if (!type.IsSubclassOf(typeof(Component)))
                {
                    Debug.LogError($"Add BindNameType Fail! Type:{type} error! Only can bind sub class of 'Component'!");
                    continue;
                }
                CodeBindNameAttribute attribute = (CodeBindNameAttribute)type.GetCustomAttributes(typeof(CodeBindNameAttribute), false)[0];
                if (BindNameTypeDict.TryGetValue(attribute.BindName, out Type bindType))
                {
                    Debug.LogError($"Add BindNameType Fail! Type name:{attribute.BindName}({bindType}) exist!");
                    continue;
                }
                if (BindTypeNameDict.TryGetValue(type, out string bindName))
                {
                    Debug.LogError($"Add BindNameType Fail! Type name:{bindName}({type}) exist!");
                    continue;
                }
                BindNameTypeDict.Add(attribute.BindName, type);
                BindTypeNameDict.Add(type, attribute.BindName);
            }

            foreach (var pair in DefaultCodeBindNameTypeConfig.BindNameTypeDict)
            {
                if (!BindNameTypeDict.ContainsKey(pair.Key) && !BindTypeNameDict.ContainsKey(pair.Value))
                {
                    BindNameTypeDict.Add(pair.Key, pair.Value);
                    BindTypeNameDict.Add(pair.Value, pair.Key);
                }
            }
        }
    }
}
