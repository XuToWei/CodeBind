using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// 缺省生成代码行为，会被 ICodeBindCustomizer 的实现覆盖
    /// </summary>
    internal sealed class DefaultCodeBindCustomizer : ICodeBindCustomizer
    {
        public int Priority => 0;

        public string GetFieldName(string bindName, string bindPrefix)
        {
            return $"m_{bindName}{bindPrefix}";
        }

        public string GetPropertyName(string bindName, string bindPrefix)
        {
            return $"{bindName}{bindPrefix}";
        }

        public string GetArrayFieldName(string bindName, string bindPrefix)
        {
            return $"m_{bindName}{bindPrefix}Array";
        }

        public string GetArrayPropertyName(string bindName, string bindPrefix)
        {
            return $"{bindName}{bindPrefix}Array";
        }

        public string GenerateExtraCode(string nameSpace, string className, IReadOnlyList<CodeBindMemberInfo> members, IReadOnlyList<CodeBindArrayMemberInfo> arrayMembers, string indentation)
        {
            return string.Empty;
        }
    }
}
