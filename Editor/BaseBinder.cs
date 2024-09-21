using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal abstract class BaseBinder
    {
        private readonly char m_SeparatorChar;

        private readonly Transform m_RootTransform;

        protected readonly List<CodeBindData> m_BindDatas;
        protected readonly List<CodeBindData> m_BindArrayDatas;
        protected readonly SortedDictionary<string, List<CodeBindData>> m_BindArrayDataDict;

        private readonly Regex m_ArrayIndexRegex;
        private readonly Regex m_VariableNameRegex;

        private readonly List<Component> m_ComponentCacheList;

        protected BaseBinder(Transform rootTransform, char separatorChar)
        {
            m_RootTransform = rootTransform;
            m_BindDatas = new List<CodeBindData>();
            m_BindArrayDatas = new List<CodeBindData>();
            m_BindArrayDataDict = new SortedDictionary<string, List<CodeBindData>>();
            m_SeparatorChar = separatorChar;
            m_ArrayIndexRegex = new Regex(@"\(-?\d*\)$");
            m_VariableNameRegex = new Regex(@"^([A-Za-z0-9\._-]+/)*[A-Za-z0-9\._-]+$");
            m_ComponentCacheList = new List<Component>();
        }

        protected bool TryGenerateNameMapTypeData()
        {
            bool TryGetBindDatas(Transform child, string[] strArray, ref List<CodeBindData> bindDatas)
            {
                m_ComponentCacheList.Clear();
                child.GetComponents(m_ComponentCacheList);
                string bindName = strArray[0];
                for (int i = 1; i < strArray.Length; i++)
                {
                    string typeStr = strArray[i];
                    if (string.Equals(typeStr, "*", StringComparison.OrdinalIgnoreCase))
                    {
                        //自动补齐所有存在的脚本，如果存在继承关系的保留子类即可
                        Type bindType = typeof(GameObject);
                        if (CodeBindNameTypeCollection.BindTypeNameDict.TryGetValue(bindType, out var bindPrefix))
                        {
                            CodeBindData bindData = new CodeBindData(bindName, bindType, bindPrefix, child);
                            bindDatas.Add(bindData);
                        }
                        foreach (var component in m_ComponentCacheList)
                        {
                            bindType = component.GetType();
                            //有继承关系的脚本，脚本部分重名，先判断有没有直接能匹配的
                            if (CodeBindNameTypeCollection.BindTypeNameDict.TryGetValue(bindType, out bindPrefix))
                            {
                                CodeBindData bindData = new CodeBindData(bindName, bindType, bindPrefix, child);
                                bindDatas.Add(bindData);
                            }
                            else
                            {
                                //没有直接匹配，可以找父类可以绑定的
                                foreach (var kv in CodeBindNameTypeCollection.BindNameTypeDict)
                                {
                                    if (bindType.IsSubclassOf(kv.Value) && TryGetBindTarget(child, kv.Value, out _))
                                    {
                                        CodeBindData bindData = new CodeBindData(bindName, kv.Value, kv.Key, child);
                                        bindDatas.Add(bindData);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (CodeBindNameTypeCollection.BindNameTypeDict.TryGetValue(typeStr, out Type type) && TryGetBindTarget(child, type, out _))
                    {
                        CodeBindData bindData = new CodeBindData(bindName, type, typeStr, child);
                        bindDatas.Add(bindData);
                    }
                    else
                    {
                        throw new Exception($"{child.name}的命名中{typeStr}不存在对应的组件类型，绑定失败！");
                    }
                }
                m_ComponentCacheList.Clear();
                if (bindDatas.Count <= 0)
                {
                    throw new Exception("获取的Bind对象个数为0，绑定失败！");
                }
                return true;
            }
            
            bool TryGetBindComponents(Transform child, out List<CodeBindData> bindDatas)
            {
                bindDatas = new List<CodeBindData>();
                string[] strArray = child.name.Split(m_SeparatorChar);
                string lastStr = strArray[^1];
                MatchCollection matchCollection = m_ArrayIndexRegex.Matches(lastStr);
                if (matchCollection.Count > 0)
                {
                    return false;
                }
                if (!TryGetBindDatas(child, strArray, ref bindDatas))
                {
                    return false;
                }
                return true;
            }
            
            bool TryGetBindArrayComponents(Transform child, out List<CodeBindData> bindDatas)
            {
                bindDatas = new List<CodeBindData>();
                string[] strArray = child.name.Split(m_SeparatorChar);
                string lastStr = strArray[^1];
                MatchCollection matchCollection = m_ArrayIndexRegex.Matches(lastStr);
                if (matchCollection.Count < 1)
                {
                    return false;
                }
                for (int i = 0; i < matchCollection.Count; i++)
                {
                    lastStr = lastStr.Replace(matchCollection[i].Value, string.Empty);
                }
                strArray[^1] = lastStr.Replace(" ", string.Empty);
                if (!TryGetBindDatas(child, strArray, ref bindDatas))
                {
                    return false;
                }
                return true;
            }
            
            m_BindDatas.Clear();
            m_BindArrayDatas.Clear();
            m_BindArrayDataDict.Clear();
#if STATE_CONTROLLER_CODE_BIND
            //如果根节点有StateControllerMono，生成Root
            Type scmType = typeof(StateController.StateControllerMono);
            if (CodeBindNameTypeCollection.BindTypeNameDict.TryGetValue(scmType, out var bindPrefix))
            {
                StateController.StateControllerMono scm = m_RootTransform.GetComponent<StateController.StateControllerMono>();
                if (scm != null)
                {
                    CodeBindData bindData = new CodeBindData("Root", scmType, bindPrefix, m_RootTransform);
                    m_BindDatas.Add(bindData);
                }
            }
#endif
            foreach (Transform child in m_RootTransform.GetComponentsInChildren<Transform>(true))
            {
                if (child == m_RootTransform || !child.name.Contains(m_SeparatorChar) || CheckIsInOtherBind(child))
                {
                    continue;
                }
                if (TryGetBindComponents(child, out List<CodeBindData> bindDatas))
                {
                    foreach (CodeBindData bindData in bindDatas)
                    {
                        if (m_BindDatas.Find(data => data.BindName == bindData.BindName && data.BindPrefix == bindData.BindPrefix) != null)
                        {
                            m_BindDatas.Clear();
                            throw new Exception($"绑定对象中存在同名[{bindData.BindName}]-[{bindData.BindPrefix}]-[{bindData.BindTransform}],请修改后重新生成。");
                        }
                        m_BindDatas.Add(bindData);
                    }
                }
                if (TryGetBindArrayComponents(child, out List<CodeBindData> bindArrayDatas))
                {
                    foreach (CodeBindData bindData in bindArrayDatas)
                    {
                        if (m_BindArrayDatas.Find(data => data.BindName == bindData.BindName && data.BindPrefix == bindData.BindPrefix && data.BindTransform == bindData.BindTransform) != null)
                        {
                            m_BindArrayDatas.Clear();
                            throw new Exception($"绑定数组对象中存在重复[{bindData.BindName}]-[{bindData.BindPrefix}]-[{bindData.BindTransform}],请修改后重新生成。");
                        }
                        m_BindArrayDatas.Add(bindData);
                    }
                }
            }
            if (m_BindDatas.Count < 1 && m_BindArrayDatas.Count < 1)
            {
                throw new Exception("绑定数量为0，生成失败。");
            }
            for (int i = 0; i < m_BindArrayDatas.Count - 1; i++)
            {
                CodeBindData firstBindData = m_BindArrayDatas[i];
                string arrayName = firstBindData.BindName + firstBindData.BindPrefix;
                if (m_BindArrayDataDict.TryGetValue(arrayName, out List<CodeBindData> bindDatas))
                {
                    continue;
                }
                bindDatas = new List<CodeBindData>() { firstBindData };
                m_BindArrayDataDict.Add(arrayName, bindDatas);
                for (int j = i + 1; j < m_BindArrayDatas.Count; j++)
                {
                    CodeBindData bindData = m_BindArrayDatas[j];
                    if (!string.Equals(bindData.BindName + bindData.BindPrefix, arrayName, StringComparison.Ordinal))
                    {
                        continue;
                    }
                    bindDatas.Add(bindData);
                }
            }
            return true;
        }

        protected void AutoFixChildBindName()
        {
            DoCheck();
            Dictionary<Transform, string> transformNameDict = new Dictionary<Transform, string>();
            Dictionary<string, List<Transform>> arrayTransformDict = new Dictionary<string, List<Transform>>();
            foreach (Transform child in m_RootTransform.GetComponentsInChildren<Transform>(true))
            {
                if (child == m_RootTransform || !child.name.Contains(m_SeparatorChar) || CheckIsInOtherBind(child))
                {
                    continue;
                }
                List<string> strList = child.name.Split(m_SeparatorChar).ToList();
                if(string.IsNullOrEmpty(strList[0]))
                {
                    throw new Exception($"变量名为空：{child.name}");
                }
                if (!m_VariableNameRegex.IsMatch(strList[0]))
                {
                    throw new Exception($"{child.name}的变量名格式不对：{strList[0]}");
                }
                //(xxx)结尾的识别为数组，方便复制
                string lastStr = strList[^1];
                MatchCollection matchCollection = m_ArrayIndexRegex.Matches(lastStr);
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
                    m_ComponentCacheList.Clear();
                    child.GetComponents(m_ComponentCacheList);
                    //自动补齐名字残缺的
                    for (int i = 1; i < strList.Count; i++)
                    {
                        string typeStr = strList[i];
                        //有的命名会有局部重复，这里如果脚本存在了就不参加模糊匹配
                        if (CodeBindNameTypeCollection.BindNameTypeDict.TryGetValue(typeStr, out var comType) && TryGetBindTarget(child, comType, out _))
                        {
                            continue;
                        }
                        Type bindType = typeof(GameObject);
                        if (CodeBindNameTypeCollection.BindTypeNameDict.TryGetValue(bindType, out var bindPrefix) &&
                            (bindPrefix.Contains(typeStr, StringComparison.OrdinalIgnoreCase) || typeStr.Contains(bindPrefix, StringComparison.OrdinalIgnoreCase)))
                        {
                            strList[i] = bindPrefix;
                            continue;
                        }
                        //有继承关系的脚本，脚本部分重名，先判断有没有直接能匹配的
                        bool isContinue = false;
                        foreach (var component in m_ComponentCacheList)
                        {
                            bindType = component.GetType();
                            if (CodeBindNameTypeCollection.BindTypeNameDict.TryGetValue(bindType, out bindPrefix) &&
                                (bindPrefix.Contains(typeStr, StringComparison.OrdinalIgnoreCase) || typeStr.Contains(bindPrefix, StringComparison.OrdinalIgnoreCase)))
                            {
                                strList[i] = bindPrefix;
                                isContinue = true;
                                break;
                            }
                        }
                        if (isContinue)
                        {
                            continue;
                        }
                        //有继承关系的脚本，可以找到父类节点绑定
                        foreach (var kv in CodeBindNameTypeCollection.BindNameTypeDict)
                        {
                            if ((kv.Key.Contains(typeStr, StringComparison.OrdinalIgnoreCase) || typeStr.Contains(kv.Key, StringComparison.OrdinalIgnoreCase)) && TryGetBindTarget(child, kv.Value, out _))
                            {
                                strList[i] = kv.Key;
                                break;
                            }
                        }
                    }
                    m_ComponentCacheList.Clear();
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
                transformNameDict.Add(child, string.Join(m_SeparatorChar, strList));
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

        protected bool TryGetBindTarget(Transform transform, Type type, out UnityEngine.Object target)
        {
            if (type == typeof(GameObject))
            {
                target = transform.gameObject;
                return true;
            }
            target = transform.GetComponent(type);
            return target != null;
        }

        private bool CheckIsInOtherBind(Transform child)
        {
            //检查父节点有没有bind，支持bind嵌套
            Transform parent = child.parent;
            bool nearestCodeBind = true;
            while (parent != null)
            {
                MonoBehaviour[] components = parent.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour component in components)
                {
                    if (component.GetType().GetCustomAttributes(typeof(CodeBindAttribute), true).Length > 0)
                    {
                        if (nearestCodeBind && parent == m_RootTransform)
                        {
                            return false;
                        }
                        if (parent != m_RootTransform)
                        {
                            return true;
                        }
                        nearestCodeBind = false;
                    }
                }
                parent = parent.parent;
            }
            return false;
        }

        public void TrySetSerialization()
        {
            CodeBindNameTypeCollection.Do();
            AutoFixChildBindName();
            if (!TryGenerateNameMapTypeData())
            {
                return;
            }
            SetSerialization();
            EditorUtility.SetDirty(m_RootTransform);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private void DoCheck()
        {
            foreach (var bindPrefix in CodeBindNameTypeCollection.BindNameTypeDict.Keys)
            {
                if (bindPrefix.Contains(m_SeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"绑定名[{bindPrefix}]中不能含有分隔符[{m_SeparatorChar}]。");
                }
            }
        }

        protected abstract void SetSerialization();
    }
}