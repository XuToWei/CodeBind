using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBind
{
    /// <summary>
    /// Hosts manual and generated references addressable by name.
    /// </summary>
    [BindingRoot]
    [BindingTargetToken("NamedReferenceBindingHost")]
    [DisallowMultipleComponent]
    public sealed class NamedReferenceBindingHost : MonoBehaviour
    {
        [SerializeField] private string[] m_ManualKeys;
        [SerializeField] private GameObject[] m_ManualGameObjects;
        [SerializeField] private string[] m_GeneratedReferenceKeys;
        [SerializeField] private UnityEngine.Object[] m_GeneratedReferences;

        public GameObject[] ManualGameObjects => m_ManualGameObjects;
        public UnityEngine.Object[] GeneratedReferences => m_GeneratedReferences;

#if UNITY_EDITOR
        [SerializeField]
        private char m_NameSeparator;

        public char NameSeparator
        {
            get => m_NameSeparator;
            set => m_NameSeparator = value;
        }

        public void SetGeneratedReferences(string[] keys, UnityEngine.Object[] references)
        {
            if (keys == null && references != null)
            {
                throw new ArgumentException("Names cannot be null when components are provided!");
            }
            if (keys != null && references == null)
            {
                throw new ArgumentException("Components cannot be null when names are provided!");
            }
            if (keys != null && references != null && keys.Length != references.Length)
            {
                throw new ArgumentException("Name count must be same with component count!");
            }
            m_GeneratedReferenceKeys = keys;
            m_GeneratedReferences = references;
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
            if (m_GeneratedReferences != null)
            {
                for (int i = 0; i < m_GeneratedReferences.Length; i++)
                {
                    if (m_GeneratedReferences[i] == null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
#endif

        private readonly Dictionary<string, GameObject> m_ManualGameObjectsByKey = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, UnityEngine.Object> m_GeneratedReferenceByKey = new Dictionary<string, UnityEngine.Object>();
        private readonly Dictionary<string, object> m_GeneratedReferenceListsByKey = new Dictionary<string, object>();
        private readonly HashSet<string> m_RepeatedGeneratedReferenceKeys = new HashSet<string>();

        private void Awake()
        {
            for (int i = 0; i < m_ManualKeys.Length; i++)
            {
                m_ManualGameObjectsByKey.Add(m_ManualKeys[i], m_ManualGameObjects[i]);
            }
            for (int i = 0; i < m_GeneratedReferenceKeys.Length; i++)
            {
                string key = m_GeneratedReferenceKeys[i];
                if (!m_GeneratedReferenceByKey.TryAdd(key, m_GeneratedReferences[i]))
                {
                    m_RepeatedGeneratedReferenceKeys.Add(key);
                }
            }
            foreach (string key in m_RepeatedGeneratedReferenceKeys)
            {
                m_GeneratedReferenceByKey.Remove(key);
            }
        }

        public GameObject GetManualGameObject(string key)
        {
            m_ManualGameObjectsByKey.TryGetValue(key, out GameObject gameObject);
            return gameObject;
        }

        public T GetGeneratedReference<T>(string key) where T : UnityEngine.Object
        {
            if (m_GeneratedReferenceByKey.TryGetValue(key, out UnityEngine.Object generatedReference))
            {
                return generatedReference as T;
            }
            return null;
        }

        public List<T> GetGeneratedReferences<T>(string key) where T : UnityEngine.Object
        {
            if (m_GeneratedReferenceListsByKey.TryGetValue(key, out object generatedReferenceList))
            {
                return generatedReferenceList as List<T>;
            }
            if (m_RepeatedGeneratedReferenceKeys.Contains(key))
            {
                List<T> generatedReferences = null;
                for (int i = 0; i < m_GeneratedReferenceKeys.Length; i++)
                {
                    if (m_GeneratedReferenceKeys[i] == key && m_GeneratedReferences[i] is T generatedReference)
                    {
                        if (generatedReferences == null)
                        {
                            generatedReferences = new List<T>();
                        }
                        generatedReferences.Add(generatedReference);
                    }
                }
                if (generatedReferences != null)
                {
                    m_GeneratedReferenceListsByKey.Add(key, generatedReferences);
                    return generatedReferences;
                }
            }
            return null;
        }
    }
}
