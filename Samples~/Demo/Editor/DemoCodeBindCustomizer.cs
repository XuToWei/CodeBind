using System.Collections.Generic;
using System.Text;
using CodeBind.Editor;

namespace CodeBind.Demo.Editor
{
    /// <summary>
    /// ICodeBindCustomizer 示例：自定义命名风格 + 追加额外代码
    /// 注意：Customizer 会影响整个工程的代码生成，Priority 大于缺省的 0 即生效，修改后需重新生成绑定代码。
    /// </summary>
    public sealed class DemoCodeBindCustomizer : ICodeBindCustomizer
    {
        public int Priority => 1;

        // 命名风格：字段用 "_" 前缀，数组后缀用 "List"，属性名为 变量名 + 类型名
        public string GetFieldName(string bindName, string bindPrefix)
        {
            return $"_{bindName}{bindPrefix}";
        }

        public string GetPropertyName(string bindName, string bindPrefix)
        {
            return $"{bindName}{bindPrefix}";
        }

        public string GetArrayFieldName(string bindName, string bindPrefix)
        {
            return $"_{bindName}{bindPrefix}List";
        }

        public string GetArrayPropertyName(string bindName, string bindPrefix)
        {
            return $"{bindName}{bindPrefix}List";
        }

        // 额外代码：为每个绑定成员生成一行说明注释
        public string GenerateExtraCode(string nameSpace, string className,
            IReadOnlyList<CodeBindMemberInfo> members,
            IReadOnlyList<CodeBindArrayMemberInfo> arrayMembers,
            string indentation)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (CodeBindMemberInfo member in members)
            {
                stringBuilder.AppendLine($"{indentation}// member: {member.Name} ({member.Type.Name})");
            }
            foreach (CodeBindArrayMemberInfo arrayMember in arrayMembers)
            {
                stringBuilder.AppendLine($"{indentation}// array member: {arrayMember.Name} ({arrayMember.Type.Name}[{arrayMember.Transforms.Count}])");
            }
            return stringBuilder.ToString();
        }
    }
}
