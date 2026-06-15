using System;
using System.Collections.Generic;

namespace CodeBind.Editor
{
    /// <summary>
    /// 绑定类型名称配置接口，提供 节点识别名 -> 绑定类型 的映射
    /// string 是绑定的节点识别名，Type 是绑定的脚本类型（需为 Component 子类或 GameObject）
    /// 可以有多个实现，识别名或类型冲突时优先级高的覆盖优先级低的
    /// 实现类需要有无参构造函数
    /// </summary>
    public interface ICodeBindNameTypeConfig
    {
        /// <summary>
        /// 优先级，数值越大优先级越高，冲突时高优先级覆盖低优先级
        /// 默认配置的优先级为 0
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 节点识别名 -> 绑定类型 的映射
        /// </summary>
        IReadOnlyDictionary<string, Type> BindNameTypeDict { get; }
    }
}
