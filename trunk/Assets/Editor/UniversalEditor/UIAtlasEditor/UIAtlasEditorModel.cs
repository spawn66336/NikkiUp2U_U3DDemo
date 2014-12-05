using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class UIAtlasEditorModel 
{//Atlas编辑器工程实现

#region 工程相关处理函数
    public bool NewPorject(string projectName)
    {//创建工程

        bool bRet = true;

        //清空当前工程
        ClearCurrentProject();

        //创建工程对象
        if (null == m_Project)
        {
            m_Project = new AtlasProject();
            m_Project.Name = projectName;
        }

        //执行新建工程回调
        if (onNewProject != null)
        {
            onNewProject();
        }

        //设定工程类型
        m_Project.ProjectType = PROJECT_TYPE.PROJECT_TYPE_NEW;

        return bRet;
     }

    public bool SaveProject(string projectName)
    {//保存工程

        bool bRet = false;

        if(m_Project != null)
        {
            //更新工程路径
            m_Project.Path = projectName;

            //保存工程
            m_Project.Save();
            bRet = true;
        }

        return bRet;
    }

    public bool LoadProject(string projectName)
    {//打开工程
        bool bRet = true;

        //清空当前工程
        ClearCurrentProject();

        //创建新工程
        m_Project = new AtlasProject();

        //更新工程名，工程路径
        m_Project.Path = projectName;
        m_Project.Name = Path.GetFileNameWithoutExtension(projectName);

        //读取工程文件
        AtlasSerializeObject obj = null;
        obj = m_Project.Load(projectName);

        //更新Atlas输出路径
        m_Project.AtlasSavePath = obj.AtlasOutputPath;

        //依次载入工程中全部小图
        List<AtlasSpriteImage> sprites = m_Project.GetAllSprites();
        foreach(var sprite in sprites)
        {
            LoadSpriteImage(UIAtlasEditorConfig.ImageBasePath + sprite.Path);
            if (onSpriteImageLoad != null)
            {
                onSpriteImageLoad(UIAtlasEditorConfig.ImageBasePath + sprite.Path);
            }
        }

        //设定工程类型
        m_Project.ProjectType = PROJECT_TYPE.PROJECT_TYPE_EXIST;

        //设定图库路径
        m_Project.ImageRelativePath = UIAtlasEditorConfig.ImageBasePath;

        return bRet;
    }

    public void ClearCurrentProject()
    {//清空当前工程

        if (m_Project != null)
        {
            //清除工程中小图信息
            m_Project.ClearSpriteImage();
            m_Project = null;
        }

        //清除临时目录中的资源
        UIAtlasTempTextureManager.GetInstance().Clear();

        if (onClearCurrentProject != null)
        {
            onClearCurrentProject();
        }
    }

    public string GetAtlasSavePath()
    {//获取Atlas输出路径

        string path = null;

        if (m_Project != null)
        {
            path = m_Project.AtlasSavePath;
        }

        return path;
    }

    public void SetAtlasSavePath(string path)
    {//设定Atlas输出路径

        if (m_Project != null)
        {
            m_Project.AtlasSavePath = path;
        }
    }

    public bool AddSpriteImage(string path)
    {//添加小图

        Texture2D tex = null;
        bool bRet = false;
        tex = null;

        if (m_Project == null)
        {
            m_Project.ProjectFailedType = PROJECT_FAILED_TYPE.PROJECT_FAILED_NONE_PROJECT_ERROR;
            bRet = false;
        }
        else
        {
            //向工程中添加小图
            if (m_Project.AddSpriteImage(path))
            {
                //载入小图资源
                tex = LoadSpriteImage(path);
                if (null == tex)
                {
                    bRet = false;
                }
                else
                {
                    bRet = true;
                }
            }
            else
            {
                bRet = false;
            }
        }

        if (onAddSpriteImageCommand != null)
        {
            onAddSpriteImageCommand(bRet, path);
        }

        return bRet;
    }

    public bool RemoveSpriteImage(string path)
    {//删除小图

        bool bRet = false;

        if (m_Project == null)
        {
            m_Project.ProjectFailedType = PROJECT_FAILED_TYPE.PROJECT_FAILED_NONE_PROJECT_ERROR;
            bRet = false;
        }
        else
        {
            //卸载小图资源
            if (UnloadSpriteImage(path))
            {
                //从工程中移除小图
                bRet = m_Project.RemoveSpriteImage(path);
            }
            else
            {
                bRet = false;
            }
        }

        if (onDeleteSpriteImageCommand != null)
        {
            onDeleteSpriteImageCommand(bRet, path);
        }

        return bRet;
    }

    public Texture2D LoadSpriteImage(string path)
    {//载入小图资源

        Texture2D retTex = UIAtlasTempTextureManager.GetInstance().LoadTexture(path);
        return retTex;
    }

    public bool UnloadSpriteImage(string path)
    {//卸载小图资源

        bool bRet = false;

        bRet = UIAtlasTempTextureManager.GetInstance().UnloadTexture(path);

        return bRet;
    }

    public bool GetSpriteImage(string path, out AtlasSpriteImage spriteImage)
    {//获取指定小图

        bool bRet = false;

        spriteImage = null;

        if (m_Project != null)
        {
            bRet = m_Project.GetSpriteImage(path, out spriteImage);
        }

        return bRet;
    }

    public Texture2D GetSpriteTexture(string path)
    {//取得小图纹理

        Texture2D retTex = UIAtlasTempTextureManager.GetInstance().GetSpriteTexture(path);
        return retTex;
    }

    public Texture2D GetSpriteZoomTexture(string path)
    {//取得小图纹理

        Texture2D retTex = UIAtlasTempTextureManager.GetInstance().GetSpriteZoomTexture(path);
        return retTex;
    }

    public void ClearSpriteImage()
    {//清空所有小图

        if (m_Project != null)
        {
            m_Project.ClearSpriteImage();
        }
    }

    public void UpdateSprite()
    {//更新全部小图资源

        UIAtlasTempTextureManager.GetInstance().Update();
    }

    public bool IsProjectExist()
    {//判断是否存在工程

        bool bRet = true;

        if (m_Project == null)
        {
            bRet = false;
        }

        return bRet;
    }

    public string GetProjectName()
    {//获取工程名

        string name = null;

        if (m_Project != null)
        {
            name = m_Project.Name;
        }

        return name;
    }

    public string GetProjectPath()
    {//获取工程路径

        string path = null;

        if (m_Project != null)
        {
            path = m_Project.Path;
        }

        return path;
    }

    public PROJECT_FAILED_TYPE GetProjectFailedType()
    {//获取工程失败类型

        PROJECT_FAILED_TYPE type = PROJECT_FAILED_TYPE.PROJECT_FAILED_DEFAULT;

        if (m_Project != null)
        {
            type = m_Project.ProjectFailedType;
        }

        return type;
    }

    public string GetImageRelativePath()
    {//获取图库路径

        string path = null;

        if (m_Project != null)
        {
            path = m_Project.ImageRelativePath;
        }

        return path;
    }

    public PROJECT_TYPE GetProjectType()
    {//获取工程类型

        PROJECT_TYPE type = PROJECT_TYPE.PROJECT_TYPE_NEW;

        if (m_Project != null)
        {
            type = m_Project.ProjectType;
        }

        return type;
    }

    public void SetProjectType(PROJECT_TYPE type)
    {//设定工程类型

        if (m_Project != null)
        {
            m_Project.ProjectType = type;
        }
    }

    public void WriteImagePathConfig(string path)
    {//写配置文件

        UIAtlasEditorConfig.WriteImageBasePath(path);
    }

    public void ReadImagePathConfig()
    {//读配置文件

        UIAtlasEditorConfig.ReadImageBasePath();
    }

    public static void OnBasePathChange(string newBasePaht)
    {//图库路径变更处理函数

        UIAtlasEditorConfig.ReadImageBasePath();
    }
#endregion

#region Sprite操作函数
    public void ZoomSpriteImage(string path, float scaleFactor)
    {//小图缩放比例变更

        if (m_Project == null)
        {
            return;
        }

        UIAtlasTempTextureManager.GetInstance().ZoomTexture(path, scaleFactor);
        m_Project.SetSpriteImageZoom(path, scaleFactor);
        if (onSpriteZoomChangedCommand != null)
        {
            onSpriteZoomChangedCommand(path);
        }
    }
   
    public bool PreViewAtlas(out Texture2D outTex)
    {//预览Atlas
        bool bRet = false;
        outTex = null;

        if(m_Project == null)
        {
            return false;
        }

        Texture2D tex = new Texture2D(1, 1);
        List<Texture2D> texCaches = UIAtlasTempTextureManager.GetInstance().GetTextureCache();
        Texture2D[] textures = new Texture2D[texCaches.Count];

        //获取全部纹理资源
        for (int i = 0; i < texCaches.Count; ++i) textures[i] = texCaches[i];

        
        if(texCaches.Count != 0)
        {//存在纹理资源

            //预览
            m_Project.PreViewAtlastexture(tex, textures, out outTex);
            bRet = true;
        }
        else
        {//不存在

            //设定失败类型为小图不存在
            m_Project.ProjectFailedType = PROJECT_FAILED_TYPE.PROJECT_FAILED_NONE_IMAGE_ERROR;
            bRet = false;
        }

        return bRet;
    }

    public bool MakeAtlas()
    {//生成Atlas

        bool bRet = true;

        Texture2D tex = new Texture2D(1, 1);
        List<Texture2D> texCaches = UIAtlasTempTextureManager.GetInstance().GetTextureCache();
        Texture2D[] textures = new Texture2D[texCaches.Count];
    
        if (m_Project == null)
        {
            m_Project.ProjectFailedType = PROJECT_FAILED_TYPE.PROJECT_FAILED_NONE_PROJECT_ERROR;
            bRet = false;
        }
        else
        {
            //获取全部小图资源
            for (int i = 0; i < texCaches.Count; ++i) textures[i] = texCaches[i];

            if ((m_Project.AtlasSavePath != null) && (m_Project.AtlasSavePath.Contains("Assets/")))
            {//Atlas输出路径合法

                if (texCaches.Count != 0)
                {//存在资源
                    string outputPath = m_Project.AtlasSavePath + m_Project.Name + ".prefab";
                    //生成Atalas
                    m_Project.MakeAtlasTexture(tex, textures);
                    //生成prefab
                    m_Project.MakeAtlasPrefab(outputPath);
                    bRet = true;
                }
                else
                {//不存在资源

                    //设定失败类型为小图不存在
                    m_Project.ProjectFailedType = PROJECT_FAILED_TYPE.PROJECT_FAILED_NONE_IMAGE_ERROR;
                    bRet = false;
                }

            }
            else
            {//Atlas输出路径不合法
                bRet = false;
                m_Project.ProjectFailedType = PROJECT_FAILED_TYPE.PROJECT_FAILED_ATLASOUTPU_PATH_ERROR;
            }
        }


        if(onMakeAtlasCommand != null)
        {
            onMakeAtlasCommand(bRet);
        }

        return bRet;
    }

    public void DeleteAtlas()
    {//删除Atlas

        string atlasName = null;
        string prefabName = null;

        if ((m_Project == null) 
            || (m_Project.AtlasSavePath == null)
            || (m_Project.Name == null))
        {
            return; 
        }

        atlasName = m_Project.AtlasSavePath + m_Project.Name + ".png";
        prefabName = m_Project.AtlasSavePath + m_Project.Name + ".prefab";

        //删除Prefab文件
        if (File.Exists(prefabName))
        {
            File.Delete(prefabName);
        }

        //删除atlas文件
        if (File.Exists(atlasName))
        {
            File.Delete(atlasName);
        }
    }
#endregion

#region 成员变量
    public AtlasProject m_Project = null;
    static private UIAtlasEditorModel m_Instance = null;

    public delegate void ModelChangeNotify();
    public delegate void SpriteImageLoadNotify(string spritePath);
    public delegate void ClearCurrentProjectNotify();

    public delegate void SpriteZoomChangedCommand(string spritePath);
    public delegate void AddSpriteImageCommand(bool bResult, string spriteName);
    public delegate void DeleteSpriteImageCommand(bool bResult, string spriteName);
    public delegate void MakeAtlasCommand(bool bResult);

    public ModelChangeNotify onNewProject;
    public SpriteImageLoadNotify onSpriteImageLoad;
    public ClearCurrentProjectNotify onClearCurrentProject;

    public SpriteZoomChangedCommand onSpriteZoomChangedCommand;
    public AddSpriteImageCommand onAddSpriteImageCommand;
    public DeleteSpriteImageCommand onDeleteSpriteImageCommand;
    public MakeAtlasCommand onMakeAtlasCommand;

    public static UIAtlasEditorModel GetInstance()
    {
        if (m_Instance == null)
        {
            m_Instance = new UIAtlasEditorModel();
        }
        return m_Instance;
    }

    public void DestoryInstance()
    {
        ClearCurrentProject();
        if (m_Instance != null)
        {
            m_Instance = null;
        }
    }
#endregion
}
