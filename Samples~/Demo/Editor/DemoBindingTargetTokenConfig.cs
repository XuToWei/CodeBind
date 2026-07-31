using System;
using System.Collections.Generic;
using CodeBind.Editor;

namespace CodeBind.Demo.Editor
{
    /// <summary>
    /// IBindingTargetTokenConfig example that registers hierarchy token mappings.
    /// </summary>
    public sealed class DemoBindingTargetTokenConfig : IBindingTargetTokenConfig
    {
        public int Priority => 1;

        public IReadOnlyDictionary<string, Type> TargetTypesByToken { get; } = new Dictionary<string, Type>
        {
            { "DemoCustom", typeof(DemoCustomComponent) },
            { "DC", typeof(DemoCustomComponent) },
        };
    }
}
