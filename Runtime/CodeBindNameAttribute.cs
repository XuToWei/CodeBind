using System;
using System.Diagnostics;

namespace CodeBind
{
    /// <summary>
    /// 用于添加绑定类型的名字，方便自定义类型使用
    /// 直接加在需要绑定的类上即可
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CodeBindNameAttribute : Attribute
    {
        public string BindName
        {
            get;
        }

        /// <summary>
        /// 绑定特性的构造函数
        /// </summary>
        /// <param name="bindName">绑定的节点识别名</param>
        public CodeBindNameAttribute(string bindName)
        {
            BindName = bindName;
        }
    }
}