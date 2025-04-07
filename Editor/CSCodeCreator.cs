using System;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// 非Mono类的绑定代码生成器
    /// </summary>
    internal sealed class CSCodeCreator : BaseCodeCreator
    {
        public CSCodeCreator(string codePath, string codeName, string codeNamespace, Transform rootTransform, char separatorChar) : base(codePath, codeName, codeNamespace, rootTransform, separatorChar)
        {
        }

        protected override void SetSerialization()
        {
            throw new Exception("CSCodeCreator does not support serialization!");
        }

        protected override string GetBindCode()
        {
            return CodeHelper.GetCSBindCodeString(m_ScriptNameSpace, m_ScriptClassName, m_BindDatas, m_BindArrayDataDict, m_BindArrayDatas);
        }

        protected override string GetClassCode()
        {
            if (!string.IsNullOrEmpty(m_ScriptNameSpace))
            {
                return $@"namespace {m_ScriptNameSpace}
{{
    public partial class {m_ScriptClassName}
    {{

    }}
}}";
            }
            else
            {
                return $@"public partial class {m_ScriptClassName} 
{{

}}";
            }
        }
    }
}