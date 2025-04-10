using System;
using System.Diagnostics;

namespace CodeBind
{
    /// <summary>
    /// 用于添加绑定类型的名字组，方便引擎类型使用
    /// 必须给static的Dictionary<string, Type>使用
    /// string是绑定的节点识别名，Type是绑定的脚本类型
    /// 优先级高于CodeBindNameAttribute
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CodeBindNameTypeAttribute : Attribute
    {
    }
}