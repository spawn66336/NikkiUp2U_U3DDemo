using UnityEngine;
using System.Collections;
using System.IO;

public class UIAtlasEditorConfig 
{
    //图片库路径
    public static string ImageBasePath { get { return imageBasePath; } set { imageBasePath = value; } }

    //得到Atlas编辑器的临时路径
    public static string AbsTempPath { get { return EditorHelper.GetProjectPath()+tempPath;} }

    //得到Atlas编辑器的相对路径（相对于工程路径）
    public static string TempPath { get { return tempPath; } set { tempPath = value; } }

    private static string tempPath = "Assets/Editor/UniversalEditor/UIAtlasEditor/_Temp/";

    private static string imageBasePath = null;

    private static string configPath = "Assets/Editor/UniversalEditor/UIAtlasEditor/Config/UIAtlasConfig.txt";

    public delegate void BasePathChangeNotify(string newBaePath);

    static public BasePathChangeNotify onBasePathChange = UIAtlasEditorModel.OnBasePathChange;

    public static void WriteImageBasePath(string path)
    {
        FileStream fileStream = null;
        StreamWriter streamW = null;

        UniversalEditorUtility.MakeFileWriteable(configPath);
        fileStream = new FileStream(configPath, FileMode.Create);
        streamW = new StreamWriter(fileStream);
      
        streamW.Write(path);

        streamW.Close();
        fileStream.Close();

        imageBasePath = path;
        onBasePathChange(path);
    }

    public static string ReadImageBasePath()
    {
        FileStream fileStream = null;
        StreamReader streamR = null;
        string basePath = null;

        if(File.Exists(configPath))
        {
            UniversalEditorUtility.MakeFileWriteable(configPath);
            fileStream = new FileStream(configPath, FileMode.Open);
            streamR = new StreamReader(fileStream);

            while(!streamR.EndOfStream)
            {
               basePath += streamR.ReadLine();
            }

            streamR.Close();
            fileStream.Close();
        }

        if ((basePath != null) && (basePath != ""))
        {
            basePath = basePath.Replace(@"/", @"\");
            imageBasePath = basePath;
        }


        return basePath;
    }

}
