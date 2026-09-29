using System;
using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// Default binding target tokens for GameObject and Transform.
    /// </summary>
    internal sealed class DefaultBindingTargetTokenConfig : IBindingTargetTokenConfig
    {
        public int Priority => 0;

        public IReadOnlyDictionary<string, Type> TargetTypesByToken => s_TargetTypesByToken;

        private static readonly Dictionary<string, Type> s_TargetTypesByToken = new Dictionary<string, Type>()
        {
            { "GameObject", typeof (UnityEngine.GameObject) },
            { "Transform", typeof (UnityEngine.Transform) },
        };
    }
}
