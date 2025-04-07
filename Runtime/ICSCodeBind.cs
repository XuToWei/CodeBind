using UnityEngine;

namespace CodeBind
{
    /// <summary>
    /// 非Mono类绑定数据接口
    /// </summary>
    public interface ICSCodeBind
    {
        CSCodeBindMono Mono { get; }
        Transform Transform { get; }
        void InitBind(CSCodeBindMono csCodeBindMono);
        void ClearBind();
    }
}