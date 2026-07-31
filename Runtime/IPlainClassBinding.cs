using UnityEngine;

namespace CodeBind
{
    /// <summary>
    /// Defines the binding contract for a plain C# class hosted by a MonoBehaviour.
    /// </summary>
    public interface IPlainClassBinding
    {
        PlainClassBindingHost BindingHost { get; }
        Transform RootTransform { get; }
        void Initialize(PlainClassBindingHost host);
        void Reset();
    }
}
