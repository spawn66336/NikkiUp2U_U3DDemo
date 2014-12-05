using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LogicMain : MonoBehaviour 
{

    void Start()
    {
        Init();
    }

    //获取模块
    public T GetModule<T>() where T:GameLogicModule
    {
        foreach( var m in modules )
        {
            if( m.GetType() == typeof(T))
            {
                return m as T;
            }
        }
        return null;
    }

    void Init()
    {
        //初始化假服务器
        FakeServer.GetInstance().Init();

        //注册假服务器发送端口回调
        RequestCenter.GetInstance().fakeServerSendPort = FakeServer.GetInstance().RecvClientRequest;
        
        modules.Add(new AreaMapModule());
        modules.Add(new BagModule());
        modules.Add(new GameItemDatabaseModule());
        modules.Add(new LevelModule());
        modules.Add(new PlayerModule());
        modules.Add(new RatingSystemModule());

        PlayerUIResource.GetInstance().Init();
        LevelUIResourceManager.GetInstance().Init();
        GameItemUIResourceManager.GetInstance().Init();
        AreaMapUIResourceManager.GetInstance().Init();

        foreach( var m in modules )
        {
            m.Init();
        } 
        
    }

    void Update()
    {
        RequestCenter.GetInstance().Update();

        foreach( var m in modules )
        {
            m.Update();
        }

        PlayerRecordManager.getInstance().updateMapTime();
    }

    void OnDestroy()
    {
        FakeServer.GetInstance().Destroy();
    }

    //模块列表
    List<GameLogicModule> modules = new List<GameLogicModule>();
}
