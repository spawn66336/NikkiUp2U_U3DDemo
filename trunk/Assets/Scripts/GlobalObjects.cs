using UnityEngine;
using System.Collections;

public class GlobalObjects : MonoBehaviour 
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
	
    void OnGUI()
    {
        Rect rect = new Rect(0, 0, 100, 100);
        if (GUILayout.Button("切场景"))
        {
            int currSceneIndex = i;
            i = (i + 1) % sceneNames.Length;
            Application.LoadLevel(sceneNames[currSceneIndex]);
        }
          
        if (GUILayout.Button("读资源"))
        {
            ResourceManager.GetInstance().Load("Origin/1006-hd",OnResouceLoadFinished);
            ResourceManager.GetInstance().Load("Origin/1sd", OnResouceLoadFinished);
        }
    }

    void OnResouceLoadFinished( ResourceManager.ResourceLoadResult result , UnityEngine.Object obj )
    {
        if (result == ResourceManager.ResourceLoadResult.Ok)
        {
            Debug.Log(obj.name + "资源读取成功！");
        }
        else
        {
            Debug.Log("失败！");
        }
    }

    private string[] sceneNames = new string[]{"EntranceAnim","Login","Map","CoreGame"};
    private int i = 0;

}
