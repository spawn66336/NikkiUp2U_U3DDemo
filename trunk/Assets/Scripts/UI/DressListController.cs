using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DressListController : UIController 
{

    UIGrid dressListGrid;

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
	}
	 
	void Update () 
    {
	    
	}

    public void AddDress(  Dress dress , bool used, UIEventListener.VoidDelegate onClick )
    {
       UnityEngine.Object prefab = ResourceManager.GetInstance().Load(ResourceType.UI ,"BagItemBtn" );
       GameObject go = GameObject.Instantiate(prefab) as GameObject;

       dressListGrid.AddChild(go.transform);

       BagItemBtnController bagItemCtrl = go.GetComponent<BagItemBtnController>();

       int indx = dressListGrid.GetChildList().size + 1;

       bagItemCtrl.SetItemID(indx);
       bagItemCtrl.SetDress(dress);
       bagItemCtrl.SetItemIcon(dress.Icon);
       bagItemCtrl.SetItemName(dress.Name);
       bagItemCtrl.SetUsed(used);

       UIEventListener.Get(bagItemCtrl.gameObject).onClick += onClick;

       dressBtnList.Add(bagItemCtrl);
    }

    public void Clear()
    {
        dressBtnList.Clear();

        var btnList = dressListGrid.GetChildList();
        foreach( var btn in btnList )
        {
            GameObject.Destroy(btn.gameObject);
        }
        btnList.Clear();
        dressListGrid.Reposition();
    }
}
