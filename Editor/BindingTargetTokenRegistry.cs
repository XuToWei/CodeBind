using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal static class BindingTargetTokenRegistry
    {
        internal static readonly Dictionary<string, Type> TargetTypesByToken = new Dictionary<string, Type>();
        internal static readonly Dictionary<Type, string> TokensByTargetType = new Dictionary<Type, string>();

        internal static void EnsureInitialized()
        {
            if (TargetTypesByToken.Count > 0)
                return;
            var attributedTypes = TypeCache.GetTypesWithAttribute<BindingTargetTokenAttribute>();
            foreach (var type in attributedTypes)
            {
                if (!type.IsSubclassOf(typeof(Component)))
                {
                    Debug.LogError($"[CodeBind] Add BindNameType Fail! Type:{type} error! Only can bind sub class of 'Component'!");
                    continue;
                }
                BindingTargetTokenAttribute attribute = (BindingTargetTokenAttribute)type.GetCustomAttributes(typeof(BindingTargetTokenAttribute), false)[0];
                if (TargetTypesByToken.TryGetValue(attribute.Token, out Type targetType))
                {
                    Debug.LogError($"[CodeBind] Add BindNameType Fail! Type name:{attribute.Token}({targetType}) exist!");
                    continue;
                }
                if (TokensByTargetType.TryGetValue(type, out string token))
                {
                    Debug.LogError($"[CodeBind] Add BindNameType Fail! Type name:{token}({type}) exist!");
                    continue;
                }
                TargetTypesByToken.Add(attribute.Token, type);
                TokensByTargetType.Add(type, attribute.Token);
            }

            List<IBindingTargetTokenConfig> tokenConfigs = new List<IBindingTargetTokenConfig>();
            foreach (var type in TypeCache.GetTypesDerivedFrom<IBindingTargetTokenConfig>())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }
                tokenConfigs.Add((IBindingTargetTokenConfig)Activator.CreateInstance(type));
            }
            tokenConfigs.Sort((a, b) =>
            {
                int compare = b.Priority.CompareTo(a.Priority);
                if (compare != 0)
                {
                    return compare;
                }
                return string.CompareOrdinal(a.GetType().FullName, b.GetType().FullName);
            });
            foreach (var tokenConfig in tokenConfigs)
            {
                if (tokenConfig.TargetTypesByToken == null)
                {
                    continue;
                }
                foreach (var mapping in tokenConfig.TargetTypesByToken)
                {
                    if (mapping.Value == null || !mapping.Value.IsSubclassOf(typeof(Component)) && mapping.Value != typeof(GameObject))
                    {
                        Debug.LogError($"[CodeBind] Add BindNameType Fail! Type:{mapping.Value} error! Only can bind sub class of 'Component'!");
                        continue;
                    }
                    if (!TargetTypesByToken.ContainsKey(mapping.Key) && !TokensByTargetType.ContainsKey(mapping.Value))
                    {
                        TargetTypesByToken.Add(mapping.Key, mapping.Value);
                        TokensByTargetType.Add(mapping.Value, mapping.Key);
                    }
                }
            }
        }
    }
}
