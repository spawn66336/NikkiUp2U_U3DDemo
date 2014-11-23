using UnityEngine;
using System.Collections;

public class DressPanelController : UIController 
{
    //换装角色
    public Transform Nikki;
    //右侧换装栏
    public Transform DressBar;
    //右侧边条
    public Transform RightSideBar;

    
    public void OnBackBtnClick()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.AreaMapUI);
    }

    public void OnFinishDressBtnClick()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.RatingUI);
    }

    public void  OnHairDressTypeBtnClick()
    {
        Debug.Log("头发");
    }
    
    public void OnDressDressTypeBtnClick()
    {
        Debug.Log("连衣裙");
    }

    public void OnCoatDressTypeBtnClick()
    {
        Debug.Log("外套");
    }

    public void OnTopsDressTypeBtnClick()
    {
        Debug.Log("上衣");
    }

    public void OnBottomsDressTypeBtnClick()
    {
        Debug.Log("下装");
    }

    public void OnSockDressTypeBtnClick()
    {
        Debug.Log("袜子");
    }

    public void OnShoesDressTypeBtnClick()
    {
        Debug.Log("鞋子");
    }


    public void OnAccDressTypeBtnClick()
    {
        Debug.Log("饰品");
    }

    //开始换装按钮
    public void OnBeginDressBtnClick()
    {
        Debug.Log("开始换装!");
    }

    //配置按钮
    public void OnOptionBtnClick()
    {
        Debug.Log("配置");
    }
}
