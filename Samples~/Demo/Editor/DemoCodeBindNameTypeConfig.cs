using System;
using System.Collections.Generic;
using CodeBind.Editor;

namespace CodeBind.Demo.Editor
{
    /// <summary>
    /// ICodeBindNameTypeConfig 示例：批量注册 节点识别名 -> 绑定类型 的映射
    /// 所有实现（含内置缺省配置 Priority=0）会一起合并，识别名或类型冲突时优先级高的覆盖低的
    /// </summary>
    public sealed class DemoCodeBindNameTypeConfig : ICodeBindNameTypeConfig
    {
        // 大于缺省配置(0)即可覆盖缺省映射；这里只新增 demo 专用映射，与缺省互不冲突
        public int Priority => 1;

        public IReadOnlyDictionary<string, Type> BindNameTypeDict { get; } = new Dictionary<string, Type>
        {
            { "DemoCustom", typeof(DemoCustomComponent) },
            { "DC", typeof(DemoCustomComponent) },
        };
    }
}
