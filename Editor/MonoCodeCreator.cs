using System;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// Mono类的绑定代码的生成器
    /// </summary>
    internal sealed class MonoCodeCreator : BaseCodeCreator
    {
        public MonoCodeCreator(string codePath, string codeName, string codeNamespace, Transform rootTransform, char separatorChar) : base(codePath, codeName, codeNamespace, rootTransform, separatorChar)
        {
            
        }

        protected override void SetSerialization()
        {
            throw new Exception("MonoCodeCreator does not support serialization!");
        }

        protected override string GetBindCode()
        {
            return CodeHelper.GetMonoBindCodeString(m_ScriptNameSpace, m_ScriptClassName, m_BindDatas, m_BindArrayDataDict);
        }

        protected override string GetClassCode()
        {
            if (!string.IsNullOrEmpty(m_ScriptNameSpace))
            {
                return $@"using UnityEngine;
using CodeBind;

namespace {m_ScriptNameSpace}
{{
    [MonoCodeBind('{m_SeparatorChar}')]
    public partial class {m_ScriptClassName} : MonoBehaviour 
    {{

    }}
}}";
            }
            else
            {
                return $@"using UnityEngine;
using CodeBind;

[MonoCodeBind('{m_SeparatorChar}')]
public partial class {m_ScriptClassName} : MonoBehaviour 
{{

}}";
            }
        }
    }
}