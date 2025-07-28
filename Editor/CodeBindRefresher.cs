using System;
using UnityEditor;
using UnityEngine;

namespace CodeBind.Editor
{
    internal class CodeBindRefresher
    {
        [MenuItem("GameObject/CodeBind/Refresh All Code", priority = -2)]
        private static void RefreshAllMonoCode()
        {
            if (Selection.gameObjects.Length < 1)
            {
                Debug.LogError("Please select at least one GameObject to refresh bind code.");
                return;
            }
            foreach (GameObject go in Selection.gameObjects)
            {
                MonoBehaviour[] monos = go.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (MonoBehaviour mono in monos)
                {
                    object[] attributes = mono.GetType().GetCustomAttributes(typeof(MonoCodeBindAttribute), false);
                    if (attributes.Length > 0)
                    {
                        MonoCodeBindAttribute attribute = attributes[0] as MonoCodeBindAttribute;
                        if (attribute == null)
                        {
                            throw new Exception($"{mono.GetType()} is not inherit from MonoCodeBindAttribute!");
                        }
                        MonoScript script = MonoScript.FromMonoBehaviour(mono);
                        MonoCodeBinder codeBinder = new MonoCodeBinder(script, mono.transform, attribute.SeparatorChar);
                        codeBinder.TryGenerateBindCode();
                        Debug.Log($"Refresh '{mono.name}({mono})' code successfully.");
                    }
                }
                CSCodeBindMono[] csCodeBinds = go.GetComponentsInChildren<CSCodeBindMono>(true);
                foreach (CSCodeBindMono bindMono in csCodeBinds)
                {
                    CSCodeBinder codeBinder = new CSCodeBinder(bindMono.BindScript, bindMono.transform, bindMono.SeparatorChar);
                    codeBinder.TryGenerateBindCode();
                }
            }
        }

        [MenuItem("GameObject/CodeBind/Refresh All Serialization", priority = -1)]
        private static void RefreshAllMonoSerialization()
        {
            if (Selection.gameObjects.Length < 1)
            {
                Debug.LogError("Please select at least one GameObject to refresh bind serialization.");
            }
            foreach (GameObject go in Selection.gameObjects)
            {
                MonoBehaviour[] monos = go.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (MonoBehaviour mono in monos)
                {
                    object[] attributes = mono.GetType().GetCustomAttributes(typeof(MonoCodeBindAttribute), false);
                    if (attributes.Length > 0)
                    {
                        MonoCodeBindAttribute attribute = attributes[0] as MonoCodeBindAttribute;
                        if (attribute == null)
                        {
                            throw new Exception($"{mono.GetType()} is not inherit from MonoCodeBindAttribute!");
                        }
                        MonoScript script = MonoScript.FromMonoBehaviour(mono);
                        MonoCodeBinder codeBinder = new MonoCodeBinder(script, mono.transform, attribute.SeparatorChar);
                        codeBinder.TrySetSerialization();
                        Debug.Log($"Refresh '{mono.name}({mono})' serialization successfully.");
                    }
                }
                CSCodeBindMono[] csCodeBinds = go.GetComponentsInChildren<CSCodeBindMono>(true);
                foreach (CSCodeBindMono bindMono in csCodeBinds)
                {
                    CSCodeBinder codeBinder = new CSCodeBinder(bindMono.BindScript, bindMono.transform, bindMono.SeparatorChar);
                    codeBinder.TrySetSerialization();
                    Debug.Log($"Refresh '{bindMono.name}({bindMono})' serialization successfully.");
                }
            }
        }
    }
}