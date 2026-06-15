using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// 缺省生成代码行为，会被 ICodeBindCustomizer 的实现覆盖
    /// </summary>
    internal sealed class DefaultCodeBindCustomizer : ICodeBindCustomizer
    {
        public int Priority => 0;

        public string FieldPrefix => "m_";

        public string ArraySuffix => "Array";

        public string GetPropertyName(string bindName, string bindPrefix)
        {
            return $"{bindName}{bindPrefix}";
        }

        public string GenerateExtraCode(string nameSpace, string className, IReadOnlyList<CodeBindMemberInfo> members, IReadOnlyList<CodeBindArrayMemberInfo> arrayMembers, string indentation)
        {
            return string.Empty;
        }
    }
}
