using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization;

public enum PROJECT_TYPE
{//工程类型
    PROJECT_TYPE_NEW = 0,   //新建工程
    PROJECT_TYPE_EXIST,     //已有工程
}

public enum PROJECT_FAILED_TYPE
{//工程操作错误类型
    PROJECT_FAILED_SPRITEIMAGE_PATH_ERROR = 0,  //小图路径错误
    PROJECT_FAILED_SPRITE_EXIST_ERROR,       //小图已存在
    PROJECT_FAILED_ATLASOUTPU_PATH_ERROR,       //Atlas输入路径错误
    PROJECT_FAILED_NONE_IMAGE_ERROR,            //未添加小图
    PROJECT_FAILED_NONE_PROJECT_ERROR,          //未创建工程
    PROJECT_FAILED_DEFAULT = -1,                //默认值
}

public class SpriteImageInfo
{//小图信息（记录于工程文件中）

    private string m_spritePath = null; //小图路径（相对于配置路径）
    private float m_zoomScale = 0f;     //小图在Atlas中的缩放比例

    public string SpritePath { get { return m_spritePath; } set { m_spritePath = value; } }
    public float ZoomScale { get { return m_zoomScale; } set { m_zoomScale = value; } }

}

public class AtlasSerializeObject
{//Atals序列化对象（持久化使用）

    private string m_atlasOutputPath = null;                                //Atlas输入路径（相对于Unity路径）

    private List<KeyValuePair<string, SpriteImageInfo>> m_spriteInfoTable;  //小图信息

    public string AtlasOutputPath { get { return m_atlasOutputPath; } set { m_atlasOutputPath = value; } }

    public List<KeyValuePair<string, SpriteImageInfo>> SpriteInfoTable
    {
        get { return m_spriteInfoTable; }
        set { m_spriteInfoTable = value; }
    }
}

public class AtlasProject
{//Atals工程

    public bool CheckSpriteImagePath(string path)
    {//检查待添加的小图的路径是否正确
        bool bRet = false;

        if ((path == null) || (UIAtlasEditorConfig.ImageBasePath == null))
        {
            bRet = false;
        }
        else
        {
            ImageRelativePath = UIAtlasEditorConfig.ImageBasePath;
            ImageRelativePath = ImageRelativePath.Replace(@"/", @"\");
            bRet = path.Contains(ImageRelativePath);
        }
      
        return bRet;
    }

#region 工程操作函数
    public void Save()
    {//保存工程文件

        if (Path == null)
        {
            return;
        }

        //创建工程文件
        StreamWriter yamlWriter = File.CreateText(Path);
        Serializer yamlSerializer = new Serializer();

        //制作持久化对象
        object obj = GetSerializeObject();

        //将持久化对象写入工程文件
        yamlSerializer.Serialize(yamlWriter, obj);
        yamlWriter.Close();
    }

    public AtlasSerializeObject Load(string path)
    {//读取工程文件

        if (path == null)
        {
            return null;
        }
        //打开工程文件
        StreamReader yamlReader = File.OpenText(path);
        Deserializer yamlDeserializer = new Deserializer();

        //读取持久化对象
        var obj = yamlDeserializer.Deserialize<AtlasSerializeObject>(yamlReader);

        //更新小图信息
        ApplySerializeObject(obj);


        yamlReader.Close();
        return obj;
    }

    public bool AddSpriteImage(string path)
    {//向工程中添加小图

        bool bRet = true;

        if (path == null)
        {
            ProjectFailedType = PROJECT_FAILED_TYPE.PROJECT_FAILED_SPRITEIMAGE_PATH_ERROR;
            return false;
        }

        AtlasSpriteImage spriteImage = new AtlasSpriteImage();

        //首次添加时缩放比例默认是1
        spriteImage.ZoomScale = 1f;

        if (CheckSpriteImagePath(path))
        {//路径合法

            spriteImage.Path = path.Substring(ImageRelativePath.Length);
            spriteImage.Name = path.Substring(path.LastIndexOfAny(new char[] { '/', '\\' }) + 1);

            foreach (var sprite in spriteImages)
            {
                if (sprite.Path == spriteImage.Path)
                {
                    //更新错误类型
                    ProjectFailedType = PROJECT_FAILED_TYPE.PROJECT_FAILED_SPRITE_EXIST_ERROR;
                    bRet = false;
                    break;
                }
            }
        }
        else
        {//路径非法

            //更新错误类型
            ProjectFailedType = PROJECT_FAILED_TYPE.PROJECT_FAILED_SPRITEIMAGE_PATH_ERROR;
            bRet = false;
        }

        if (bRet)
        {
            //添加小图
            spriteImages.Add(spriteImage);
            bRet = true;
        }

        return bRet;
    }

    public bool RemoveSpriteImage(string path)
    {//从工程中移除小图

        bool bRet = true;
        string spritePath = null;

        if (path == null)
        {
            return false;
        }

        //查询待删除的小图
        foreach (var ImageInfo in spriteImages)
        {
            //获取小图的相对于配置目录的路径
            spritePath = path.Substring(path.IndexOfAny(ImageRelativePath.ToCharArray()) + ImageRelativePath.Length);
            if (ImageInfo.Path == spritePath)
            {
                spriteImages.Remove(ImageInfo);
                break;
            }
        }

        return bRet;
    }

    public bool GetSpriteImage(string path, out AtlasSpriteImage spriteImage)
    {//获取指定文件名的小图

        bool bRet = true;

        if (path == null)
        {
            spriteImage = null;
            return false;
        }

        string tempPath = path.Substring(path.IndexOfAny(ImageRelativePath.ToCharArray()) + ImageRelativePath.Length);
        spriteImage = null;

        //查询目标小图
        foreach (var sprite in spriteImages)
        {
            if (sprite.Path == tempPath)
            {
                spriteImage = sprite;
                break;
            }
        }

        return bRet;
    }

    public List<AtlasSpriteImage> GetAllSprites()
    {//获取工程中全部小图

        return spriteImages;
    }

    public void ClearSpriteImage()
    {//清空小图

        spriteImages.Clear();
    }

    private AtlasSerializeObject GetSerializeObject()
    {//制作待持久化的对象

        AtlasSerializeObject obj = new AtlasSerializeObject();

        //设定Atals输出路径（相对Until的路径）
        obj.AtlasOutputPath = AtlasSavePath;

        List<KeyValuePair<string, SpriteImageInfo>> spriteImageInfo = new List<KeyValuePair<string, SpriteImageInfo>>();

        //设定小图路径（相对配置路径）、小图缩放比例
        foreach (var ImageInfo in spriteImages)
        {
            SpriteImageInfo newInfo = new SpriteImageInfo();
            newInfo.SpritePath = ImageInfo.Path;
            newInfo.ZoomScale = ImageInfo.ZoomScale;

            KeyValuePair<string, SpriteImageInfo> spriteInfoKeyPair = new KeyValuePair<string, SpriteImageInfo>
            (ImageInfo.Name, newInfo);
            spriteImageInfo.Add(spriteInfoKeyPair);
        }

        obj.SpriteInfoTable = spriteImageInfo;

        return obj;
    }

    private bool ApplySerializeObject(AtlasSerializeObject obj)
    {//获取持久化对象

        bool bRet = true;

        if (obj == null)
        {
            return false;
        }

        //获取Atlas输出路径
        AtlasSavePath = obj.AtlasOutputPath;
        spriteImages.Clear();

        //获取小图信息
        foreach (var ImageInfo in obj.SpriteInfoTable)
        {
            AtlasSpriteImage image = new AtlasSpriteImage();
            image.Name = ImageInfo.Key;
            image.Path = ImageInfo.Value.SpritePath;
            image.ZoomScale = ImageInfo.Value.ZoomScale;
            image.Texture = null;

            spriteImages.Add(image);
        }

        return bRet;
    }
#endregion


#region Sprite操作函数
    public void SetSpriteImageZoom(string path , float scaleFactor)
    {//变更小图在Atlas中的缩放比例

        if(path == null)
        {
            return;
        }

        //查询目标小图
        foreach (var ImageInfo in spriteImages)
        {
            string spritePath = path.Substring(path.IndexOfAny(ImageRelativePath.ToCharArray()) + ImageRelativePath.Length);
            if (ImageInfo.Path == spritePath)
            {
                ImageInfo.ZoomScale = scaleFactor;
                break;
            }
        }
    }

    public bool PreViewAtlastexture(Texture2D tex, Texture2D[] imgs, out Texture2D outTex)
    {//获取预览Atals

        outTex = null;
        DefaultTexturePackagingStrategy maker = new DefaultTexturePackagingStrategy();

        //打包纹理
        if(maker.Pack(tex, imgs, null))
        {
            outTex = tex;
        }

        return true;
    }

    public bool MakeAtlasTexture(Texture2D tex, Texture2D[] imgs)
    {//生成Atlas Png

        string newPath = null;
        bool bRet = false;

        if ((atlasSavePath == null) || (name == null))
        {
            return false;
        }

        DefaultTexturePackagingStrategy maker = new DefaultTexturePackagingStrategy();

        //打包纹理
        if(maker.Pack(tex, imgs, null))
        {
            if(tex == null)
            {
                return false;
            }

            //创建png文件
            byte[] bytes = tex.EncodeToPNG();
            newPath = atlasSavePath + name + ".png";
            System.IO.File.WriteAllBytes(newPath, bytes);
            bytes = null;
            bRet = true;
        }

        return bRet;
    }

    public void MakeAtlasPrefab(string outputPath)
    {//生成Atlas prefab

        if ((outputPath == null) || (!outputPath.Contains(".prefab")))
        {
            return;
        }

        GameObject go = AssetDatabase.LoadAssetAtPath(outputPath, typeof(GameObject)) as GameObject;
        Object prefab = (go != null) ? go : PrefabUtility.CreateEmptyPrefab(outputPath);
       
        string atlasName = outputPath.Replace(".prefab", "");
        atlasName = atlasName.Substring(outputPath.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
       
        go = new GameObject(atlasName);     
        PrefabUtility.ReplacePrefab(go, prefab);
        UnityEngine.Object.DestroyImmediate(go);
       
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
      }
#endregion

#region 成员变量
    private string name = null;                         //工程名（不包含路径信息和扩展名）
    private string path = null;                         //工程路径（绝对路径）
    private string atlasSavePath = null;                //Atals输出路径（相对路径）
    private string imageRelativePath = null;            //小图配置路径
    
    private PROJECT_TYPE projcetType;                   //工程类型
    private PROJECT_FAILED_TYPE failedType;             //失败类型
    
    private List<AtlasSpriteImage> spriteImages = new List<AtlasSpriteImage>(); //存储小图资源

    public string Name { get { return name; } set { name = value; } }         
    public string Path { get { return path; } set { path = value; } }         
    public string ImageRelativePath { get { return imageRelativePath; } set { imageRelativePath = value; } }

    public PROJECT_TYPE ProjectType { get { return projcetType; } set { projcetType = value; } }
    public PROJECT_FAILED_TYPE ProjectFailedType { get { return failedType; } set { failedType = value; } }
    public string AtlasSavePath { get { return atlasSavePath; } set { atlasSavePath = value; } }
#endregion
}
