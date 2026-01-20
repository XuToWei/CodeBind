using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CodeBind.Editor
{
    internal sealed class MonoCodeBindPropertyProcessor<T> : OdinPropertyProcessor<T, MonoCodeBindAttribute>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            MonoCodeBindAttribute attribute = Property.GetAttribute<MonoCodeBindAttribute>();
            
            propertyInfos.AddDelegate("Code Binder", (Action) (() => { }), -100000f, new Attribute[2]
            {
                (Attribute) new InfoBoxAttribute($"Separator Char:{attribute.SeparatorChar}"),
                (Attribute) new OnInspectorGUIAttribute("@")
            });

            propertyInfos.AddDelegate("Generate Bind Code", TryGenerateBindCode, -100000f);
            propertyInfos.AddDelegate("Generate Serialization", TrySetSerialization, -100000f);
        }

        private void TryGenerateBindCode()
        {
            foreach (T t in ValueEntry.Values)
            {
                MonoCodeBindAttribute attribute = Property.GetAttribute<MonoCodeBindAttribute>();
                MonoBehaviour mono = t as MonoBehaviour;
                if (mono == null)
                {
                    throw new Exception($"{t.GetType()} is not inherit from MonoBehaviour!");
                }
                MonoScript script = MonoScript.FromMonoBehaviour(mono);
                MonoCodeBinder codeBinder = new MonoCodeBinder(script, mono.transform, attribute.SeparatorChar);
                codeBinder.TryGenerateBindCode();
            }

            if (ValueEntry.Values.Count > 0)
            {
                SessionState.SetBool("CodeBind.NeedTrySetSerialization", true);
            }
        }

        private void TrySetSerialization()
        {
            foreach (T t in ValueEntry.Values)
            {
                MonoCodeBindAttribute attribute = Property.GetAttribute<MonoCodeBindAttribute>();
                MonoBehaviour mono = t as MonoBehaviour;
                if (mono == null)
                {
                    throw new Exception($"{t.GetType()} is not inherit from MonoBehaviour!");
                }
                MonoScript script = MonoScript.FromMonoBehaviour(mono);
                MonoCodeBinder codeBinder = new MonoCodeBinder(script, mono.transform, attribute.SeparatorChar);
                codeBinder.TrySetSerialization();
            }
        }
    }

    internal static class MonoCodeBindReloadScripts
    {
        [DidReloadScripts]
        private static void OnReloadScripts()
        {
            if (!SessionState.GetBool("CodeBind.NeedTrySetSerialization", false))
            {
                return;
            }
            SessionState.EraseBool("CodeBind.NeedTrySetSerialization");

            foreach (GameObject go in Selection.gameObjects)
            {
                foreach (MonoBehaviour mono in go.GetComponents<MonoBehaviour>())
                {
                    foreach (var customAttribute in mono.GetType().GetCustomAttributes(typeof(MonoCodeBindAttribute), false))
                    {
                        MonoCodeBindAttribute attribute = customAttribute as MonoCodeBindAttribute;
                        if (attribute == null)
                        {
                            continue;
                        }
                        MonoScript script = MonoScript.FromMonoBehaviour(mono);
                        MonoCodeBinder codeBinder = new MonoCodeBinder(script, mono.transform, attribute.SeparatorChar);
                        codeBinder.TrySetSerialization();
                    }
                }
            }
        }
    }
}
