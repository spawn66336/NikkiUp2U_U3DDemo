using UnityEngine;
using System.Collections;

public static class EditorHelper 
{
    public static bool debugMode = true;
    public static bool IsDebugMode()
    {
        return debugMode;
    }

    public static string GetProjectPath()
    {
        string projPath = Application.dataPath;
        projPath = projPath.Substring(0, projPath.LastIndexOf('/') + 1);
        return projPath;
    }
}
