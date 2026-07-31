using System.Diagnostics;

namespace CodeBind
{
    [Conditional("UNITY_EDITOR")]
    public sealed class MonoBehaviourBindingAttribute : BindingRootAttribute
    {
        public readonly char NameSeparator;

        public MonoBehaviourBindingAttribute(char nameSeparator)
        {
            this.NameSeparator = nameSeparator;
        }

        public MonoBehaviourBindingAttribute()
        {
            NameSeparator = '_';
        }
    }
}
