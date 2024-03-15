using System;
using System.Diagnostics;

namespace CodeBind
{
    /// <summary>
    /// 用于添加绑定类型的名字，方便自定义类型使用
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CodeBindNameAttribute : Attribute
    {
        public string BindName
        {
            get;
        }

        public CodeBindNameAttribute(string bindName)
        {
            BindName = bindName;
        }
    }
}
