using System;
using System.Diagnostics;

namespace CodeBind
{
    /// <summary>
    /// Marks a binding root and creates a boundary for nested binding discovery.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class BindingRootAttribute : Attribute
    {
    }
}
