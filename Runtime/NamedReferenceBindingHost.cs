using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBind
{
    /// <summary>
    /// Hosts manual and automatically collected references addressable by name.
    /// </summary>
    [BindingRoot]
    [BindingTargetToken("NamedReferenceBindingHost")]
    [DisallowMultipleComponent]
    public sealed class NamedReferenceBindingHost : MonoBehaviour
    {
        [SerializeField] private string[] m_ManualKeys;
        [SerializeField] private GameObject[] m_ManualGameObjects;
        [SerializeField] private string[] m_AutoKeys;
        [SerializeField] private UnityEngine.Object[] m_AutoTargets;

        public GameObject[] ManualGameObjects => m_ManualGameObjects;
        public UnityEngine.Object[] AutoTargets => m_AutoTargets;

#if UNITY_EDITOR
        [SerializeField]
        private char m_NameSeparator;

        public char NameSeparator
        {
            get => m_NameSeparator;
            set => m_NameSeparator = value;
        }

        public void SetAutoTargets(string[] keys, UnityEngine.Object[] targets)
        {
            if (keys == null && targets != null)
            {
                throw new ArgumentException("Names cannot be null when components are provided!");
            }
            if (keys != null && targets == null)
            {
                throw new ArgumentException("Components cannot be null when names are provided!");
            }
            if (keys != null && targets != null && keys.Length != targets.Length)
            {
                throw new ArgumentException("Name count must be same with component count!");
            }
            m_AutoKeys = keys;
            m_AutoTargets = targets;
        }

        public bool HasMissingReferences()
        {
            if (m_ManualGameObjects != null)
            {
                for (int i = 0; i < m_ManualGameObjects.Length; i++)
                {
                    if (m_ManualGameObjects[i] == null)
                    {
                        return true;
                    }
                }
            }
            if (m_AutoTargets != null)
            {
                for (int i = 0; i < m_AutoTargets.Length; i++)
                {
                    if (m_AutoTargets[i] == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
#endif

        private readonly Dictionary<string, GameObject> m_ManualGameObjectsByKey = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, UnityEngine.Object> m_AutoTargetByKey = new Dictionary<string, UnityEngine.Object>();
        private readonly Dictionary<string, object> m_AutoTargetListsByKey = new Dictionary<string, object>();
        private readonly HashSet<string> m_RepeatedAutoKeys = new HashSet<string>();

        private void Awake()
        {
            for (int i = 0; i < m_ManualKeys.Length; i++)
            {
                m_ManualGameObjectsByKey.Add(m_ManualKeys[i], m_ManualGameObjects[i]);
            }
            for (int i = 0; i < m_AutoKeys.Length; i++)
            {
                var key = m_AutoKeys[i];
                if (!m_AutoTargetByKey.TryAdd(key, m_AutoTargets[i]))
                {
                    m_RepeatedAutoKeys.Add(key);
                }
            }
            foreach (var key in m_RepeatedAutoKeys)
            {
                m_AutoTargetByKey.Remove(key);
            }
        }

        public GameObject GetManualGameObject(string key)
        {
            m_ManualGameObjectsByKey.TryGetValue(key, out GameObject gameObject);
            return gameObject;
        }

        public T GetAutoTarget<T>(string key) where T : UnityEngine.Object
        {
            if (m_AutoTargetByKey.TryGetValue(key, out UnityEngine.Object target))
            {
                return target as T;
            }
            return null;
        }

        public List<T> GetAutoTargets<T>(string key) where T : UnityEngine.Object
        {
            if (m_AutoTargetListsByKey.TryGetValue(key, out object targetList))
            {
                return targetList as List<T>;
            }
            if (m_RepeatedAutoKeys.Contains(key))
            {
                List<T> targets = null;
                for (int i = 0; i < m_AutoKeys.Length; i++)
                {
                    if (m_AutoKeys[i] == key && m_AutoTargets[i] is T target)
                    {
                        if (targets == null)
                        {
                            targets = new List<T>();
                        }
                        targets.Add(target);
                    }
                }
                if (targets != null)
                {
                    m_AutoTargetListsByKey.Add(key, targets);
                    return targets;
                }
            }
            return null;
        }
    }
}
