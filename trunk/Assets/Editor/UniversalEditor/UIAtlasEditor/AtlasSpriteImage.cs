using UnityEngine;
using System.Collections;

public class AtlasSpriteImage 
{//小图信息

    private Texture2D m_Texture = null;     //纹理
    private string m_Name = null;           //小图绝对路径
    private string m_Path = null;           //小图相对于配置目录路径
    private float m_ZoomScale;              //小图缩放比例

    public Texture2D Texture { get { return m_Texture; } set { m_Texture = value; } }
    public string Name { get { return m_Name; } set { m_Name = value; } }
    public string Path { get { return m_Path; } set { m_Path = value; } }
    public float ZoomScale { get { return m_ZoomScale; } set { m_ZoomScale = value; } }

}
