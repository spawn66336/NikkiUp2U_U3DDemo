using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LogicMain : MonoBehaviour 
{
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
    }

    List<GameLogicModule> modules = new List<GameLogicModule>();
}
