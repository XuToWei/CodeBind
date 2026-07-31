using System;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal class BindingRefreshCommands
    {
        [MenuItem("GameObject/CodeBind/Refresh All Binding Sources", priority = -2)]
        private static void RefreshAllBindingSources()
        {
            if (Selection.gameObjects.Length < 1)
            {
                Debug.LogError("[CodeBind] Please select at least one GameObject to refresh binding sources.");
                return;
            }
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                MonoBehaviour[] behaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    object[] attributes = behaviour.GetType().GetCustomAttributes(typeof(MonoBehaviourBindingAttribute), false);
                    if (attributes.Length > 0)
                    {
                        MonoBehaviourBindingAttribute attribute = attributes[0] as MonoBehaviourBindingAttribute;
                        if (attribute == null)
                        {
                            throw new Exception($"{behaviour.GetType()} is not inherit from MonoBehaviourBindingAttribute!");
                        }
                        MonoScript script = MonoScript.FromMonoBehaviour(behaviour);
                        MonoBehaviourBinder binder = new MonoBehaviourBinder(script, behaviour.transform, attribute.NameSeparator);
                        binder.TryGenerateBindingSource();
                        Debug.Log($"[CodeBind] Refresh '{behaviour.name}({behaviour})' binding source successfully.");
                    }
                }
                PlainClassBindingHost[] hosts = gameObject.GetComponentsInChildren<PlainClassBindingHost>(true);
                foreach (PlainClassBindingHost host in hosts)
                {
                    PlainClassBinder binder = new PlainClassBinder(host.BindingClassScript, host.transform, host.NameSeparator);
                    binder.TryGenerateBindingSource();
                }
            }
        }

        [MenuItem("GameObject/CodeBind/Refresh All Serialized Bindings", priority = -1)]
        private static void RefreshAllSerializedBindings()
        {
            if (Selection.gameObjects.Length < 1)
            {
                Debug.LogError("[CodeBind] Please select at least one GameObject to refresh serialized bindings.");
            }
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                MonoBehaviour[] behaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    object[] attributes = behaviour.GetType().GetCustomAttributes(typeof(MonoBehaviourBindingAttribute), false);
                    if (attributes.Length > 0)
                    {
                        MonoBehaviourBindingAttribute attribute = attributes[0] as MonoBehaviourBindingAttribute;
                        if (attribute == null)
                        {
                            throw new Exception($"{behaviour.GetType()} is not inherit from MonoBehaviourBindingAttribute!");
                        }
                        MonoScript script = MonoScript.FromMonoBehaviour(behaviour);
                        MonoBehaviourBinder binder = new MonoBehaviourBinder(script, behaviour.transform, attribute.NameSeparator);
                        binder.TrySerializeBindings();
                        Debug.Log($"[CodeBind] Refresh '{behaviour.name}({behaviour})' serialization successfully.");
                    }
                }
                PlainClassBindingHost[] hosts = gameObject.GetComponentsInChildren<PlainClassBindingHost>(true);
                foreach (PlainClassBindingHost host in hosts)
                {
                    PlainClassBinder binder = new PlainClassBinder(host.BindingClassScript, host.transform, host.NameSeparator);
                    binder.TrySerializeBindings();
                    Debug.Log($"[CodeBind] Refresh '{host.name}({host})' serialization successfully.");
                }
            }
        }
    }
}
