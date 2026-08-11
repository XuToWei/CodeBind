using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// 基础绑定器
    /// </summary>
    internal abstract class HierarchyBindingProcessor
    {
        /// <summary>
        /// 匹配数组索引，如 (0), (1), (-1) 等
        /// </summary>
        private static readonly Regex s_ArrayIndexRegex = new Regex(@"\(-?\d*\)$", RegexOptions.Compiled);

        /// <summary>
        /// 匹配有效的 ASCII C# 标识符格式
        /// </summary>
        private static readonly Regex s_MemberNamePrefixRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        protected readonly char m_NameSeparator;
        protected readonly Transform m_RootTransform;
        protected readonly List<BindingDescriptor> m_SingleBindings;
        protected readonly List<BindingDescriptor> m_ArrayBindingElements;
        protected readonly SortedDictionary<string, List<BindingDescriptor>> m_ArrayBindingsByMemberName;

        private readonly List<Component> m_ComponentBuffer;

        protected HierarchyBindingProcessor(Transform rootTransform, char nameSeparator)
        {
            m_RootTransform = rootTransform;
            m_SingleBindings = new List<BindingDescriptor>();
            m_ArrayBindingElements = new List<BindingDescriptor>();
            m_ArrayBindingsByMemberName = new SortedDictionary<string, List<BindingDescriptor>>();
            m_NameSeparator = nameSeparator;
            m_ComponentBuffer = new List<Component>();
        }

        protected void CollectBindings()
        {
            void CollectNodeBindings(Transform child, string[] nameSegments, List<BindingDescriptor> nodeBindings)
            {
                m_ComponentBuffer.Clear();
                child.GetComponents(m_ComponentBuffer);
                for (int i = m_ComponentBuffer.Count -1; i >= 0; i--)
                {
                    if (m_ComponentBuffer[i].hideFlags != HideFlags.None)
                    {
                        m_ComponentBuffer.RemoveAt(i);
                    }
                }
                string memberNamePrefix = nameSegments[0];
                for (int i = 1; i < nameSegments.Length; i++)
                {
                    string targetTokenCandidate = nameSegments[i];
                    if (string.Equals(targetTokenCandidate, "*", StringComparison.OrdinalIgnoreCase))
                    {
                        //自动补齐所有存在的脚本，如果存在继承关系的保留子类即可
                        Type targetType = typeof(GameObject);
                        if (BindingTargetTokenRegistry.TokensByTargetType.TryGetValue(targetType, out var targetToken))
                        {
                            BindingDescriptor binding = new BindingDescriptor(memberNamePrefix, targetType, targetToken, child);
                            nodeBindings.Add(binding);
                        }
                        foreach (var component in m_ComponentBuffer)
                        {
                            targetType = component.GetType();
                            //有继承关系的脚本，脚本部分重名，先判断有没有直接能匹配的
                            if (BindingTargetTokenRegistry.TokensByTargetType.TryGetValue(targetType, out targetToken))
                            {
                                BindingDescriptor binding = new BindingDescriptor(memberNamePrefix, targetType, targetToken, child);
                                nodeBindings.Add(binding);
                            }
                            else
                            {
                                //没有直接匹配，可以找父类可以绑定的
                                foreach (KeyValuePair<string, Type> targetTypeByToken in BindingTargetTokenRegistry.TargetTypesByToken)
                                {
                                    if (targetType.IsSubclassOf(targetTypeByToken.Value) && TryGetBindingTarget(child, targetTypeByToken.Value, out _))
                                    {
                                        BindingDescriptor binding = new BindingDescriptor(memberNamePrefix, targetTypeByToken.Value, targetTypeByToken.Key, child);
                                        nodeBindings.Add(binding);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (BindingTargetTokenRegistry.TargetTypesByToken.TryGetValue(targetTokenCandidate, out Type configuredTargetType) && TryGetBindingTarget(child, configuredTargetType, out _))
                    {
                        BindingDescriptor binding = new BindingDescriptor(memberNamePrefix, configuredTargetType, targetTokenCandidate, child);
                        nodeBindings.Add(binding);
                    }
                    else
                    {
                        throw new Exception($"{child.name}的命名中{targetTokenCandidate}不存在对应的组件类型，绑定失败！");
                    }
                }
                m_ComponentBuffer.Clear();
                if (nodeBindings.Count <= 0)
                {
                    throw new Exception("获取的Bind对象个数为0，绑定失败！");
                }
            }

            bool TryCollectSingleBindings(Transform child, out List<BindingDescriptor> nodeBindings)
            {
                nodeBindings = new List<BindingDescriptor>();
                string[] nameSegments = child.name.Split(m_NameSeparator);
                string lastSegment = nameSegments[^1];
                MatchCollection arrayIndexMatches = s_ArrayIndexRegex.Matches(lastSegment);
                if (arrayIndexMatches.Count > 0)
                {
                    return false;
                }
                CollectNodeBindings(child, nameSegments, nodeBindings);
                return true;
            }
            
            bool TryCollectArrayBindings(Transform child, out List<BindingDescriptor> nodeBindings)
            {
                nodeBindings = new List<BindingDescriptor>();
                string[] nameSegments = child.name.Split(m_NameSeparator);
                string lastSegment = nameSegments[^1];
                MatchCollection arrayIndexMatches = s_ArrayIndexRegex.Matches(lastSegment);
                if (arrayIndexMatches.Count < 1)
                {
                    return false;
                }
                for (int i = 0; i < arrayIndexMatches.Count; i++)
                {
                    lastSegment = lastSegment.Replace(arrayIndexMatches[i].Value, string.Empty);
                }
                nameSegments[^1] = lastSegment.Replace(" ", string.Empty);
                CollectNodeBindings(child, nameSegments, nodeBindings);
                return true;
            }
            
            m_SingleBindings.Clear();
            m_ArrayBindingElements.Clear();
            m_ArrayBindingsByMemberName.Clear();
            foreach (Transform child in m_RootTransform.GetComponentsInChildren<Transform>(true))
            {
                if (child == m_RootTransform || !child.name.Contains(m_NameSeparator) || IsNestedUnderAnotherBindingRoot(child))
                {
                    continue;
                }
                if (TryCollectSingleBindings(child, out List<BindingDescriptor> nodeBindings))
                {
                    foreach (BindingDescriptor binding in nodeBindings)
                    {
                        if (m_SingleBindings.Find(existingBinding => existingBinding.MemberNamePrefix == binding.MemberNamePrefix && existingBinding.TargetToken == binding.TargetToken) != null)
                        {
                            m_SingleBindings.Clear();
                            throw new Exception($"绑定对象中存在同名[{binding.MemberNamePrefix}]-[{binding.TargetToken}]-[{binding.SourceTransform}],请修改后重新生成。");
                        }
                        m_SingleBindings.Add(binding);
                    }
                }
                if (TryCollectArrayBindings(child, out List<BindingDescriptor> arrayBindings))
                {
                    foreach (BindingDescriptor binding in arrayBindings)
                    {
                        if (m_ArrayBindingElements.Find(existingBinding => existingBinding.MemberNamePrefix == binding.MemberNamePrefix && existingBinding.TargetToken == binding.TargetToken && existingBinding.SourceTransform == binding.SourceTransform) != null)
                        {
                            m_ArrayBindingElements.Clear();
                            throw new Exception($"绑定数组对象中存在重复[{binding.MemberNamePrefix}]-[{binding.TargetToken}]-[{binding.SourceTransform}],请修改后重新生成。");
                        }
                        m_ArrayBindingElements.Add(binding);
                    }
                }
            }
            if (m_SingleBindings.Count < 1 && m_ArrayBindingElements.Count < 1)
            {
                throw new Exception("绑定数量为0，生成失败。");
            }
            for (int i = 0; i < m_ArrayBindingElements.Count - 1; i++)
            {
                BindingDescriptor firstArrayBinding = m_ArrayBindingElements[i];
                string arrayMemberName = firstArrayBinding.MemberNamePrefix + firstArrayBinding.TargetToken;
                if (m_ArrayBindingsByMemberName.TryGetValue(arrayMemberName, out List<BindingDescriptor> nodeBindings))
                {
                    continue;
                }
                nodeBindings = new List<BindingDescriptor>() { firstArrayBinding };
                m_ArrayBindingsByMemberName.Add(arrayMemberName, nodeBindings);
                for (int j = i + 1; j < m_ArrayBindingElements.Count; j++)
                {
                    BindingDescriptor binding = m_ArrayBindingElements[j];
                    if (!string.Equals(binding.MemberNamePrefix + binding.TargetToken, arrayMemberName, StringComparison.Ordinal))
                    {
                        continue;
                    }
                    nodeBindings.Add(binding);
                }
            }
            //进行排序，保证不同名字相同节点顺序不同的预制可以公用绑定脚本
            m_SingleBindings.Sort();
        }

        protected void NormalizeBindingNodeNames()
        {
            ValidateBindingConfiguration();
            Dictionary<Transform, string> normalizedNamesByTransform = new Dictionary<Transform, string>();
            Dictionary<string, List<Transform>> arrayElementsByMemberNamePrefix = new Dictionary<string, List<Transform>>();
            foreach (Transform child in m_RootTransform.GetComponentsInChildren<Transform>(true))
            {
                if (child == m_RootTransform || !child.name.Contains(m_NameSeparator) || IsNestedUnderAnotherBindingRoot(child))
                {
                    continue;
                }
                List<string> nameSegments = child.name.Split(m_NameSeparator).ToList();
                if(string.IsNullOrEmpty(nameSegments[0]))
                {
                    throw new Exception($"成员名前缀为空：{child.name}");
                }
                if (!s_MemberNamePrefixRegex.IsMatch(nameSegments[0]))
                {
                    throw new Exception($"{child.name}的成员名前缀格式不对：{nameSegments[0]}");
                }
                //(xxx)结尾的识别为数组，方便复制
                string lastSegment = nameSegments[^1];
                MatchCollection arrayIndexMatches = s_ArrayIndexRegex.Matches(lastSegment);
                if (arrayIndexMatches.Count > 0)
                {
                    if (arrayElementsByMemberNamePrefix.TryGetValue(nameSegments[0], out List<Transform> transforms))
                    {
                        transforms.Add(child);
                    }
                    else
                    {
                        arrayElementsByMemberNamePrefix[nameSegments[0]] = new List<Transform>() { child };
                    }
                    for (int i = 0; i < arrayIndexMatches.Count; i++)
                    {
                        lastSegment = lastSegment.Replace(arrayIndexMatches[i].Value, string.Empty);
                    }
                    nameSegments[^1] = lastSegment.Replace(" ", string.Empty);
                }
                bool hasAll = false;
                for (int i = 1; i < nameSegments.Count; i++)
                {
                    if (string.IsNullOrEmpty(nameSegments[i]))
                    {
                        throw new Exception($"不支持自动补齐名字为空的脚本：{child.name}");
                    }
                    if (string.Equals(nameSegments[1], "*", StringComparison.OrdinalIgnoreCase))
                    {
                        hasAll = true;
                    }
                }
                if (hasAll)
                {
                    nameSegments = new List<string>
                    {
                        nameSegments[0],
                        "*"
                    };
                }
                else
                {
                    m_ComponentBuffer.Clear();
                    child.GetComponents(m_ComponentBuffer);
                    for (int i = m_ComponentBuffer.Count -1; i >= 0; i--)
                    {
                        if (m_ComponentBuffer[i].hideFlags != HideFlags.None)
                        {
                            m_ComponentBuffer.RemoveAt(i);
                        }
                    }
                    //自动补齐名字残缺的
                    for (int i = 1; i < nameSegments.Count; i++)
                    {
                        string targetTokenCandidate = nameSegments[i];
                        //有的命名会有局部重复，这里如果脚本存在了就不参加模糊匹配
                        if (BindingTargetTokenRegistry.TargetTypesByToken.TryGetValue(targetTokenCandidate, out var componentType) && TryGetBindingTarget(child, componentType, out _))
                        {
                            continue;
                        }
                        Type targetType = typeof(GameObject);
                        if (BindingTargetTokenRegistry.TokensByTargetType.TryGetValue(targetType, out var targetToken) &&
                            (targetToken.Contains(targetTokenCandidate, StringComparison.OrdinalIgnoreCase) || targetTokenCandidate.Contains(targetToken, StringComparison.OrdinalIgnoreCase)))
                        {
                            nameSegments[i] = targetToken;
                            continue;
                        }
                        //有继承关系的脚本，脚本部分重名，先判断有没有直接能匹配的
                        bool targetTokenResolved = false;
                        foreach (var component in m_ComponentBuffer)
                        {
                            targetType = component.GetType();
                            if (BindingTargetTokenRegistry.TokensByTargetType.TryGetValue(targetType, out targetToken) &&
                                (targetToken.Contains(targetTokenCandidate, StringComparison.OrdinalIgnoreCase) || targetTokenCandidate.Contains(targetToken, StringComparison.OrdinalIgnoreCase)))
                            {
                                nameSegments[i] = targetToken;
                                targetTokenResolved = true;
                                break;
                            }
                        }
                        if (targetTokenResolved)
                        {
                            continue;
                        }
                        //有继承关系的脚本，可以找到父类节点绑定
                        foreach (KeyValuePair<string, Type> targetTypeByToken in BindingTargetTokenRegistry.TargetTypesByToken)
                        {
                            if ((targetTypeByToken.Key.Contains(targetTokenCandidate, StringComparison.OrdinalIgnoreCase) || targetTokenCandidate.Contains(targetTypeByToken.Key, StringComparison.OrdinalIgnoreCase)) && TryGetBindingTarget(child, targetTypeByToken.Value, out _))
                            {
                                nameSegments[i] = targetTypeByToken.Key;
                                break;
                            }
                        }
                    }
                    m_ComponentBuffer.Clear();
                }
                for (int i = 1; i < nameSegments.Count - 1; i++)
                {
                    for (int j = i + 1; j < nameSegments.Count; j++)
                    {
                        if (string.Equals(nameSegments[i], nameSegments[j], StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception($"Child:{child} component name is repeated or auto fix repeated!");
                        }
                    }
                }
                normalizedNamesByTransform.Add(child, string.Join(m_NameSeparator, nameSegments));
            }
            //处理Array
            foreach (KeyValuePair<string, List<Transform>> arrayElementsByPrefix in arrayElementsByMemberNamePrefix)
            {
                if (arrayElementsByPrefix.Value.Count < 2)
                {
                    continue;
                }
                Transform firstArrayElement = arrayElementsByPrefix.Value[0];
                string firstElementName = normalizedNamesByTransform[firstArrayElement];
                for (int i = 1; i < arrayElementsByPrefix.Value.Count; i++)
                {
                    if (normalizedNamesByTransform[arrayElementsByPrefix.Value[i]] != firstElementName)
                    {
                        throw new Exception($"Child:{arrayElementsByPrefix.Value[i]} has different component ({normalizedNamesByTransform[arrayElementsByPrefix.Value[i]]}:{firstElementName}) in array!");
                    }
                }
                normalizedNamesByTransform[firstArrayElement] = $"{firstElementName} ({0})";
                for (int i = 1; i < arrayElementsByPrefix.Value.Count; i++)
                {
                    string elementName = normalizedNamesByTransform[arrayElementsByPrefix.Value[i]];
                    normalizedNamesByTransform[arrayElementsByPrefix.Value[i]] = $"{elementName} ({i})";
                }
            }

            foreach (KeyValuePair<Transform, string> normalizedNameByTransform in normalizedNamesByTransform)
            {
                normalizedNameByTransform.Key.name = normalizedNameByTransform.Value;
            }
        }

        protected bool TryGetBindingTarget(Transform transform, Type type, out UnityEngine.Object target)
        {
            if (type == typeof(GameObject))
            {
                target = transform.gameObject;
                return true;
            }
            target = transform.GetComponent(type);
            return target != null;
        }

        private bool IsNestedUnderAnotherBindingRoot(Transform transform)
        {
            transform = transform.parent;
            //检查父节点有没有bind，支持bind嵌套
            bool nearestCodeBind = true;
            while (transform != null)
            {
                //子节点可以绑定，创建代码类型不需要判断特性
                if(transform == m_RootTransform)
                {
                    return false;
                }
                MonoBehaviour[] components = transform.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour component in components)
                {
                    if (component.GetType().GetCustomAttributes(typeof(BindingRootAttribute), true).Length > 0)
                    {
                        if (nearestCodeBind && transform == m_RootTransform)
                        {
                            return false;
                        }
                        if (transform != m_RootTransform)
                        {
                            return true;
                        }
                        nearestCodeBind = false;
                    }
                }
                transform = transform.parent;
            }
            return false;
        }

        public void UpdateSerializedBindings()
        {
            BindingTargetTokenRegistry.EnsureInitialized();
            NormalizeBindingNodeNames();
            CollectBindings();
            SerializeBindings();
            EditorUtility.SetDirty(m_RootTransform);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private void ValidateBindingConfiguration()
        {
            foreach (var targetToken in BindingTargetTokenRegistry.TargetTypesByToken.Keys)
            {
                if (targetToken.Contains(m_NameSeparator, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"绑定名[{targetToken}]中不能含有分隔符[{m_NameSeparator}]。");
                }
            }
        }

        protected abstract void SerializeBindings();
    }
}