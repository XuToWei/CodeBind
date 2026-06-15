using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBind.Editor
{
    /// <summary>
    /// 生成代码时的单个绑定成员信息
    /// </summary>
    public readonly struct CodeBindMemberInfo
    {
        /// <summary>
        /// 公共属性名，如 SelfTransform
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 绑定类型
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 绑定的节点
        /// </summary>
        public Transform Transform { get; }

        public CodeBindMemberInfo(string name, Type type, Transform transform)
        {
            Name = name;
            Type = type;
            Transform = transform;
        }
    }

    /// <summary>
    /// 生成代码时的数组绑定成员信息
    /// </summary>
    public readonly struct CodeBindArrayMemberInfo
    {
        /// <summary>
        /// 公共属性名，如 ItemTransformArray
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 数组元素类型
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 数组各元素绑定的节点
        /// </summary>
        public IReadOnlyList<Transform> Transforms { get; }

        public CodeBindArrayMemberInfo(string name, Type type, IReadOnlyList<Transform> transforms)
        {
            Name = name;
            Type = type;
            Transforms = transforms;
        }
    }

    /// <summary>
    /// 自定义生成代码接口，包含命名风格和额外代码生成
    /// 实现此接口即可覆盖默认行为，未实现则使用默认行为（字段前缀 m_，数组后缀 Array，无额外代码）
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
        /// 私有序列化字段前缀，默认 "m_"
        /// </summary>
        string FieldPrefix { get; }

        /// <summary>
        /// 数组字段和属性的后缀，默认 "Array"
        /// </summary>
        string ArraySuffix { get; }

        /// <summary>
        /// 公共属性命名，默认 bindName + bindPrefix
        /// </summary>
        /// <param name="bindName">绑定的变量名</param>
        /// <param name="bindPrefix">绑定的类型名</param>
        string GetPropertyName(string bindName, string bindPrefix);

        /// <summary>
        /// 额外生成代码，返回追加到 partial 类体内的代码，无内容返回空字符串
        /// Mono 模式和纯 C# 模式共用同一份输出
        /// </summary>
        /// <param name="nameSpace">生成类的命名空间，可能为空</param>
        /// <param name="className">生成类的类名</param>
        /// <param name="members">单个绑定成员列表</param>
        /// <param name="arrayMembers">数组绑定成员列表</param>
        /// <param name="indentation">类体内的缩进字符串</param>
        string GenerateExtraCode(string nameSpace, string className, IReadOnlyList<CodeBindMemberInfo> members, IReadOnlyList<CodeBindArrayMemberInfo> arrayMembers, string indentation);
    }
}
