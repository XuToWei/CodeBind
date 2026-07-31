using UnityEditor;

namespace CodeBind.Editor
{
    internal static class BindingEditorPreferences
    {
        internal static string GetOutputPath()
        {
            return EditorPrefs.GetString("CodeBind.OutputPath", "Assets/");
        }

        internal static void SetOutputPath(string path)
        {
            EditorPrefs.SetString("CodeBind.OutputPath", path);
        }

        internal static string GetDefaultNamespace()
        {
            return EditorPrefs.GetString("CodeBind.DefaultNamespace", string.Empty);
        }

        internal static void SetDefaultNamespace(string namespaceName)
        {
            EditorPrefs.SetString("CodeBind.DefaultNamespace", namespaceName);
        }

        internal static char GetNameSeparator()
        {
            return EditorPrefs.GetString("CodeBind.NameSeparator", "_")[0];
        }

        internal static void SetNameSeparator(char nameSeparator)
        {
            EditorPrefs.SetString("CodeBind.NameSeparator", nameSeparator.ToString());
        }
    }
}
