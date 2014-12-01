using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DressListController : UIController 
{

    UIGrid dressListGrid;

    public UIScrollBar dressListScrollbar;

    //列表中的衣服按钮
    public List<BagItemBtnController> dressBtnList = new List<BagItemBtnController>();


    public override void OnEnterUI()
    {
        base.OnEnterUI(); 

    }

    public override void OnLeaveUI()
    {
        base.OnLeaveUI(); 
    }

	void Start () 
    {
        dressListGrid = GetComponentInChildren<UIGrid>();
        dressListGrid.onCustomSort = _DressBtnListCompare; 
        dressListGrid.sorting = UIGrid.Sorting.Custom;
	}
	 
	void Update () 
    {
	    
	}

    public void AddDress(  Dress dress , bool used, 
            UIEventListener.VoidDelegate onClick , 
            UIEventListener.VoidDelegate onDressDescBegin , 
            UIEventListener.VoidDelegate onDressDescEnd 
        )
    {
       UnityEngine.Object prefab = ResourceManager.GetInstance().Load(ResourceType.UI ,"BagItemBtn" );
       GameObject go = GameObject.Instantiate(prefab) as GameObject;
         
       go.transform.parent = dressListGrid.gameObject.transform;
       go.transform.localPosition = Vector3.zero;
       go.transform.localRotation = Quaternion.identity;
       go.transform.localScale = Vector3.one;

       BagItemBtnController bagItemCtrl = go.GetComponent<BagItemBtnController>();
         

       int indx = dressBtnList.Count + 1;
       bagItemCtrl.gameObject.name = indx.ToString();
       bagItemCtrl.SetItemID(indx);
       bagItemCtrl.SetDress(dress);
       bagItemCtrl.SetItemIcon(dress.Icon);
       bagItemCtrl.SetItemName(dress.Name);
       bagItemCtrl.SetUsed(used);
         
       bagItemCtrl.onClickCallback = onClick;
       bagItemCtrl.onDressDescBeginCallback = onDressDescBegin;
       bagItemCtrl.onDressDescEndCallback = onDressDescEnd;

       if (dressListScrollbar != null )
       {
           dressListScrollbar.value = 0f;
       }
         
       dressListGrid.Reposition(); 
       dressBtnList.Add(bagItemCtrl);
    }



    int _DressBtnListCompare( Transform a , Transform b )
    {
        int ai = 0;
        int.TryParse(a.gameObject.name,out ai);
        int bi = 0;
        int.TryParse(b.gameObject.name,out bi);

        return ai - bi;
    }

    public void Clear()
    {
        dressBtnList.Clear(); 

        var btnList = 
        dressListGrid.transform.GetComponentsInChildren<BagItemBtnController>(); 
        foreach( var btn in btnList )
        {
            GameObject.DestroyImmediate(btn.gameObject);
        } 
        dressListGrid.Reposition();
    }
}
