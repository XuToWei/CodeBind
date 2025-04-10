using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBind
{
    /// <summary>
    /// 引用绑定的Mono
    /// </summary>
    [CodeBind]
    [CodeBindName("ReferenceBindMono")]
    [DisallowMultipleComponent]
    public sealed class ReferenceBindMono : MonoBehaviour
    {
        [SerializeField] private string[] m_BindNames;
        [SerializeField] private GameObject[] m_BindGameObjects;
        [SerializeField] private string[] m_AutoBindComponentNames;
        [SerializeField] private UnityEngine.Object[] m_AutoBindComponents;

        public GameObject[] BindGameObjects => m_BindGameObjects;
        public UnityEngine.Object[] AutoBindComponents => m_AutoBindComponents;

#if UNITY_EDITOR
        [SerializeField]
        private char m_SeparatorChar;

        public char SeparatorChar => m_SeparatorChar;

        public void SetAutoBindComponents(string[] names, UnityEngine.Object[] components)
        {
            if (names == null || components == null)
            {
                throw new Exception("Name and Component cant be null!");
            }
            if (names.Length != components.Length)
            {
                throw new Exception("Name count must be same with Component count!");
            }
            m_AutoBindComponentNames = names;
            m_AutoBindComponents = components;
        }
#endif

        private readonly Dictionary<string, GameObject> m_GameObjectDict = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, UnityEngine.Object> m_AutoBindDict = new Dictionary<string, UnityEngine.Object>();
        private readonly Dictionary<string, object> m_AutoBindArrayDict = new Dictionary<string, object>();
        private readonly HashSet<string> m_AutoBindArrayNames = new HashSet<string>();

        private void Awake()
        {
            for (int i = 0; i < m_BindNames.Length; i++)
            {
                m_GameObjectDict.Add(m_BindNames[i], m_BindGameObjects[i]);
            }
            for (int i = 0; i < m_AutoBindComponentNames.Length; i++)
            {
                var componentName = m_AutoBindComponentNames[i];
                if (!m_AutoBindDict.TryAdd(componentName, m_AutoBindComponents[i]))
                {
                    m_AutoBindArrayNames.Add(componentName);
                }
            }
            foreach (var componentName in m_AutoBindArrayNames)
            {
                m_AutoBindDict.Remove(componentName);
            }
        }

        public GameObject GetGameObject(string key)
        {
            m_GameObjectDict.TryGetValue(key, out GameObject go);
            return go;
        }
        
        public T GetAs<T>(string key) where T : UnityEngine.Object
        {
            if (m_AutoBindDict.TryGetValue(key, out UnityEngine.Object obj))
            {
                return obj as T;
            }
            return null;
        }
        
        public List<T> GetList<T>(string key) where T : UnityEngine.Object
        {
            if (m_AutoBindArrayDict.TryGetValue(key, out object listObj))
            {
                return listObj as List<T>;
            }
            if (m_AutoBindArrayNames.Contains(key))
            {
                List<T> list = null;
                for (int i = 0; i < m_AutoBindComponentNames.Length; i++)
                {
                    if (m_AutoBindComponentNames[i] == key && m_AutoBindComponents[i] is T component)
                    {
                        if (list == null)
                        {
                            list = new List<T>();
                        }
                        list.Add(component);
                    }
                }
                if (list != null)
                {
                    m_AutoBindArrayDict.Add(key, list);
                    return list;
                }
            }
            return null;
        }
    }
}