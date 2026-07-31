using System;
using UnityEngine;

namespace CodeBind
{
    /// <summary>
    /// Hosts the serialized targets and runtime instance for a plain class binding.
    /// </summary>
    [BindingRoot]
    [BindingTargetToken("PlainClassBindingHost")]
    [DisallowMultipleComponent]
    public sealed class PlainClassBindingHost : MonoBehaviour
    {
        private static readonly PlainClassBindingPool s_BindingPool = new PlainClassBindingPool();

        [SerializeField]
        private UnityEngine.Object[] m_BindingTargets;

        public UnityEngine.Object[] BindingTargets => m_BindingTargets;

        private IPlainClassBinding m_Binding;

#if UNITY_EDITOR
        [SerializeField]
        private char m_NameSeparator;

        [SerializeField]
        private UnityEditor.MonoScript m_BindingClassScript;

        [SerializeField]
        private string[] m_BindingMemberNames;

        public char NameSeparator
        {
            get => m_NameSeparator;
            set => m_NameSeparator = value;
        }

        public UnityEditor.MonoScript BindingClassScript
        {
            get => m_BindingClassScript;
            set => m_BindingClassScript = value;
        }

        public string[] BindingMemberNames => m_BindingMemberNames;

        public void SetBindingTargets(string[] memberNames, UnityEngine.Object[] targets)
        {
            if (memberNames == null && targets != null)
            {
                throw new ArgumentException("Names cannot be null when components are provided!");
            }
            if (memberNames != null && targets == null)
            {
                throw new ArgumentException("Components cannot be null when names are provided!");
            }
            if (memberNames != null && targets != null && memberNames.Length != targets.Length)
            {
                throw new ArgumentException("Name count must be same with component count!");
            }
            m_BindingMemberNames = memberNames;
            m_BindingTargets = targets;
        }

        public bool HasMissingTargets()
        {
            if (m_BindingTargets != null)
            {
                for (int i = 0; i < m_BindingTargets.Length; i++)
                {
                    if (m_BindingTargets[i] == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
#endif

        /// <summary>
        /// Gets the cached plain class binding instance.
        /// </summary>
        public T GetPlainClassBinding<T>() where T : class, IPlainClassBinding, new()
        {
#if UNITY_EDITOR
            Type bindingType = m_BindingClassScript.GetClass();
            Type requestedType = typeof(T);
            if (bindingType != requestedType)
            {
                Debug.LogWarning($"[CodeBind] {gameObject.name} bind type is {bindingType}, but get is {requestedType}.");
            }
#endif
            if (m_Binding == null)
            {
                m_Binding = s_BindingPool.Acquire<T>(this);
            }
            else
            {
                if (m_Binding is not T)
                {
                    Debug.LogWarning($"[CodeBind] Get different object(type:{typeof(T)}, the old object(type:{m_Binding.GetType()} will recycle!)");
                    s_BindingPool.Release(m_Binding);
                    m_Binding = s_BindingPool.Acquire<T>(this);
                }
            }
            return (T)m_Binding;
        }

        private void OnDestroy()
        {
            if (m_Binding != null)
            {
                var binding = m_Binding;
                m_Binding = null;
                s_BindingPool.Release(binding);
            }
        }
    }
}
