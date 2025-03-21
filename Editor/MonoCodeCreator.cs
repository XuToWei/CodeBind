using System;
using UnityEngine;

namespace CodeBind.Editor
{
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

namespace {m_ScriptNameSpace}
{{
    public partial class {m_ScriptClassName} : MonoBehaviour 
    {{

    }}
}}";
            }
            else
            {
                return $@"using UnityEngine;

public partial class {m_ScriptClassName} : MonoBehaviour 
{{
    
}}";
            }
        }
    }
}