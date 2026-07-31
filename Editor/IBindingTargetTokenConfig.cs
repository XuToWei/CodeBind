using System;
using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// Provides hierarchy binding target token to type mappings.
    /// </summary>
    public interface IBindingTargetTokenConfig
    {
        int Priority { get; }

        IReadOnlyDictionary<string, Type> TargetTypesByToken { get; }
    }
}
