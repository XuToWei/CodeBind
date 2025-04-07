using System;
using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// 引用绑定器
    /// </summary>
    internal sealed class ReferenceBinder : BaseBinder
    {
        private readonly ReferenceBindMono m_ReferenceBindMono;
        
        public ReferenceBinder(ReferenceBindMono referenceBindMono, char separatorChar) : base(referenceBindMono.transform, separatorChar)
        {
            m_ReferenceBindMono = referenceBindMono;
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
            m_ReferenceBindMono.SetAutoBindComponents(bindNames.ToArray(), bindComponents.ToArray());
        }
    }
}