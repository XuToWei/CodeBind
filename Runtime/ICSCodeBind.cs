using UnityEngine;

namespace CodeBind
{
    public interface ICSCodeBind
    {
        CSCodeBindMono Mono { get; }
        Transform Transform { get; }
        void InitBind(CSCodeBindMono csCodeBindMono);
        void ClearBind();
    }
}
