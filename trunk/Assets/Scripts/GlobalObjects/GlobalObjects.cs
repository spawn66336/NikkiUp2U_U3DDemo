using UnityEngine;
using System.Collections;

public class GlobalObjects : MonoBehaviour 
{
    void Awake()
    { 
        DontDestroyOnLoad(this.gameObject);

        coroutineMgr = GetComponentInChildren<CoroutineManager>();
        logicMain = GetComponentInChildren<LogicMain>();
        resourceMgr = GetComponentInChildren<ResourceManager>();
        uiSwitchMgr = GetComponentInChildren<UISwitchManager>();
        soundMgr = GetComponentInChildren<SoundManager>();

        s_instance = this; 
    }

    void Start()
    {
        soundMgr.Play("yk_bgm", true);
    }

     
    public CoroutineManager GetCoroutineManager() { return coroutineMgr; }

    public LogicMain GetLogicMain() { return logicMain; }

    public ResourceManager GetResouceManager() { return resourceMgr; }

    public UISwitchManager GetUISwitchManager() { return uiSwitchMgr; }

    public SoundManager GetSoundManager() { return soundMgr; }

    void OnGUI()
    {
        
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

    public static GlobalObjects GetInstance() { return s_instance;  }

    //协程管理器
    private CoroutineManager coroutineMgr;
    //游戏逻辑主模块
    private LogicMain logicMain;
    //资源管理器
    private ResourceManager resourceMgr;
    //UI切换管理器
    private UISwitchManager uiSwitchMgr;
    //音效管理器
    private SoundManager soundMgr;

    private string[] sceneNames = new string[]{"EntranceAnim","Login","Map","CoreGame"};
    private int i = 0;

    private static GlobalObjects s_instance;

}
