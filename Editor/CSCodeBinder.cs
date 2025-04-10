using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal sealed class CSCodeBinder : BaseCodeBinder
    {
        private readonly CSCodeBindMono m_CsCodeBindMono;
        
        public CSCodeBinder(MonoScript script, Transform rootTransform, char separatorChar): base(script, rootTransform, separatorChar)
        {
            m_CsCodeBindMono = rootTransform.GetComponent<CSCodeBindMono>();
            if (m_CsCodeBindMono == null)
            {
                throw new Exception($"PureCSCodeBinder init fail! {rootTransform} has no CSCodeBindMono!");
            }
        }

        protected override string GetGeneratedCode()
        {
            return CodeHelper.GetCSBindCodeString(m_ScriptNameSpace, m_ScriptClassName, m_BindDatas, m_BindArrayDataDict, m_BindArrayDatas);
        }

        protected override void SetSerialization()
        {
            List<string> bindNames = new List<string>();
            List<UnityEngine.Object> bindComponents = new List<UnityEngine.Object>();
            foreach (CodeBindData bindData in m_BindDatas)
            {
                bindNames.Add(bindData.BindName + bindData.BindPrefix);
                if(!TryGetBindTarget(bindData.BindTransform, bindData.BindType, out var target))
                {
                    throw new Exception($"Bind '{bindData.BindTransform} - {bindData.BindType}' fail!");
                }
                bindComponents.Add(target);
            }
            foreach (CodeBindData bindData in m_BindArrayDatas)
            {
                bindNames.Add($"{bindData.BindName}{bindData.BindPrefix}Array");
                if(!TryGetBindTarget(bindData.BindTransform, bindData.BindType, out var target))
                {
                    throw new Exception($"Bind '{bindData.BindTransform} - {bindData.BindType}' fail!");
                }
                bindComponents.Add(target);
            }
            m_CsCodeBindMono.SetBindComponents(bindNames.ToArray(), bindComponents.ToArray());
        }
    }
}
