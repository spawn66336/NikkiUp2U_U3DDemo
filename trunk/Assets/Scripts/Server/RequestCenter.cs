using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RequestMessage
{ 
    //消息内容
    public virtual int Message { get { return 0; } }  
}

public class ReplyMessage : RequestMessage
{ 
    //结果对象
    public object resultObject;
}

public enum RequestReplyResult
{
    Error = 0,
    Ok
}

public delegate void RequestReplyCallback( RequestReplyResult result, ReplyMessage reply , object param );

public delegate void FakeServerSendPortCallback(ServerRequestMessage req);

public delegate void FakeServerRecvPortCallback(ServerReplyMessage reply); 

//送往服务器的消息
public class ServerRequestMessage
{
    public int serial;
    public RequestMessage msg;
}

//从服务器接收的消息
public class ServerReplyMessage
{
    public int serial;
    public object resultObject;
}

public class RequestCenter 
{



    public class RequestInfo
    {
        //消息序列号
        public int serial;
        public RequestMessage msg;
        public RequestReplyCallback replyCallback;
        public object param;
    }

    public void SendRequest( RequestMessage msg , RequestReplyCallback callback , object param)
    {
        RequestInfo reqInfo = new RequestInfo();
        reqInfo.serial = _GenSerial();
        reqInfo.msg = msg;
        reqInfo.replyCallback = callback;
        reqInfo.param = param;

        if (_SendRequestToServer(reqInfo))
        {
            requestInfos.Add(reqInfo);
        }
    }

    public void Update()
    {

        foreach( var reply in replyMessageList )
        {
            foreach( var req in requestInfos )
            {
                if (req.serial == reply.serial)
                {
                    ReplyMessage replyMsg = new ReplyMessage();
                    replyMsg.resultObject = reply.resultObject; 

                    //调用回调
                    if( req.replyCallback != null )
                    {
                        req.replyCallback(RequestReplyResult.Ok, replyMsg, req.param);
                    } 
                    tempRequestList.Add(req);
                }
            }

            //清除已有回复的消息
            foreach( var removeReq in tempRequestList )
            {
                requestInfos.Remove(removeReq);
            }

            tempRequestList.Clear();
        }
       
    }

    bool _SendRequestToServer( RequestInfo req )
    {
        if( fakeServerSendPort != null )
        {
            ServerRequestMessage reqMsg = new ServerRequestMessage();
            reqMsg.serial = req.serial;
            reqMsg.msg = req.msg;

            fakeServerSendPort(reqMsg);
            return true;
        }
        return false;
    }

    public void OnRecvFakeServerReply( ServerReplyMessage reply )
    {
        replyMessageList.Add(reply);
    }
    

    public FakeServerSendPortCallback fakeServerSendPort;
    

    List<RequestInfo> requestInfos = new List<RequestInfo>();
    List<ServerReplyMessage> replyMessageList = new List<ServerReplyMessage>();

    //用于防止内存碎片
    List<RequestInfo> tempRequestList = new List<RequestInfo>();

    public static RequestCenter GetInstance() 
    {
        if( s_instance == null )
        {
            s_instance = new RequestCenter();
        }
        return s_instance; 
    }

    static int _GenSerial()
    {
        return serial++;
    }

    static RequestCenter s_instance;
    static int serial = 1;
}
