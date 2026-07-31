using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal sealed class MonoBehaviourBinder : ExistingScriptBindingGenerator
    {
        private readonly MonoBehaviour m_TargetBehaviour;

        public MonoBehaviourBinder(MonoScript script, Transform rootTransform, char nameSeparator): base(script, rootTransform, nameSeparator)
        {
            m_TargetBehaviour = rootTransform.GetComponent(script.GetClass()) as MonoBehaviour;
            if (m_TargetBehaviour == null)
            {
                throw new Exception("MonoBehaviourBinder only can be used of MonoBehaviour!");
            }
        }

        protected override string BuildBindingSource()
        {
            return BindingSourceBuilder.BuildMonoBehaviourBindingSource(m_TargetNamespace, m_TargetClassName, m_SingleBindings, m_ArrayBindingsByMemberName);
        }

        protected override void SerializeBindings()
        {
            if(EditorApplication.isCompiling)
            {
                throw new Exception("Unity正在编译，无法进行绑定数据生成，请稍后再试。");
            }
            if (EditorUtility.scriptCompilationFailed)
            {
                throw new Exception("Unity脚本编译失败，无法进行绑定数据生成，请修复脚本错误后再试。");
            }
            Type targetType = m_TargetBehaviour.GetType();
            foreach (BindingDescriptor binding in m_SingleBindings)
            {
                FieldInfo targetField = targetType.GetField(BindingCodeCustomizerRegistry.GetSerializedFieldName(binding.VariableName, binding.TargetToken), BindingFlags.NonPublic | BindingFlags.Instance);
                if(!TryGetBindingTarget(binding.SourceTransform, binding.TargetType, out var target))
                {
                    throw new Exception($"Bind '{binding.SourceTransform} - {binding.TargetType}' fail!");
                }
                targetField.SetValue(m_TargetBehaviour, target);
            }

            foreach (KeyValuePair<string, List<BindingDescriptor>> kv in m_ArrayBindingsByMemberName)
            {
                List<object> boundTargets = new List<object>();
                foreach (BindingDescriptor binding in kv.Value)
                {
                    if(!TryGetBindingTarget(binding.SourceTransform, binding.TargetType, out var target))
                    {
                        throw new Exception($"Bind '{binding.SourceTransform} - {binding.TargetType}' fail!");
                    }
                    boundTargets.Add(target);
                }
                BindingDescriptor firstArrayBinding = kv.Value[0];
                FieldInfo targetField = targetType.GetField(BindingCodeCustomizerRegistry.GetSerializedArrayFieldName(firstArrayBinding.VariableName, firstArrayBinding.TargetToken), BindingFlags.NonPublic | BindingFlags.Instance);
                Type type = targetField.FieldType.GetElementType();
                Array filledArray = Array.CreateInstance(type, kv.Value.Count);
                Array.Copy(boundTargets.ToArray(), filledArray, kv.Value.Count);
                targetField.SetValue(m_TargetBehaviour, filledArray);
            }
        }
    }
}
