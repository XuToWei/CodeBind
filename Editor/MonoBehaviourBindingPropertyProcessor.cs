using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CodeBind.Editor
{
    internal sealed class MonoBehaviourBindingPropertyProcessor<T> : OdinPropertyProcessor<T, MonoBehaviourBindingAttribute>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            MonoBehaviourBindingAttribute attribute = Property.GetAttribute<MonoBehaviourBindingAttribute>();

            propertyInfos.AddDelegate("Binding Generator", (Action) (() => { }), -100000f, new Attribute[2]
            {
                (Attribute) new InfoBoxAttribute($"Name Separator:{attribute.NameSeparator}"),
                (Attribute) new OnInspectorGUIAttribute("@")
            });

            propertyInfos.AddDelegate("Generate Binding Source", GenerateBindingSource, -100000f);
            propertyInfos.AddDelegate("Generate Serialization", SerializeBindings, -100000f);
        }

        private void GenerateBindingSource()
        {
            foreach (T targetValue in ValueEntry.Values)
            {
                MonoBehaviourBindingAttribute attribute = Property.GetAttribute<MonoBehaviourBindingAttribute>();
                MonoBehaviour targetBehaviour = targetValue as MonoBehaviour;
                if (targetBehaviour == null)
                {
                    throw new Exception($"{targetValue.GetType()} is not inherit from MonoBehaviour!");
                }
                MonoScript script = MonoScript.FromMonoBehaviour(targetBehaviour);
                MonoBehaviourBinder binder = new MonoBehaviourBinder(script, targetBehaviour.transform, attribute.NameSeparator);
                binder.GenerateBindingSource();
            }

            if (ValueEntry.Values.Count > 0)
            {
                SessionState.SetBool("CodeBind.PendingBindingSerialization", true);
            }
        }

        private void SerializeBindings()
        {
            foreach (T targetValue in ValueEntry.Values)
            {
                MonoBehaviourBindingAttribute attribute = Property.GetAttribute<MonoBehaviourBindingAttribute>();
                MonoBehaviour targetBehaviour = targetValue as MonoBehaviour;
                if (targetBehaviour == null)
                {
                    throw new Exception($"{targetValue.GetType()} is not inherit from MonoBehaviour!");
                }
                MonoScript script = MonoScript.FromMonoBehaviour(targetBehaviour);
                MonoBehaviourBinder binder = new MonoBehaviourBinder(script, targetBehaviour.transform, attribute.NameSeparator);
                binder.UpdateSerializedBindings();
            }
        }
    }

    internal static class BindingSerializationReloadHook
    {
        [DidReloadScripts]
        private static void HandleScriptsReloaded()
        {
            if (!SessionState.GetBool("CodeBind.PendingBindingSerialization", false))
            {
                return;
            }
            SessionState.EraseBool("CodeBind.PendingBindingSerialization");

            foreach (GameObject gameObject in Selection.gameObjects)
            {
                foreach (MonoBehaviour targetBehaviour in gameObject.GetComponents<MonoBehaviour>())
                {
                    foreach (var customAttribute in targetBehaviour.GetType().GetCustomAttributes(typeof(MonoBehaviourBindingAttribute), false))
                    {
                        MonoBehaviourBindingAttribute attribute = customAttribute as MonoBehaviourBindingAttribute;
                        if (attribute == null)
                        {
                            continue;
                        }
                        MonoScript script = MonoScript.FromMonoBehaviour(targetBehaviour);
                        MonoBehaviourBinder binder = new MonoBehaviourBinder(script, targetBehaviour.transform, attribute.NameSeparator);
                        binder.UpdateSerializedBindings();
                    }
                }
            }
        }
    }
}
