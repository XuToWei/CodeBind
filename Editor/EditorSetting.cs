using UnityEditor;

namespace CodeBind.Editor
{
    internal static class EditorSetting
    {
        internal static string GetSaveCodePath()
        {
            return EditorPrefs.GetString("CodeBind.CodePath", "Assets/");
        }
        
        internal static void SetSaveCodePath(string path)
        {
            EditorPrefs.SetString("CodeBind.CodePath", path);
        }
        
        internal static string GetSaveCodeNamespace()
        {
            return EditorPrefs.GetString("CodeBind.CodeNamespace", string.Empty);
        }
        
        internal static void SetSaveCodeNamespace(string codeNamespace)
        {
            EditorPrefs.SetString("CodeBind.CodeNamespace", codeNamespace);
        }
        
        internal static char GetSaveSeparatorChar()
        {
            return EditorPrefs.GetString("CodeBind.SeparatorChar", "_")[0];
        }
        
        internal static void SetSaveSeparatorChar(char separatorChar)
        {
            EditorPrefs.SetString("CodeBind.SeparatorChar", separatorChar.ToString());
        }
    }
}