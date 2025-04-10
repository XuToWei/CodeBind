using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal sealed class MonoCodeBinder : BaseCodeBinder
    {
        private readonly MonoBehaviour m_MonoObj;
        
        public MonoCodeBinder(MonoScript script, Transform rootTransform, char separatorChar): base(script, rootTransform, separatorChar)
        {
            m_MonoObj = rootTransform.GetComponent(script.GetClass()) as MonoBehaviour;
            if (m_MonoObj == null)
            {
                throw new Exception("MonoCodeBinder only can be used of MonoBehaviour!");
            }
        }
        
        protected override string GetGeneratedCode()
        {
            return CodeHelper.GetMonoBindCodeString(m_ScriptNameSpace, m_ScriptClassName, m_BindDatas, m_BindArrayDataDict);
        }

        protected override void SetSerialization()
        {
            Type monoType = m_MonoObj.GetType();
            foreach (CodeBindData bindData in m_BindDatas)
            {
                FieldInfo fieldInfo = monoType.GetField($"m_{bindData.BindName}{bindData.BindPrefix}", BindingFlags.NonPublic | BindingFlags.Instance);
                if(!TryGetBindTarget(bindData.BindTransform, bindData.BindType, out var target))
                {
                    throw new Exception($"Bind '{bindData.BindTransform} - {bindData.BindType}' fail!");
                }
                fieldInfo.SetValue(m_MonoObj, target);
            }
            
            foreach (KeyValuePair<string, List<CodeBindData>> kv in m_BindArrayDataDict)
            {
                List<object> components = new List<object>();
                foreach (CodeBindData bindData in kv.Value)
                {
                    if(!TryGetBindTarget(bindData.BindTransform, bindData.BindType, out var target))
                    {
                        throw new Exception($"Bind '{bindData.BindTransform} - {bindData.BindType}' fail!");
                    }
                    components.Add(target);
                }
                FieldInfo fieldInfo = monoType.GetField($"m_{kv.Key}Array", BindingFlags.NonPublic | BindingFlags.Instance);
                Type type = fieldInfo.FieldType.GetElementType();
                Array filledArray = Array.CreateInstance(type, kv.Value.Count);
                Array.Copy(components.ToArray(), filledArray, kv.Value.Count);
                fieldInfo.SetValue(m_MonoObj, filledArray);
            }
        }
    }
}
