using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UIState
{
    None = 0,
    LoginUI = 1,
    AreaMapUI,
    LevelDialogUI,
    LevelDressUI,
    RatingUI
}

public delegate void OnProgressChange(int count, int total);
public delegate void OnPrepareFinished();
public delegate IEnumerator OnPrepareCallback(OnProgressChange progressCallback ,OnPrepareFinished finishedCallback);

public class UIStateDesc
{
    public UIStateDesc( 
        UIState s , 
        string uiName , 
        string scene , 
        OnPrepareCallback onPrepareScene , 
        OnPrepareCallback onCleanScene,
        OnPrepareCallback onPrepareUI,
        OnPrepareCallback onCleanUI
        ) 
    {
        state = s;
        uiPrefabName = uiName;
        sceneName = scene;
        onPrepareSceneCallback = onPrepareScene;
        onCleanSceneCallback = onCleanScene;
        onPrepareUICallback = onPrepareUI;
        onCleanUICallback = onCleanUI;
    }

    //UI状态
    public UIState state;
    //Prefab名称（名称即可）
    public string uiPrefabName;
    //场景名
    public string sceneName; 
    //进场准备回调
    public OnPrepareCallback onPrepareSceneCallback;
    //离场准备回调
    public OnPrepareCallback onCleanSceneCallback;
    //进UI准备回调
    public OnPrepareCallback onPrepareUICallback;
    //离UI准备回调
    public OnPrepareCallback onCleanUICallback;

}

public class UISwitchManager : MonoBehaviour 
{ 

    public class StateSwitchRequest
    {
        public UIState state;
        public string sceneName;
    }

    enum SwitchState
    {
        Idel = 0,
        PrepareScene,
        PreparingScene,
        PrepareUI,
        PreparingUI
    }

    public UISwitchManager() { }
     

    public void SetNextState(UIState state)
    {
        if( !uiStates.ContainsKey(state) )
        {
            return;
        }

        UIStateDesc desc;
        uiStates.TryGetValue(state,out desc);

        StateSwitchRequest newReq = new StateSwitchRequest();
        newReq.state = state;
        newReq.sceneName = desc.sceneName;

        stateRequestList.Add(newReq);
    }

    public void PopState()
    {
        if( stateRequestStack.Count > 0 )
        {
           var prevReq = stateRequestStack.Pop();
           stateRequestList.Add(prevReq);
        }
    }

    public bool RegisterUIState( UIState state , UIStateDesc desc )
    {
        //如果当前注册表已经注册该状态，则注册失败
        if( uiStates.ContainsKey(state) )
        {
            return false;
        }
        //将UI状态加入注册表
        uiStates.Add(state, desc);
        return true;
    }

    void Awake()
    { 
        UIStateDesc loginUIDesc = new UIStateDesc(UIState.LoginUI, "LoginPanel", "Login", 
            UIPrepareCallback.PrepareLoginScene, UIPrepareCallback.CleanLoginScene, 
            null, null);

        UIStateDesc areaMapUIDesc = new UIStateDesc(UIState.AreaMapUI, "AreaMapPanel", "Map", 
            UIPrepareCallback.PrepareMapScene, UIPrepareCallback.CleanMapScene, 
            UIPrepareCallback.PrepareAreaMapUI, UIPrepareCallback.CleanAreaMapUI);

        UIStateDesc levelDialogUIDesc = new UIStateDesc(UIState.LevelDialogUI, "LevelDialogPanel", "CoreGame",
            UIPrepareCallback.PrepareCoreGameScene, UIPrepareCallback.CleanCoreGameScene, 
            UIPrepareCallback.PrepareLevelDialogUI,UIPrepareCallback.CleanLevelDialogUI); 

        UIStateDesc levelDressUIDesc = new UIStateDesc(UIState.LevelDressUI, "DressPanel", "CoreGame",
            UIPrepareCallback.PrepareCoreGameScene, UIPrepareCallback.CleanCoreGameScene, 
            UIPrepareCallback.PrepareDressUI, UIPrepareCallback.CleanDressUI);

        UIStateDesc ratingUIDesc = new UIStateDesc(UIState.RatingUI, "RatingPanel", "CoreGame", 
            UIPrepareCallback.PrepareCoreGameScene, UIPrepareCallback.CleanCoreGameScene, 
            UIPrepareCallback.PrepareRatingUI,UIPrepareCallback.CleanRatingUI );

        RegisterUIState(UIState.LoginUI,loginUIDesc);
        RegisterUIState(UIState.AreaMapUI, areaMapUIDesc);
        RegisterUIState(UIState.LevelDialogUI, levelDialogUIDesc);
        RegisterUIState(UIState.LevelDressUI, levelDressUIDesc);
        RegisterUIState(UIState.RatingUI, ratingUIDesc);

        SetNextState(UIState.LoginUI);
    }

    void Update()
    {
        switch (switchState)
        {
            case SwitchState.Idel:
                _OnSwitchStateIdel();
                break;
            case SwitchState.PrepareScene:
                _OnSwitchStatePrepareScene();
                break;
            case SwitchState.PrepareUI:
                _OnSwitchStatePrepareUI();
                break;
            case SwitchState.PreparingScene:
            case SwitchState.PreparingUI:
            default:
                break;
        }
    }


    void _OnSwitchStateIdel()
    {
        if (stateRequestList.Count == 0)
            return;
        
        while( stateRequestList.Count > 0 )
        {
            StateSwitchRequest req = stateRequestList[0];
            if( _IsNeedSwitch(req) )
            {
                break;
            }
            //移除无需转变的状态请求
            stateRequestList.RemoveAt(0);
        }
        
        if( stateRequestList.Count > 0 )
        {
            switchState = SwitchState.PrepareScene;
        } 
    }

    void _OnSwitchStatePrepareScene()
    {
        UIStateDesc desc;
        uiStates.TryGetValue(stateRequestList[0].state, out desc);

        if( stateRequestList[0].sceneName.Equals(currScene) )
        {//没必要切换场景或没有准备场景callback直接跳转PrepareUI
            switchState = SwitchState.PrepareUI;
            return;
        }

        _ShowSceneLoadingUI(true); 
        isLoadingUIDisplayed = true;
        isNeedSwitchScene = true;

        if (desc.onPrepareSceneCallback == null)
        {
            _PrepareSceneFinished();
            switchState = SwitchState.PrepareUI;
            return;
        }

        GlobalObjects.GetInstance().GetCoroutineManager().StartCoroutine(desc.onPrepareSceneCallback(_OnProgressChange, _OnPrepareFinished));  
        switchState = SwitchState.PreparingScene;
    }

 

    void _OnSwitchStatePrepareUI()
    {
        UIStateDesc desc;
        uiStates.TryGetValue(stateRequestList[0].state, out desc); 

        if( !isLoadingUIDisplayed )
        {
            _ShowUILoadingUI(true);
            isLoadingUIDisplayed = true;
        }

        if( desc.onPrepareUICallback == null )
        {
            switchState = SwitchState.PreparingUI;
            _OnPrepareFinished();
            return;
        }

        GlobalObjects.GetInstance().GetCoroutineManager().StartCoroutine(desc.onPrepareUICallback(_OnProgressChange, _OnPrepareFinished));
        switchState = SwitchState.PreparingUI;
    }
     

    IEnumerator _FakePrepare()
    {
        yield return new WaitForSeconds(2.0f);
        yield return 1;
        Debug.Log("Step One");
        yield return 1;
        Debug.Log("Step Two");
    }

    public void _OnProgressChange(int count, int total)
    {
        Debug.Log(switchState.ToString()+": count="+count+" total="+total);
    }

    public  void _OnPrepareFinished()
    {
        switch( switchState)
        {
            case SwitchState.PreparingScene:
                _PrepareSceneFinished();
                switchState = SwitchState.PrepareUI;
                break;
            case SwitchState.PreparingUI:
                _AllPrepareFinished();
                break;
            default:
                break;
        }
    }

    public void _PrepareSceneFinished()
    {
        UIStateDesc srcStateDesc;
        uiStates.TryGetValue(currState, out srcStateDesc);
        UIStateDesc dstStateDesc;
        uiStates.TryGetValue(stateRequestList[0].state, out dstStateDesc);

        if (isNeedSwitchScene)
        {
            //调用当前状态的离场回调
            if (srcStateDesc != null && srcStateDesc.onCleanSceneCallback != null)
            {
                var cleanSceneEnumator = srcStateDesc.onCleanSceneCallback(null, null);
                while (cleanSceneEnumator.MoveNext()) { }
            }
            Application.LoadLevel(dstStateDesc.sceneName);
            Resources.UnloadUnusedAssets();
        }
    }

    public void _AllPrepareFinished()
    { 
        UIStateDesc srcStateDesc;
        uiStates.TryGetValue(currState, out srcStateDesc);
        UIStateDesc dstStateDesc;
        uiStates.TryGetValue(stateRequestList[0].state, out dstStateDesc);



        if (!isNeedSwitchScene)
        {//若场景没有切换
            UIController srcUI = _GetOrCreateUI(currState);
            if (srcUI != null)
            {
                srcUI.OnLeaveUI();
            }
        }

        UIController dstUI = _GetOrCreateUI(stateRequestList[0].state); 
        if( dstUI != null )
        {
            dstUI.OnEnterUI();
        }

        if (isNeedSwitchScene)
        {//若在切换状态中出现切场动作，则需要清空请求栈
         //因为在切换场景后状态无法返回
            stateRequestStack.Clear();
        }
        else
        {//保存当前状态
            StateSwitchRequest req = new StateSwitchRequest();
            req.state = currState;
            req.sceneName = currScene;  
            stateRequestStack.Push(req);
        } 
        stateRequestList.RemoveAt(0);

        isNeedSwitchScene = false; 

        currState = dstStateDesc.state;
        currScene = dstStateDesc.sceneName;

        switchState = SwitchState.Idel;
        isLoadingUIDisplayed = false;
              
        //关闭所有Loading UI
        _ShowSceneLoadingUI(false);
        _ShowUILoadingUI(false);  
    }

    Transform _GetBindUITarget()
    {
        UIPanelBindingTarget uiRoot = GameObject.FindObjectOfType<UIPanelBindingTarget>();
       if( uiRoot != null )
       {
           return uiRoot.transform;
       }
       return null;
    }

    void _ShowSceneLoadingUI( bool visable )
    {
        if (sceneLoadingUI == null)
            return;

        if( sceneLoadingUI.gameObject.activeInHierarchy != visable )
        {
            sceneLoadingUI.gameObject.SetActive(visable);
        }
    }

    void _ShowUILoadingUI( bool visable )
    {
        if (uiLoadingUI == null)
            return;

        if (uiLoadingUI.gameObject.activeInHierarchy != visable)
        {
            uiLoadingUI.gameObject.SetActive(visable);
        }
    }

    bool _IsNeedSwitch( StateSwitchRequest req )
    {
        if( req.sceneName.Equals(currScene) && currState == req.state )
        {
            return false;
        }
        return true;
    }

    UIController _GetOrCreateUI( UIState state )
    {
        UIStateDesc desc;
        if( !uiStates.TryGetValue(state, out desc) )
        {
            return null;
        }
         
        GameObject uiObj = GameObject.Find(desc.uiPrefabName);

        if (uiObj != null)
        {
            return uiObj.GetComponent<UIController>();
        }

        uiObj = ResourceManager.GetInstance().Load(ResourceType.UI, desc.uiPrefabName) as GameObject;

        GameObject uiInst = GameObject.Instantiate(uiObj) as GameObject; 
        if( uiInst != null )
        {
            uiInst.name = desc.uiPrefabName;
            Transform uiTrans = uiInst.transform;
            uiTrans.parent = _GetBindUITarget();
            uiTrans.localPosition = Vector3.zero;
            uiTrans.localRotation = Quaternion.identity;
            uiTrans.localScale = Vector3.one;

            return uiInst.GetComponent<UIController>();
        }
        return null;
    }

    //当前场景
    string currScene = "MainEntry";
    //当前UI状态
    UIState currState = UIState.None;
    
    //当前转换状态
    SwitchState switchState = SwitchState.Idel;

    //当前转换请求是否需要切换场景
    bool isNeedSwitchScene = false;
    //是否已经显示读取界面
    bool isLoadingUIDisplayed = false;

    //场景读取UI
    public Transform sceneLoadingUI;

    //UI读取界面
    public Transform uiLoadingUI;

    //状态转换请求队列
    List<StateSwitchRequest> stateRequestList = new List<StateSwitchRequest>();

    //状态切换请求栈
    Stack<StateSwitchRequest> stateRequestStack = new Stack<StateSwitchRequest>();

    //UI状态注册表
    Dictionary<UIState, UIStateDesc> uiStates = new Dictionary<UIState, UIStateDesc>();
}
