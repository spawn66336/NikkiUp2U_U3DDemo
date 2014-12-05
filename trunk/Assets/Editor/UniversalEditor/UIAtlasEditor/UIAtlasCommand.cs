using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtlasImageZoomCommand : IEditorCommand
{//小图缩放命令

    public float m_oldScaleFactor = 0f;
    public float m_newScaleFactor = 0f;
    public string m_SpriteName = null;

    public string Name { get { return "Atlas SpriteImage Zoom Change"; } }
    public bool DontSaved { get { return true; } }

    public void Execute()
    {
        UIAtlasEditorModel.GetInstance().ZoomSpriteImage(m_SpriteName, m_newScaleFactor);
    }

    public void UnExecute()
    {
        UIAtlasEditorModel.GetInstance().ZoomSpriteImage(m_SpriteName, m_oldScaleFactor);
    }

}

public class AtlasAddSpriteImageCommand : IEditorCommand
{//添加小图命令

    public string m_SpriteName = null;

    public string Name { get { return "Atlas Add SpriteImage "; } }
    public bool DontSaved { get { return true; } }

    public void Execute()
    {
        UIAtlasEditorModel.GetInstance().AddSpriteImage(m_SpriteName);
    }

    public void UnExecute()
    {
        UIAtlasEditorModel.GetInstance().RemoveSpriteImage(m_SpriteName);
    }
}

public class AtlasDeleteSpriteImageCommand : IEditorCommand
{//删除小图命令

    public string m_SpriteName = null;

    public string Name { get { return "Atlas Delete SpriteImage "; } }
    public bool DontSaved { get { return true; } }

    public void Execute()
    {
        UIAtlasEditorModel.GetInstance().RemoveSpriteImage(m_SpriteName);
    }

    public void UnExecute()
    {
        UIAtlasEditorModel.GetInstance().AddSpriteImage(m_SpriteName);
    }
}

public class AtlasMakeAtlas : IEditorCommand
{//生成Atlas命令

    public string Name { get { return "Atlas MakeAtlas"; } }
    public bool DontSaved { get { return true; } } 

    public void Execute()
    {
        UIAtlasEditorModel.GetInstance().MakeAtlas();
    }

    public void UnExecute()
    {
        UIAtlasEditorModel.GetInstance().DeleteAtlas();
    }
}