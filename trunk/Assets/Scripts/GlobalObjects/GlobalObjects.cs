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

        Debug.Log(Application.dataPath);
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

    public void ShowBusyIcon( bool show )
    {
        if( busyPanel.activeInHierarchy != show )
        {
            busyPanel.SetActive(show);
        }
    }

    public void ShowLoadingPanel( bool show )
    {
        if( loadingPanel.activeInHierarchy != show )
        {
            loadingPanel.SetActive(show);
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

    //忙等界面
    public GameObject busyPanel;
    //读取界面
    public GameObject loadingPanel;

    private string[] sceneNames = new string[]{"EntranceAnim","Login","Map","CoreGame"};
    private int i = 0;

    private static GlobalObjects s_instance;

}
