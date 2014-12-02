using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System;
using UnityEngineInternal;
using UnityEditorInternal;

public static class ResourceManageToolUtility
{
    public static bool PathIsFolder( string path )
    {
        FileAttributes fAttrib = File.GetAttributes(path);
        return ((fAttrib & FileAttributes.Directory) != 0);
    }

    public static string GuidToPath( Guid id )
    {
        return AssetDatabase.GUIDToAssetPath(id.ToString("N"));
    }

    public static Guid PathToGuid( string path )
    {
        return new Guid( AssetDatabase.AssetPathToGUID(path) );
    }

    public static string[] GetAllAssetPaths()
    {
        return AssetDatabase.GetAllAssetPaths();
    }

    public static string[] GetDependencies(string path )
    {
        return AssetDatabase.GetDependencies(new string[] { path });
    }

    public static Texture GetCachedIcon( string path )
    {
        return AssetDatabase.GetCachedIcon(path);
    }

    public static void RefreshAssetDatabase()
    {
        AssetDatabase.Refresh();
    }

    public static bool TryGetAssetTypeFromMetaFile( string path , ref string assetType )
    {
        string metaPath = AssetDatabase.GetTextMetaDataPathFromAssetPath(path);
        string projPath = EditorHelper.GetProjectPath();
        assetType = "";
        StreamReader sr = null;
        try
        {
            sr = File.OpenText(projPath + metaPath);
        }
        catch (Exception e)
        {//meta打开失败直接忽略此文件 
            return false;
        }


        bool ret = false;
        string line = "";
        while ((line = sr.ReadLine()) != null)
        {
            
            if (-1 != line.IndexOf("TextureImporter"))
            {
                assetType = "Texture";
                ret = true;
                break;
            }
            else if(-1 != line.IndexOf("DDSImporter"))
            {
                assetType = "Texture";
                ret = true;
                break;
            }
            else if (-1 != line.IndexOf("ModelImporter"))
            {
                assetType = "Model";
                ret = true;
                break;
            }
            else if (-1 != line.IndexOf("TextScriptImporter"))
            {
                assetType = "Text";
                ret = true;
                break;
            }
            else if (-1 != line.IndexOf("ShaderImporter"))
            {
                assetType = "Shader";
                ret = true;
                break;
            }
            else if (-1 != line.IndexOf("MonoImporter"))
            {
                assetType = "Script";
                ret = true;
                break;
            }
            else if (-1 != line.IndexOf("MonoAssemblyImporter"))
            {
                assetType = "MonoAssembly";
                ret = true;
                break;
            }else if(-1 != line.IndexOf("ComputeShaderImporter"))
            {
                assetType = "ComputeShader";
                ret = true;
                break;
            }else if(-1 != line.IndexOf("TrueTypeFontImporter"))
            {
                assetType = "TrueTypeFont";
                ret = true;
                break;
            }
            else if (-1 != line.IndexOf("AudioImporter"))
            {
                assetType = "AudioClip";
                ret = true;
                break;
            }
        }

        sr.Close();
        return ret;
    }
    
    public static string GetAssetTypeName( string path ) 
    {
        string typeName = "";

        //先判断路径是否指向的是文件夹

        if (PathIsFolder(path))
        {
            return "Folder"; 
        }

        if( TryGetAssetTypeFromMetaFile(path,ref typeName) )
        {//先尝试从meta文件中推测文件类型
            return typeName;
        }

        typeName = "UnKnown";

        string ext = Path.GetExtension(path);
        if (ext.Equals(".unity"))
        {
            typeName = "Scene";
        }else if (ext.Equals(".prefab"))
        {
            typeName = "Prefab";
        }else if(ext.Equals(".anim"))
        {
            typeName = "AnimationClip";
        }
        else if (ext.Equals(".controller"))
        {
            typeName = "AnimatorController";
        }else if(ext.Equals(".mat"))
        {
            typeName = "Material";
        }
        else if (ext.Equals(".physicMaterial"))
        {
            typeName = "PhyMaterial";
        }
        else if (ext.Equals(".guiskin"))
        {
            typeName = "GUISkin";
        }
        else if (ext.Equals(".fontsettings"))
        {
            typeName = "Font";
        }
        else if (ext.Equals(".overrideController"))
        {
            typeName = "OverrideController";
        }
        else if (ext.Equals(".mask"))
        {
            typeName = "AvatarMask";
        }
        else if (ext.Equals(".cubemap"))
        {
            typeName = "CubeMap";
        }
        else if (ext.Equals(".flare"))
        {
            typeName = "Flare";
        }
        else if (ext.Equals(".renderTexture"))
        {
            typeName = "RenderTexture";
        }
        else if (ext.Equals(".physicsMaterial2D"))
        {
            typeName = "Phy2DMaterial";
        }


        return typeName;
    }

    public static void InitAssetInfo( string path , ref U3DAssetInfo assetInfo )
    {
        assetInfo.path = path;
        assetInfo.typeName = GetAssetTypeName(path);
        assetInfo.guid = new Guid(AssetDatabase.AssetPathToGUID(path));
        InitAssetInfoIcon(ref assetInfo); 
    }

    public static void MarkAssetCorrupted( ref U3DAssetInfo assetInfo )
    {
        assetInfo.Corrupted = true;
        assetInfo.TypeName = "Corrupted";
        assetInfo.icon = UnityInternalIconCache.GetInstance().GetCacheIcon("d_console.erroricon.sml");
    }

    public static void InitAssetInfoIcon( ref U3DAssetInfo assetInfo )
    {
        if (assetInfo.TypeName.Equals("Texture"))
        { 
            assetInfo.icon = AssetPreview.GetMiniTypeThumbnail(typeof(Texture));
        }else if( assetInfo.TypeName.Equals("CubeMap"))
        {
            assetInfo.icon = AssetPreview.GetMiniTypeThumbnail(typeof(Cubemap));
        }else if( assetInfo.typeName.Equals("RenderTexture"))
        {
            assetInfo.icon = AssetPreview.GetMiniTypeThumbnail(typeof(RenderTexture));
        }
        else
        {
            assetInfo.icon = GetCachedIcon(assetInfo.path);
        }
    }
}
