using UnityEngine;
using System.Collections;

public class BagItemBtnController : MonoBehaviour 
{
    
    public UILabel ItemIDLabel;
    public UILabel ItemName;
    public UITexture ItemIcon;
    public UISprite ItemUsed;
    public RankController rankCtrl;

    private Dress dress;
     

    public UIEventListener.VoidDelegate onClickCallback = null;
    public UIEventListener.VoidDelegate onDressDescBeginCallback = null;
    public UIEventListener.VoidDelegate onDressDescEndCallback = null;

    //点击阈值
    float clickThresholdInterval = 0.5f;
    //是否鼠标键点下
    bool pressDown = false;
    //鼠标键按下时刻
    int pressBeginTick = 0;
    //是否提示Desc
    bool isShowDressDesc = false;

 
    public void SetUsed( bool b )
    {
        if( ItemUsed.gameObject.activeInHierarchy != b )
        {
            ItemUsed.gameObject.SetActive(b);
        }
    }

    public void SetItemIcon( Texture2D icon )
    {
        ItemIcon.mainTexture = icon;
        ItemIcon.MakePixelPerfect();
    }

    public void SetItemName( string name )
    {
        ItemName.text = name;
        ItemName.MakePixelPerfect();
    }

    public void SetItemID( int id )
    {
        ItemIDLabel.text = id.ToString();
        ItemIDLabel.MakePixelPerfect();
    }

    public void SetDress( Dress d )
    {
        dress = d;
    }

    public Dress GetDress()
    {
        return dress;
    }

    public void SetRank( int rank )
    {
        rankCtrl.SetRank(rank);
    }

    void Update()
    {
        if( pressDown )
        {
            int currTick = System.Environment.TickCount;
            int elapseTick = currTick - pressBeginTick;
            float elapseSec = ((float)elapseTick) * 0.001f;

            if( elapseSec >= clickThresholdInterval )
            {
                if ( !isShowDressDesc && onDressDescBeginCallback != null)
                {
                    isShowDressDesc = true;
                    onDressDescBeginCallback(this.gameObject);
                }
            }
        }
    }

    void OnClick()
    { 
        if (onClickCallback != null)
        {
             onClickCallback(this.gameObject);
        } 
    }

    void OnPress(bool isPressed)
    {
        if( isPressed )
        {
            pressDown = true;
            pressBeginTick = System.Environment.TickCount;
        }
        else
        {
            int currTick = System.Environment.TickCount;
            int elapseTick = currTick - pressBeginTick;
            float elapseSec = ((float)elapseTick) * 0.001f;

            if (elapseSec >= clickThresholdInterval )
            { 
                if ( isShowDressDesc && onDressDescEndCallback != null)
                {
                    onDressDescEndCallback(this.gameObject);
                }
            }

            isShowDressDesc = false;
            pressDown = false; 
            pressBeginTick = 0; 
        } 
    }
}
