using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DressPanelController : UIController 
{
    //换装角色
    public Actor Nikki; 
    //关卡名称
    public UILabel LevelNameLabel;
    //关卡对话内容
    public UITextList levelDialogContents;
    //服装列表
    public DressListController dressListCtrl;
    //服装类型列表
    public GameObject dressTypeList;
    
    public GameObject dressGroup;
    //任务提示框
    public GameObject levelDialog;
     
    //选中类型
    DressType chosenType; 
  
    //衣服列表
    List<Dress> dressList = new List<Dress>();

    enum DressPanelState
    {
        DressTypeChoose = 1,
        AccTypeChoose,
        DressChoose,
        AccChoose
    }

    DressPanelState prevState = DressPanelState.DressTypeChoose;
    DressPanelState currState = DressPanelState.DressTypeChoose;
    DressPanelState nextState = DressPanelState.DressTypeChoose;


    public override void OnEnterUI()
    {
        base.OnEnterUI();
        if (PlayerUIResource.GetInstance().CurrLevelUIInfo != null)
        {
            PlayerLevelUIInfo currLevelUIInfo = PlayerUIResource.GetInstance().CurrLevelUIInfo;

            LevelNameLabel.text = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.name;
                        
            var contents = currLevelUIInfo.levelInfo.dialogInfo.contents;
            foreach( var c in contents )
            {
                levelDialogContents.Add(c.content);
            } 

            levelDialogContents.scrollValue = 0f;
        }
        else
        {
            LevelNameLabel.text = "关卡名获取失败！";
        }
         
    }

    public override void OnLeaveUI()
    {
        base.OnLeaveUI();

    }

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
        chosenType = DressType.Hair;
        _SetNextState(DressPanelState.DressChoose);
    }
    
    public void OnDressDressTypeBtnClick()
    {
        chosenType = DressType.Dress;
        _SetNextState(DressPanelState.DressChoose);
    }

    public void OnCoatDressTypeBtnClick()
    {
        chosenType = DressType.Coat;
        _SetNextState(DressPanelState.DressChoose);
    }

    public void OnTopsDressTypeBtnClick()
    {
        chosenType = DressType.Tops;
        _SetNextState(DressPanelState.DressChoose);
    }

    public void OnBottomsDressTypeBtnClick()
    {
        chosenType = DressType.Bottoms;
        _SetNextState(DressPanelState.DressChoose);
    }

    public void OnSockDressTypeBtnClick()
    {
        chosenType = DressType.Socks;
        _SetNextState(DressPanelState.DressChoose);
    }

    public void OnShoesDressTypeBtnClick()
    {
        chosenType = DressType.Shoes;
        _SetNextState(DressPanelState.DressChoose);
    }


    public void OnAccDressTypeBtnClick()
    {
        _SetNextState(DressPanelState.AccTypeChoose);
    }

    public void OnAccHeadTypeBtnClick()
    {
        chosenType = DressType.AccHead;
        _SetNextState(DressPanelState.AccChoose);
    }

    public void OnAccEarTypeBtnClick()
    {
        chosenType = DressType.AccEar;
        _SetNextState(DressPanelState.AccChoose);
    }

    public void OnAccNeckTypeBtnClick()
    {
        chosenType = DressType.AccNeck;
        _SetNextState(DressPanelState.AccChoose);
    }

    public void OnAccHandTypeBtnClick()
    {
        chosenType = DressType.AccHand;
        _SetNextState(DressPanelState.AccChoose);
    }

    public void OnAccWaistTypeBtnClick()
    {
        chosenType = DressType.AccWaist;
        _SetNextState(DressPanelState.AccChoose);
    }

    public void OnAccLegTypeBtnClick()
    {
        chosenType = DressType.AccLeg;
        _SetNextState(DressPanelState.AccChoose);
    }

    public void OnAccSpecialTypeBtnClick()
    {
        chosenType = DressType.AccSpecial;
        _SetNextState(DressPanelState.AccChoose);
    }

    public void OnAccFaceTypeBtnClick()
    {
        chosenType = DressType.AccFace;
        _SetNextState(DressPanelState.AccChoose);
    }


    void _SetNextState( DressPanelState state )
    {
        if( !_IsAllAnimStable() )
        {
            return;
        }
       

        nextState = state;
        _KickStateTransition();
    }

    void _KickStateTransition()
    {
        if (currState == nextState)
            return;

        UILocker.GetInstance().Lock(gameObject);

        switch (nextState)
        {
            case DressPanelState.DressTypeChoose:
                _OnKickDressTypeChooseStateTransition();
                break;
            case DressPanelState.DressChoose:
                _OnKickDressChooseStateTransition();
                break;
            case DressPanelState.AccTypeChoose:
                _OnKickAccTypeChooseStateTransition();
                break;
            case DressPanelState.AccChoose:
                _OnKickAccChooseStateTransition();
                break;

        }
    }

    void _OnKickDressTypeChooseStateTransition()
    {
        if( currState == DressPanelState.DressChoose)
        {
            _PlayAnim(dressListCtrl.gameObject, true);
            _PlayAnim(dressTypeList, false);
        }else if( currState == DressPanelState.AccTypeChoose )
        {
            _PlayAnim(dressGroup, true);
        }
    }

    void _OnKickDressChooseStateTransition()
    {
        if( currState == DressPanelState.DressTypeChoose )
        {
            _PlayAnim(dressTypeList, true);
            _PlayAnim(dressListCtrl.gameObject, false);

            _RebuildDressScrollList();
        } 
    }

    void _OnKickAccTypeChooseStateTransition()
    {
        if( currState == DressPanelState.DressTypeChoose )
        {
            _PlayAnim(dressGroup, false);
        }else if( currState == DressPanelState.AccChoose )
        {
            dressListCtrl.gameObject.SetActive(true);
            _PlayAnim(dressListCtrl.gameObject, true);
            _PlayAnim(dressTypeList, false);
        }
    }

    void _OnKickAccChooseStateTransition()
    {
        if( currState == DressPanelState.AccTypeChoose )
        {
            _PlayAnim(dressTypeList, true);
            _PlayAnim(dressListCtrl.gameObject, false);

            _RebuildDressScrollList();
        }
    }

    bool _IsAllAnimStable()
    {
       return 
           _IsAnimStable(dressTypeList) && 
           _IsAnimStable(dressListCtrl.gameObject) && 
           _IsAnimStable(dressGroup);
    }


    bool _IsAnimStable(GameObject go)
    {
        Animation anim = go.GetComponent<Animation>();
        if (anim == null)
        {
            return true;
        }

        AnimationClip defaultClip = anim.clip;

        if (defaultClip == null)
            return true;

        return anim.isPlaying == false;
    }

    void _PlayAnim(GameObject go, bool reverse)
    {
        Animation anim = go.GetComponent<Animation>();
        if (anim == null)
        {
            return;
        }

        AnimationClip defaultClip = anim.clip;

        if (defaultClip == null)
            return;

        float speed = 1f;
        float time = 0f;
        if (reverse)
        {
            speed = -1f;
            time = 1f;
        }
        anim[defaultClip.name].speed = speed;
        anim[defaultClip.name].normalizedTime = time;
        anim.Play();
    }

    void _UpdateState()
    {
        if( currState != nextState )
        {
            if( _IsAnimStable(dressTypeList) && _IsAnimStable(dressListCtrl.gameObject) && _IsAnimStable(dressGroup))
            {
                currState = nextState;
                UILocker.GetInstance().UnLock(gameObject);
            }
        }
    }

    void _RebuildDressScrollList()
    {
        _UpdateDressList();
        _UpdateDressScrollList();
        _UpdateUsedState();
    }

    //根据选择类型更新衣服列表
    void _UpdateDressList()
    {
        dressList.Clear(); 
        var bagItemUIInfos = PlayerUIResource.GetInstance().BagItemUIInfos;
        foreach( var info in bagItemUIInfos )
        {
            Dress dress = info.item as Dress;
            if( dress != null )
            {
                if( dress.ClothType == chosenType )
                {
                    dressList.Add(dress);
                }
            }
        }
    }

    void _UpdateDressScrollList()
    {
        dressListCtrl.Clear();
        foreach( var dress in dressList )
        {
            dressListCtrl.AddDress(dress, false, _OnDressClick); 
        }
    }

    void _UpdateUsedState()
    {
        foreach( var btn in dressListCtrl.dressBtnList )
        {
            Dress dress = btn.GetDress();
            if( Nikki.GetDressSet().IsUsed(dress) )
            {
                btn.SetUsed(true);
            }
            else
            {
                btn.SetUsed(false);
            }
        }
    }

    void _OnDressClick( GameObject go )
    {
        var ctrl = go.GetComponent<BagItemBtnController>();
        if( ctrl == null )
        {
            Debug.Log("未获取到BagItemBtnController");
            return;
        }

        Dress dress = ctrl.GetDress();
        if (Nikki.GetDressSet().IsUsed(dress))
        {
            Nikki.SetDress(dress.ClothType, null);
        }
        else
        {
            Nikki.SetDress(dress.ClothType, dress);
        }
        _UpdateUsedState();

        var panel = GetComponent<UIPanel>();
        panel.RebuildAllDrawCalls();
    }
 
    void Update()
    {
        _UpdateState();
    }

    //在空白处点击
    public void OnBlankClick()
    {
        if( currState == DressPanelState.DressChoose )
        {
            _SetNextState(DressPanelState.DressTypeChoose);
        }else if( currState == DressPanelState.AccTypeChoose)
        {
            _SetNextState(DressPanelState.DressTypeChoose);
        }else if( currState == DressPanelState.AccChoose )
        {
            _SetNextState(DressPanelState.AccTypeChoose);
        }
    }

    public void OnLevelDialogBtnClick()
    {
        levelDialog.SetActive(true);
    }

    public void OnLevelDialogDisableClick()
    {
        levelDialog.SetActive(false);
    }

    public void OnDressFinishedBtnClick()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.RatingUI);
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
