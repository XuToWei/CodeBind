using System.Diagnostics;

namespace CodeBind
{
    [Conditional("UNITY_EDITOR")]
    public sealed class MonoCodeBindAttribute : CodeBindAttribute
    {
        public readonly char SeparatorChar;

        public MonoCodeBindAttribute(char separatorChar)
        {
            this.SeparatorChar = separatorChar;
        }
        
        public MonoCodeBindAttribute()
        {
            this.SeparatorChar = '_';
        }
    }
}
