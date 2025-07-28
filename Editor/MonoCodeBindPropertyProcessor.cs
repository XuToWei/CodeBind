using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
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
}
