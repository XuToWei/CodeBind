using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// 缺省生成代码行为，会被 ICodeBindCustomizer 的实现覆盖
    /// </summary>
    internal sealed class DefaultCodeBindCustomizer : ICodeBindCustomizer
    {
        public int Priority => 0;

        public string GetFieldName(string name)
        {
            return $"m_{name}";
        }

        public string GetPropertyName(string name)
        {
            return name;
        }

        public string GenerateExtraCode(string nameSpace, string className, List<CodeBindData> bindDatas, SortedDictionary<string, List<CodeBindData>> bindArrayDataDict, string indentation)
        {
            return string.Empty;
        }
    }
}
