using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System;
using System.Text;
using System.Globalization;
public class TempTextureInfo
{//Temp文件夹中资源信息

    private Texture2D m_texture = null;         //纹理
    private Texture2D m_ZoomTexture = null;     //缩放纹理
    private string m_sourcePath = null;         //源文件绝对路径
    private string m_tempPath = null;           //Temp文件路径

    public Texture2D Texture { get { return m_texture; } set { m_texture = value; } }
    public Texture2D ZoomTexture { get { return m_ZoomTexture; } set { m_ZoomTexture = value; } }

    public string SourcePath { get { return m_sourcePath; } set { m_sourcePath = value; } }
    public string TempPath { get { return m_tempPath; } set { m_tempPath = value; } }

}
public class UIAtlasTempTextureManager
{//Temp文件夹资源

    public UIAtlasTempTextureManager() { }

    public Texture2D LoadTexture( string path )
    {//载入纹理（支持载入磁盘任意位置的纹理）

        _TouchTempDir();

        string fileName = Path.GetFileName(path);
        string zoomedName = null;
        bool isNeedRename = false;
        TempTextureInfo retTexInfo = null;

        //是纹理文件
        if (!IsEnableTextureFile(fileName))
        {
            return null;
        }

        //文件必需存在
        if (!File.Exists(path))
        {
            if( textureCache.ContainsKey(path))
            {
                textureCache.Remove(path);
                EditorUtility.UnloadUnusedAssets();
            }

            if (_IsTextureAssetAlreadyExistsInTempFolder(fileName, out isNeedRename))
            {
                AssetDatabase.DeleteAsset(UIAtlasEditorConfig.TempPath + fileName);
            }
            return null;
        }
        
 
        bool needCopy = false;
        if (_IsTextureAssetAlreadyExistsInTempFolder(path, out isNeedRename))
        {//纹理资源已经存在于缓存文件夹中
            if(!_IsTextureAssetSameModTime(path))
            {
                needCopy = true;
            }
        }
        else
        {
            needCopy = true;
        }


        if (needCopy)
        {
            string oldname = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);

            if (isNeedRename)
            {
                fileName = oldname + "副本" + extension;
                zoomedName = oldname + "副本" + "zoomed.png";
                File.Copy(path, UIAtlasEditorConfig.AbsTempPath + fileName, true);
                File.Copy(path, UIAtlasEditorConfig.AbsTempPath + zoomedName, true);
            }
            else
            {
                zoomedName = oldname + "zoomed.png";
                File.Copy(path, UIAtlasEditorConfig.AbsTempPath + fileName, true);
                File.Copy(path, UIAtlasEditorConfig.AbsTempPath + zoomedName, true);

            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        if (!textureCache.ContainsKey(path))
        {//还未载入内存
            AssetDatabase.ImportAsset(UIAtlasEditorConfig.TempPath + fileName);
            MakeTextureReadable(UIAtlasEditorConfig.TempPath + fileName, false);
         
            TempTextureInfo newTexInfo = new TempTextureInfo();
            newTexInfo.SourcePath = path;
            newTexInfo.TempPath = UIAtlasEditorConfig.TempPath + fileName;
            newTexInfo.Texture = AssetDatabase.LoadAssetAtPath(newTexInfo.TempPath, typeof(Texture)) as Texture2D;
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            if (newTexInfo.Texture != null)
            {
                newTexInfo.ZoomTexture = newTexInfo.Texture;
                textureCache.Add(path, newTexInfo);
            }
            
        }

        if (textureCache.ContainsKey(path))
        {
            textureCache.TryGetValue(path, out retTexInfo);
        }

        return retTexInfo.ZoomTexture;
    }

    public bool UnloadTexture(string path)
    {//卸载纹理
        bool bRet = true;

        if(path == null)
        {
            return false;
        }

        foreach (var textureInfo in textureCache)
        {
            if (textureInfo.Key == path)
            {
                AssetDatabase.DeleteAsset(textureInfo.Value.TempPath);
                textureCache.Remove(path);
                bRet = true;
                break;
            }
        }

        return bRet;
    }

    public Texture2D ZoomTexture(string path, float scaleFactor)
    {//缩放纹理

        Texture2D tex = null;

        if ((path == null) ||
            ((-0.000001f < scaleFactor) && (scaleFactor < 0.000001f)))
        {
            return null;
        }

        if (textureCache.ContainsKey(path))
        {
            TempTextureInfo sourceTexInfo = null;

            textureCache.TryGetValue(path, out sourceTexInfo);

            sourceTexInfo.ZoomTexture = ScaleTextureBilinear(sourceTexInfo.Texture, scaleFactor);

            byte[] bytes = sourceTexInfo.ZoomTexture.EncodeToPNG();
            string newPath = Path.GetFileNameWithoutExtension(sourceTexInfo.TempPath);
            newPath = UIAtlasEditorConfig.TempPath + newPath + "zoomed.png";
            System.IO.File.WriteAllBytes(newPath, bytes);
            bytes = null;

            AssetDatabase.ImportAsset(newPath);
            MakeTextureReadable(newPath, false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            sourceTexInfo.ZoomTexture = AssetDatabase.LoadAssetAtPath(newPath, typeof(Texture)) as Texture2D;

            tex = sourceTexInfo.ZoomTexture;
        }
        return tex;
    }

    public Texture2D GetSpriteTexture(string path)
    {//获取原始纹理

        Texture2D tex = null;
        if (path == null)
        {
            return null;
        }

        foreach (var textureInfo in textureCache)
        {
            if (textureInfo.Key == path)
            {
                tex = textureInfo.Value.Texture;
                break;
            }
        }

        return tex;
    }

    public Texture2D GetSpriteZoomTexture(string path)
    {//获取缩放纹理

        Texture2D tex = null;
        if(path == null)
        {
            return null;
        }

        foreach (var textureInfo in textureCache)
        {
            if (textureInfo.Key == path)
            {
                tex = textureInfo.Value.ZoomTexture;
                break;
            }
        }

        return tex;
    }

    public List<Texture2D> GetTextureCache()
    {//获取全部纹理资源

        List<Texture2D> textureList = new List<Texture2D>();

        foreach (KeyValuePair<string, TempTextureInfo> texture in textureCache)
        {
            textureList.Add(texture.Value.Texture);
        }

        return textureList;
    }

    public void Update()
    {//更新全部纹理

        foreach( var texInfo in textureCache )
        {
            LoadTexture(texInfo.Key);
        }
    }

    public void Clear()
    {//清空临时文件夹资源

        List<string> assetPaths = new List<string>();
        foreach( var tex in textureCache )
        {
            string fileName = Path.GetFileName(tex.Key);
            assetPaths.Add(UIAtlasEditorConfig.TempPath + fileName);
        }
        textureCache.Clear();

        foreach( var path in assetPaths )
        {
            AssetDatabase.DeleteAsset(path);
        }

        //删除临时文件夹
        if (Directory.Exists(UIAtlasEditorConfig.TempPath))
        {
            DirectoryInfo info = new DirectoryInfo(UIAtlasEditorConfig.TempPath);
            DeleteFileByDirectory(info);
        }
    }

    public void DeleteFileByDirectory(DirectoryInfo info)
    {//删除临时文件夹

        foreach (DirectoryInfo newInfo in info.GetDirectories())
        {
            DeleteFileByDirectory(newInfo);
        }
        foreach (FileInfo newInfo in info.GetFiles())
        {
            newInfo.Attributes = newInfo.Attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
            newInfo.Delete();
        }

        info.Attributes = info.Attributes & ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
        info.Delete();
    }

    private bool _IsTextureAssetAlreadyExistsInTempFolder(string path,out bool bIsNeedRename)
    {
        bool bRet = false;
        string fileName = Path.GetFileName(path);

        if (File.Exists(UIAtlasEditorConfig.AbsTempPath + fileName))
        {
            bIsNeedRename = true;
            bRet = false;
            foreach (var textureInfo in textureCache)
            {
                if (textureInfo.Key == path)
                {
                    bRet = true;
                    bIsNeedRename = false;
                    break;
                }
            }
        }
        else 
        {
            bIsNeedRename = false;
            bRet = false;
        }

        return bRet;
    }

    private bool _IsTextureAssetSameModTime(string path)
    {
        string fileName = Path.GetFileName(path);
        string assetFilePath = UIAtlasEditorConfig.AbsTempPath + fileName;
        DateTime orginFileWriteTime = File.GetLastWriteTime(path);
        DateTime assetFileWriteTime = File.GetLastWriteTime(assetFilePath);
        return orginFileWriteTime.Equals(assetFileWriteTime);
    }

    private void _TouchTempDir()
    {
        //查看缓存文件夹是否存在
        if (!Directory.Exists(UIAtlasEditorConfig.AbsTempPath))
        {
            Directory.CreateDirectory(UIAtlasEditorConfig.AbsTempPath);
        }
    }

    private bool MakeTextureReadable(string path, bool force)
    {//使纹理可读

        if (string.IsNullOrEmpty(path)) return false;
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return false;

        TextureImporterSettings settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);

        if (force || !settings.readable || settings.npotScale != TextureImporterNPOTScale.None
#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1
 || settings.alphaIsTransparency
#endif
)
        {
            settings.readable = true;
            settings.textureFormat = TextureImporterFormat.ARGB32;
            settings.npotScale = TextureImporterNPOTScale.None;
#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1
            settings.alphaIsTransparency = false;
#endif
            ti.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }

        return true;
    }

    private Texture2D ScaleTextureBilinear(Texture2D originalTexture, float scaleFactor)
    {//缩放纹理

        if ((originalTexture == null) 
            || ((-0.000001f < scaleFactor) && (scaleFactor < 0.000001f)))
        {
            return null;
        }

        if (scaleFactor == 1.0f)
        {
            return originalTexture;
        }

        Texture2D newTexture = new Texture2D(Mathf.CeilToInt(originalTexture.width * scaleFactor), Mathf.CeilToInt(originalTexture.height * scaleFactor));
        float scale = 1.0f / scaleFactor;
        int maxX = originalTexture.width - 1;
        int maxY = originalTexture.height - 1;
        for (int y = 0; y < newTexture.height; y++)
        {
            for (int x = 0; x < newTexture.width; x++)
            {
                // Bilinear Interpolation
                float targetX = x * scale;
                float targetY = y * scale;
                int x1 = Mathf.Min(maxX, Mathf.FloorToInt(targetX));
                int y1 = Mathf.Min(maxY, Mathf.FloorToInt(targetY));
                int x2 = Mathf.Min(maxX, x1 + 1);
                int y2 = Mathf.Min(maxY, y1 + 1);

                float u = targetX - x1;
                float v = targetY - y1;
                float w1 = (1 - u) * (1 - v);
                float w2 = u * (1 - v);
                float w3 = (1 - u) * v;
                float w4 = u * v;
                Color color1 = originalTexture.GetPixel(x1, y1);
                Color color2 = originalTexture.GetPixel(x2, y1);
                Color color3 = originalTexture.GetPixel(x1, y2);
                Color color4 = originalTexture.GetPixel(x2, y2);
                Color color = new Color(Mathf.Clamp01(color1.r * w1 + color2.r * w2 + color3.r * w3 + color4.r * w4),
                    Mathf.Clamp01(color1.g * w1 + color2.g * w2 + color3.g * w3 + color4.g * w4),
                    Mathf.Clamp01(color1.b * w1 + color2.b * w2 + color3.b * w3 + color4.b * w4),
                    Mathf.Clamp01(color1.a * w1 + color2.a * w2 + color3.a * w3 + color4.a * w4)
                    );
                newTexture.SetPixel(x, y, color);
            }
        }

        return newTexture;
    }

    public bool IsEnableTextureFile(string fileName)
    {//判断目标文件是否是纹理

        bool bRet = false;

        if(fileName == null)
        {
            return false;
        }

        if(fileName.EndsWith(".png") 
           ||fileName.EndsWith(".bmp")
           ||fileName.EndsWith(".jpg")
           ||fileName.EndsWith(".jpeg"))
        {
            bRet = true;
        }

        return bRet;
    }

    private Dictionary<string, TempTextureInfo> textureCache = new Dictionary<string, TempTextureInfo>();

    static public UIAtlasTempTextureManager GetInstance()
    {
        if( s_instance == null )
        {
            s_instance = new UIAtlasTempTextureManager();
        }
        return s_instance;
    }

    static public void DestroyInstance()
    {
        if( s_instance != null )
        {
            s_instance.Clear();
            s_instance = null;
        }
    }

    static private UIAtlasTempTextureManager s_instance = null;
}
