using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtlasImageZoomCommand : IEditorCommand
{
    public float m_oldScaleFactor = 0f;
    public float m_newScaleFactor = 0f;
    public string m_SpriteName = null;

    public string Name { get { return "Atlas SpriteImage Zoom Change"; } }
    public bool DontSaved { get { return false; } }

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
{
    public string m_SpriteName = null;

    public string Name { get { return "Atlas Add SpriteImage "; } }
    public bool DontSaved { get { return false; } }

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
{
    public string m_SpriteName = null;

    public string Name { get { return "Atlas Delete SpriteImage "; } }
    public bool DontSaved { get { return false; } }

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
{
    public string Name { get { return "Atlas MakeAtlas"; } }
    public bool DontSaved { get { return false; } } 

    public void Execute()
    {
        UIAtlasEditorModel.GetInstance().MakeAtlas();
    }

    public void UnExecute()
    {
        UIAtlasEditorModel.GetInstance().DeleteAtlas();
    }
}