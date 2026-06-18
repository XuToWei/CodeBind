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

        // 命名风格：字段用 "_" 前缀，属性沿用组合名（大写开头）
        // name 为框架拼好的组合名（数组已带 "Array" 后缀），只需决定前后缀即可
        public string GetFieldName(string name)
        {
            return $"_{name}";
        }

        public string GetPropertyName(string name)
        {
            return name;
        }

        // 额外代码：为每个绑定成员生成一行说明注释
        public string GenerateExtraCode(string nameSpace, string className,
            List<CodeBindData> bindDatas,
            SortedDictionary<string, List<CodeBindData>> bindArrayDataDict,
            string indentation)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (CodeBindData bindData in bindDatas)
            {
                stringBuilder.AppendLine($"{indentation}// member: {GetPropertyName($"{bindData.BindName}{bindData.BindPrefix}")} ({bindData.BindType.Name})");
            }
            foreach (KeyValuePair<string, List<CodeBindData>> kv in bindArrayDataDict)
            {
                CodeBindData firstBindData = kv.Value[0];
                //数组属性名为组合名加固定的 "Array" 后缀
                stringBuilder.AppendLine($"{indentation}// array member: {GetPropertyName($"{firstBindData.BindName}{firstBindData.BindPrefix}Array")} ({firstBindData.BindType.Name}[{kv.Value.Count}])");
            }
            return stringBuilder.ToString();
        }
    }
}
