using System;
using UnityEngine;

namespace CodeBind
{
    /// <summary>
    /// 非Mono类绑定数据的Mono
    /// </summary>
    [CodeBind]
    [CodeBindName("CSCodeBindMono")]
    [DisallowMultipleComponent]
    public sealed class CSCodeBindMono : MonoBehaviour
    {
        private static readonly CSCodeBindPool s_Pool = new CSCodeBindPool();

        [SerializeField]
        private UnityEngine.Object[] m_BindComponents;

        public UnityEngine.Object[] BindComponents => m_BindComponents;

        private ICSCodeBind m_CSCodeBindObject;

#if UNITY_EDITOR
        [SerializeField]
        private char m_SeparatorChar;

        [SerializeField]
        private UnityEditor.MonoScript m_BindScript;

        [SerializeField]
        private string[] m_BindComponentNames;

        public char SeparatorChar
        {
            get => m_SeparatorChar;
            set => m_SeparatorChar = value;
        }

        public UnityEditor.MonoScript BindScript
        {
            get => m_BindScript;
            set => m_BindScript = value;
        }

        public string[] BindComponentNames => m_BindComponentNames;

        public void SetBindComponents(string[] names, UnityEngine.Object[] components)
        {
            if (names == null && components != null)
            {
                throw new ArgumentException("Names cannot be null when components are provided!");
            }
            if (names != null && components == null)
            {
                throw new ArgumentException("Components cannot be null when names are provided!");
            }
            if (names != null && components != null && names.Length != components.Length)
            {
                throw new ArgumentException("Name count must be same with component count!");
            }
            m_BindComponentNames = names;
            m_BindComponents = components;
        }

        public bool CheckBindDataExitEmpty()
        {
            if (m_BindComponents != null)
            {
                for (int i = 0; i < m_BindComponents.Length; i++)
                {
                    if (m_BindComponents[i] == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
#endif

        /// <summary>
        /// 获取绑定代码的的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetCSCodeBindObject<T>() where T : class, ICSCodeBind, new()
        {
#if UNITY_EDITOR
            Type bindType = m_BindScript.GetClass();
            Type getType = typeof(T);
            if (bindType != getType)
            {
                Debug.LogWarning($"{gameObject.name} bind type is {bindType}, but get is {getType}.");
            }
#endif
            if (m_CSCodeBindObject == null)
            {
                m_CSCodeBindObject = s_Pool.Fetch<T>(this);
            }
            else
            {
                if (m_CSCodeBindObject is not T)
                {
                    Debug.LogWarning($"Get different object(type:{typeof(T)}, the old object(type:{m_CSCodeBindObject.GetType()} will recycle!)");
                    s_Pool.Recycle(m_CSCodeBindObject);
                    m_CSCodeBindObject = s_Pool.Fetch<T>(this);
                }
            }
            return (T)m_CSCodeBindObject;
        }

        private void OnDestroy()
        {
            if (m_CSCodeBindObject != null)
            {
                var obj = m_CSCodeBindObject;
                m_CSCodeBindObject = null;
                s_Pool.Recycle(obj);
            }
        }
    }
}