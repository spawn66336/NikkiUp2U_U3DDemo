using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AtlasDialog : EditorWindow
{
    public string m_strProjectName = "New Project";

    void OnGUI()
    {
        maxSize = new Vector2(500f, 100f);
        minSize = new Vector2(500f, 100f);
       
        GUILayout.Space(20f); 
        GUILayout.BeginHorizontal();
        GUILayout.Label("工程名", GUILayout.Width(60f));
        m_strProjectName = GUILayout.TextField(m_strProjectName, 150, GUILayout.Width(250f));
        GUILayout.EndHorizontal();

        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();

        if(GUILayout.Button("确认", GUILayout.Width(76f)))
        {
            if ((m_strProjectName != null) && (m_strProjectName != ""))
            {
                //清除现有工程
                UIAtlasEditorModel.GetInstance().ClearCurrentProject();
                //创建新工程
                UIAtlasEditorModel.GetInstance().NewPorject(m_strProjectName);

                this.Close();
            }

        }

        if (GUILayout.Button("取消", GUILayout.Width(76f)))
        {
            this.Close();
        }

        GUILayout.EndHorizontal();

      
    }
}
