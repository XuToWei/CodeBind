using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// 自定义生成代码接口，包含命名风格和额外代码生成
    /// 实现此接口即可覆盖默认行为，未实现则使用默认行为（字段 m_ 前缀、属性首字母小写、无额外代码）
    /// 命名方法接收已拼好的组合名（变量名 + 类型名），数组会由框架自动追加 "Array" 后缀后再传入，
    /// 实现只需决定字段/属性的前后缀风格，无需自行拼接
    /// 实现类需要有无参构造函数
    /// </summary>
    public interface ICodeBindCustomizer
    {
        /// <summary>
        /// 优先级，数值越大优先级越高，最高优先级的实现会被使用
        /// 默认实现的优先级为 0
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 私有序列化字段命名，默认 "m_" + name
        /// </summary>
        /// <param name="name">已拼好的组合名（变量名 + 类型名，数组已带 "Array" 后缀）</param>
        string GetFieldName(string name);

        /// <summary>
        /// 公共属性命名，默认 name 首字母小写
        /// </summary>
        /// <param name="name">已拼好的组合名（变量名 + 类型名，数组已带 "Array" 后缀）</param>
        string GetPropertyName(string name);

        /// <summary>
        /// 额外生成代码，返回追加到 partial 类体内的代码，无内容返回空字符串
        /// Mono 模式和纯 C# 模式共用同一份输出
        /// </summary>
        /// <param name="nameSpace">生成类的命名空间，可能为空</param>
        /// <param name="className">生成类的类名</param>
        /// <param name="bindDatas">单个绑定数据列表</param>
        /// <param name="bindArrayDataDict">数组绑定数据字典，键为数组名，值为该数组各元素的绑定数据</param>
        /// <param name="indentation">类体内的缩进字符串</param>
        string GenerateExtraCode(string nameSpace, string className, List<CodeBindData> bindDatas, SortedDictionary<string, List<CodeBindData>> bindArrayDataDict, string indentation);
    }
}
