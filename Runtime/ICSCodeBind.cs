using UnityEngine;

namespace CodeBind
{
    /// <summary>
    /// 非Mono类绑定数据接口
    /// </summary>
    public interface ICSCodeBind
    {
        CSCodeBindMono BindMono { get; }
        Transform CachedTransform { get; }
        void InitBind(CSCodeBindMono csCodeBindMono);
        void ClearBind();
    }
}