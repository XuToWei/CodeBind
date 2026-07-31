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
        private static readonly Regex s_VariableNameRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

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

        protected bool TryCollectBindings()
        {
            bool TryCollectNodeBindings(Transform child, string[] strArray, ref List<BindingDescriptor> singleBindings)
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
                string variableName = strArray[0];
                for (int i = 1; i < strArray.Length; i++)
                {
                    string typeStr = strArray[i];
                    if (string.Equals(typeStr, "*", StringComparison.OrdinalIgnoreCase))
                    {
                        //自动补齐所有存在的脚本，如果存在继承关系的保留子类即可
                        Type targetType = typeof(GameObject);
                        if (BindingTargetTokenRegistry.TokensByTargetType.TryGetValue(targetType, out var targetToken))
                        {
                            BindingDescriptor binding = new BindingDescriptor(variableName, targetType, targetToken, child);
                            singleBindings.Add(binding);
                        }
                        foreach (var component in m_ComponentBuffer)
                        {
                            targetType = component.GetType();
                            //有继承关系的脚本，脚本部分重名，先判断有没有直接能匹配的
                            if (BindingTargetTokenRegistry.TokensByTargetType.TryGetValue(targetType, out targetToken))
                            {
                                BindingDescriptor binding = new BindingDescriptor(variableName, targetType, targetToken, child);
                                singleBindings.Add(binding);
                            }
                            else
                            {
                                //没有直接匹配，可以找父类可以绑定的
                                foreach (var kv in BindingTargetTokenRegistry.TargetTypesByToken)
                                {
                                    if (targetType.IsSubclassOf(kv.Value) && TryGetBindingTarget(child, kv.Value, out _))
                                    {
                                        BindingDescriptor binding = new BindingDescriptor(variableName, kv.Value, kv.Key, child);
                                        singleBindings.Add(binding);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (BindingTargetTokenRegistry.TargetTypesByToken.TryGetValue(typeStr, out Type type) && TryGetBindingTarget(child, type, out _))
                    {
                        BindingDescriptor binding = new BindingDescriptor(variableName, type, typeStr, child);
                        singleBindings.Add(binding);
                    }
                    else
                    {
                        throw new Exception($"{child.name}的命名中{typeStr}不存在对应的组件类型，绑定失败！");
                    }
                }
                m_ComponentBuffer.Clear();
                if (singleBindings.Count <= 0)
                {
                    throw new Exception("获取的Bind对象个数为0，绑定失败！");
                }
                return true;
            }
            
            bool TryCollectSingleBindings(Transform child, out List<BindingDescriptor> singleBindings)
            {
                singleBindings = new List<BindingDescriptor>();
                string[] strArray = child.name.Split(m_NameSeparator);
                string lastStr = strArray[^1];
                MatchCollection matchCollection = s_ArrayIndexRegex.Matches(lastStr);
                if (matchCollection.Count > 0)
                {
                    return false;
                }
                if (!TryCollectNodeBindings(child, strArray, ref singleBindings))
                {
                    return false;
                }
                return true;
            }
            
            bool TryCollectArrayBindings(Transform child, out List<BindingDescriptor> singleBindings)
            {
                singleBindings = new List<BindingDescriptor>();
                string[] strArray = child.name.Split(m_NameSeparator);
                string lastStr = strArray[^1];
                MatchCollection matchCollection = s_ArrayIndexRegex.Matches(lastStr);
                if (matchCollection.Count < 1)
                {
                    return false;
                }
                for (int i = 0; i < matchCollection.Count; i++)
                {
                    lastStr = lastStr.Replace(matchCollection[i].Value, string.Empty);
                }
                strArray[^1] = lastStr.Replace(" ", string.Empty);
                if (!TryCollectNodeBindings(child, strArray, ref singleBindings))
                {
                    return false;
                }
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
                if (TryCollectSingleBindings(child, out List<BindingDescriptor> singleBindings))
                {
                    foreach (BindingDescriptor binding in singleBindings)
                    {
                        if (m_SingleBindings.Find(data => data.VariableName == binding.VariableName && data.TargetToken == binding.TargetToken) != null)
                        {
                            m_SingleBindings.Clear();
                            throw new Exception($"绑定对象中存在同名[{binding.VariableName}]-[{binding.TargetToken}]-[{binding.SourceTransform}],请修改后重新生成。");
                        }
                        m_SingleBindings.Add(binding);
                    }
                }
                if (TryCollectArrayBindings(child, out List<BindingDescriptor> arrayBindings))
                {
                    foreach (BindingDescriptor binding in arrayBindings)
                    {
                        if (m_ArrayBindingElements.Find(data => data.VariableName == binding.VariableName && data.TargetToken == binding.TargetToken && data.SourceTransform == binding.SourceTransform) != null)
                        {
                            m_ArrayBindingElements.Clear();
                            throw new Exception($"绑定数组对象中存在重复[{binding.VariableName}]-[{binding.TargetToken}]-[{binding.SourceTransform}],请修改后重新生成。");
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
                string arrayName = firstArrayBinding.VariableName + firstArrayBinding.TargetToken;
                if (m_ArrayBindingsByMemberName.TryGetValue(arrayName, out List<BindingDescriptor> singleBindings))
                {
                    continue;
                }
                singleBindings = new List<BindingDescriptor>() { firstArrayBinding };
                m_ArrayBindingsByMemberName.Add(arrayName, singleBindings);
                for (int j = i + 1; j < m_ArrayBindingElements.Count; j++)
                {
                    BindingDescriptor binding = m_ArrayBindingElements[j];
                    if (!string.Equals(binding.VariableName + binding.TargetToken, arrayName, StringComparison.Ordinal))
                    {
                        continue;
                    }
                    singleBindings.Add(binding);
                }
            }
            //进行排序，保证不同名字相同节点顺序不同的预制可以公用绑定脚本
            m_SingleBindings.Sort();
            return true;
        }

        protected void NormalizeBindingNodeNames()
        {
            ValidateBindingConfiguration();
            Dictionary<Transform, string> transformNameDict = new Dictionary<Transform, string>();
            Dictionary<string, List<Transform>> arrayTransformDict = new Dictionary<string, List<Transform>>();
            foreach (Transform child in m_RootTransform.GetComponentsInChildren<Transform>(true))
            {
                if (child == m_RootTransform || !child.name.Contains(m_NameSeparator) || IsNestedUnderAnotherBindingRoot(child))
                {
                    continue;
                }
                List<string> strList = child.name.Split(m_NameSeparator).ToList();
                if(string.IsNullOrEmpty(strList[0]))
                {
                    throw new Exception($"变量名为空：{child.name}");
                }
                if (!s_VariableNameRegex.IsMatch(strList[0]))
                {
                    throw new Exception($"{child.name}的变量名格式不对：{strList[0]}");
                }
                //(xxx)结尾的识别为数组，方便复制
                string lastStr = strList[^1];
                MatchCollection matchCollection = s_ArrayIndexRegex.Matches(lastStr);
                if (matchCollection.Count > 0)
                {
                    if (arrayTransformDict.TryGetValue(strList[0], out List<Transform> transforms))
                    {
                        transforms.Add(child);
                    }
                    else
                    {
                        arrayTransformDict[strList[0]] = new List<Transform>() { child };
                    }
                    for (int i = 0; i < matchCollection.Count; i++)
                    {
                        lastStr = lastStr.Replace(matchCollection[i].Value, string.Empty);
                    }
                    strList[^1] = lastStr.Replace(" ", string.Empty);
                }
                bool hasAll = false;
                for (int i = 1; i < strList.Count; i++)
                {
                    if (string.IsNullOrEmpty(strList[i]))
                    {
                        throw new Exception($"不支持自动补齐名字为空的脚本：{child.name}");
                    }
                    if (string.Equals(strList[1], "*", StringComparison.OrdinalIgnoreCase))
                    {
                        hasAll = true;
                    }
                }
                if (hasAll)
                {
                    strList = new List<string>
                    {
                        strList[0],
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
                    for (int i = 1; i < strList.Count; i++)
                    {
                        string typeStr = strList[i];
                        //有的命名会有局部重复，这里如果脚本存在了就不参加模糊匹配
                        if (BindingTargetTokenRegistry.TargetTypesByToken.TryGetValue(typeStr, out var comType) && TryGetBindingTarget(child, comType, out _))
                        {
                            continue;
                        }
                        Type targetType = typeof(GameObject);
                        if (BindingTargetTokenRegistry.TokensByTargetType.TryGetValue(targetType, out var targetToken) &&
                            (targetToken.Contains(typeStr, StringComparison.OrdinalIgnoreCase) || typeStr.Contains(targetToken, StringComparison.OrdinalIgnoreCase)))
                        {
                            strList[i] = targetToken;
                            continue;
                        }
                        //有继承关系的脚本，脚本部分重名，先判断有没有直接能匹配的
                        bool isContinue = false;
                        foreach (var component in m_ComponentBuffer)
                        {
                            targetType = component.GetType();
                            if (BindingTargetTokenRegistry.TokensByTargetType.TryGetValue(targetType, out targetToken) &&
                                (targetToken.Contains(typeStr, StringComparison.OrdinalIgnoreCase) || typeStr.Contains(targetToken, StringComparison.OrdinalIgnoreCase)))
                            {
                                strList[i] = targetToken;
                                isContinue = true;
                                break;
                            }
                        }
                        if (isContinue)
                        {
                            continue;
                        }
                        //有继承关系的脚本，可以找到父类节点绑定
                        foreach (var kv in BindingTargetTokenRegistry.TargetTypesByToken)
                        {
                            if ((kv.Key.Contains(typeStr, StringComparison.OrdinalIgnoreCase) || typeStr.Contains(kv.Key, StringComparison.OrdinalIgnoreCase)) && TryGetBindingTarget(child, kv.Value, out _))
                            {
                                strList[i] = kv.Key;
                                break;
                            }
                        }
                    }
                    m_ComponentBuffer.Clear();
                }
                for (int i = 1; i < strList.Count - 1; i++)
                {
                    for (int j = i + 1; j < strList.Count; j++)
                    {
                        if (string.Equals(strList[i], strList[j], StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception($"Child:{child} component name is repeated or auto fix repeated!");
                        }
                    }
                }
                transformNameDict.Add(child, string.Join(m_NameSeparator, strList));
            }
            //处理Array
            foreach (KeyValuePair<string, List<Transform>> kv in arrayTransformDict)
            {
                if (kv.Value.Count < 2)
                {
                    continue;
                }
                Transform first = kv.Value[0];
                string firstName = transformNameDict[first];
                for (int i = 1; i < kv.Value.Count; i++)
                {
                    if (transformNameDict[kv.Value[i]] != firstName)
                    {
                        throw new Exception($"Child:{kv.Value[i]} has different component ({transformNameDict[kv.Value[i]]}:{firstName}) in array!");
                    }
                }
                transformNameDict[first] = $"{firstName} ({0})";
                for (int i = 1; i < kv.Value.Count; i++)
                {
                    string name = transformNameDict[kv.Value[i]];
                    transformNameDict[kv.Value[i]] = $"{name} ({i})";
                }
            }

            foreach (KeyValuePair<Transform, string> kv in transformNameDict)
            {
                kv.Key.name = kv.Value;
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

        public void TrySerializeBindings()
        {
            BindingTargetTokenRegistry.EnsureInitialized();
            NormalizeBindingNodeNames();
            if (!TryCollectBindings())
            {
                return;
            }
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