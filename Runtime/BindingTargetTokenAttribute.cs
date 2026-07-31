using System;
using System.Diagnostics;

namespace CodeBind
{
    /// <summary>
    /// Defines the hierarchy token used to identify a binding target type.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BindingTargetTokenAttribute : Attribute
    {
        public string Token
        {
            get;
        }

        /// <summary>
        /// Creates a hierarchy binding target token.
        /// </summary>
        /// <param name="token">The token used in hierarchy node names.</param>
        public BindingTargetTokenAttribute(string token)
        {
            this.Token = token;
        }
    }
}
