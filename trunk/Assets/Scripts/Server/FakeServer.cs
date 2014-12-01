using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModuleServer
{
    public virtual void Init() { }

    public virtual void Destory() { }

    public void ReplyToClient( ServerReplyMessage reply )
    {
        FakeServer.GetInstance().ReplyToClient(reply);
    }


    //需要继承类实现响应
    public virtual void HandleRequest( ServerRequestMessage request )
    {
        
    }
}

public class FakeServer 
{ 

    public void Init()
    {
		DataManager.GetInstance().Init();

        servers.Add(new AreaMapModuleServer());
        servers.Add(new BagModuleServer());
        servers.Add(new GameItemDBModuleServer());
        servers.Add(new LevelModuleServer());
        servers.Add(new PlayerModuleServer());
        servers.Add(new RatingSysModuleServer());

        foreach( var s in servers )
        {
            s.Init();
        }
    }

    public GameItemDBModuleServer getGameItemDBServer()
    {
        foreach (var s in servers)
        {
            if (s.GetType() == typeof(GameItemDBModuleServer))
            {
                GameItemDBModuleServer gameItemDBServer = (GameItemDBModuleServer)s;
                return gameItemDBServer;
            }
        }
        return null;
    }

    public PlayerModuleServer GetPlayerModuleServer()
    {
        foreach (var s in servers)
        {
            if (s.GetType() == typeof(PlayerModuleServer))
            {
                PlayerModuleServer server = (PlayerModuleServer)s;
                return server;
            }
        }
        return null;
    }

    public void Destroy()
    {
        foreach( var s in servers )
        {
            s.Destory();
        }
        servers.Clear();
    }



    public void ReplyToClient( ServerReplyMessage reply )
    {
        RequestCenter.GetInstance().OnRecvFakeServerReply(reply);
    }

    public void RecvClientRequest( ServerRequestMessage request )
    {
        foreach( var s in servers )
        {
            s.HandleRequest(request);
        }
    }

    List<ModuleServer> servers = new List<ModuleServer>();

    public static FakeServer GetInstance()
    {
        if( s_instance == null )
        {
            s_instance = new FakeServer();
        }
        return s_instance;
    }

    static FakeServer s_instance; 
}
