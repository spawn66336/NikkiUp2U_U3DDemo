using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using H3DEditor;
public class ResDialog : EditorWindow
{
    public MainView mainview = null;
    void OnGUI()
    {
        maxSize = new Vector2(200f, 300f);
        minSize = new Vector2(200f, 300f);

        GUILayout.BeginVertical();

        string[] resolusion = ConfigTool.Instance.vResolutionPreview.ToArray();
        if(resolusion.Length > 0)
        {
            int nSel = GUILayout.SelectionGrid(-1, resolusion, 1);
            if (nSel >= 0 && nSel < resolusion.Length)
            {
                string sSel = resolusion[nSel];
                sSel = sSel.Trim();
                int nIndex = sSel.IndexOf('*');
                if (nIndex >= 0)
                {
                    int nW = 0;
                    int nH = 0;
                    int.TryParse(sSel.Substring(0, nIndex), out nW);
                    int.TryParse(sSel.Substring(nIndex + 1), out nH);
                    if (nW > 0 && nH > 0 && mainview != null)
                    {
                        mainview.ChangeResolution(nW, nH);
                    }
                }
            }
        }
        
        GUILayout.EndVertical();
    }
}
