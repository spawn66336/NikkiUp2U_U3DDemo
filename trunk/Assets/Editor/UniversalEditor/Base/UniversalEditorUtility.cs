using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class UniversalEditorUtility
{
    public static void MakeFileWriteable(string path)
    {
        if ((path == null) || (path == ""))
        {
            return;
        }

        if (File.Exists(path))
        {
            File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.Hidden);
            File.SetAttributes(path, File.GetAttributes(path) & ~(FileAttributes.Archive | FileAttributes.ReadOnly));
        }
    }
}