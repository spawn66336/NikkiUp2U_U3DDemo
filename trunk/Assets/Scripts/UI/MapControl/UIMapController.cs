using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMapController : MonoBehaviour
{
    public GameObject[] levels;
    UIEventListener[] levelButtonsLisenEvents;
    [System.NonSerialized]
    public GameObject levelNow;
    Transform[] levelUIMapTrans;
    [System.NonSerialized]
    public int levelNowIndex=0;//外部调用此接口，用于生成地图
    [System.NonSerialized]
    public int playerNowSubLevelIndex = 0;//外部获得,player当前所在的关卡
    [System.NonSerialized]
    public int maxSubLevel = 10;//已经完成的最大关卡，也就是下一个需要去完成的关卡
    public List<Transform> buttonTrans = new List<Transform>();
    public string[] levelSpriteName;
    GameObject playerObj;
    Transform endTrans;
    public UILabel levelNameLabel;

    public GameObject beginButton;
    public GameObject nextButton;
    public GameObject upLevelButton;
    public GameObject lockObj;


    int levelBeginIndex=0;
    bool nextMapIsTrue;
    bool upMapIsTrue;
    void OnEnable()
    {
        levelNowIndex = PlayerUIResource.GetInstance().CurrAreaMapIndex;
        maxSubLevel = PlayerUIResource.GetInstance().CurrAreaMapLevelUIInfos.Count; 
        Init(levelNowIndex);
        playerObj = GameObject.FindGameObjectWithTag("Player");
        playerObj.transform.parent = buttonTrans[PlayerUIResource.GetInstance().CurrLevelIndex+levelBeginIndex];
        playerNowSubLevelIndex = PlayerUIResource.GetInstance().CurrLevelIndex+levelBeginIndex;
//        Debug.Log("player now  index+++++++++++++++++++++++++++++++  " + playerNowSubLevelIndex);
        playerObj.transform.localPosition = Vector3.zero;
        UIEventListener.Get(nextButton).onClick += ChangeToNextLevel;
        UIEventListener.Get(lockOkButton).onClick += LockButtonEvent;
        lockInfolabel.gameObject.SetActive(false);
    }
    void OnDisable()
    {

        DisAll();
        buttonTrans.Clear();
        UIEventListener.Get(nextButton).onClick -= ChangeToNextLevel;
        UIEventListener.Get(lockOkButton).onClick -= LockButtonEvent;

    }

    void DisAll()
    {
        Destroy(levelNow);// or active false 看资源管理如何做这块了
        for (int i = 0; i < buttonTrans.Count; ++i)
        {
            UIEventListener.Get(buttonTrans[i].gameObject).onClick -= ButtonReactEvent;
        }
    }
    public void Init(int levelNameIndex)
    {
        levelNow = Object.Instantiate(levels[levelNameIndex], Vector3.zero, Quaternion.identity) as GameObject;//创建此关位置，看资源管理
        buttonTrans.Clear();
        levelNow.transform.parent = transform;
        levelNow.transform.localScale = Vector3.one*2;
        levelNow.name = levelNowIndex.ToString();
        levelUIMapTrans = new Transform[levelNow.transform.childCount];
        for (int i = 0; i < levelNow.transform.childCount; ++i)
        {
            levelUIMapTrans[i] = levelNow.transform.GetChild(i);
            Transform[] childTrans = new Transform[levelUIMapTrans[i].childCount];
            if (childTrans.Length > 0)
            {
                for (int j = 0; j < childTrans.Length; ++j)
                {
                    childTrans[j] = levelUIMapTrans[i].GetChild(j);                   
                    if (!buttonTrans.Contains(childTrans[j]))
                        buttonTrans.Add(childTrans[j]);                   
                }
            }
        }
        Transform tempTrans;
        int countLengthTemp = buttonTrans.Count;
        for (int i = 0; i < (countLengthTemp - 1); ++i)
        {
            for (int j = (i+1); j < countLengthTemp; ++j)
            {
                if (int.Parse(buttonTrans[i].name) > int.Parse(buttonTrans[j].name))
                {
                    tempTrans = buttonTrans[i];
                    buttonTrans[i] = buttonTrans[j];
                    buttonTrans[j] = tempTrans;
                }
            }
        }
        levelNameLabel.text = PlayerUIResource.GetInstance().CurrLevelAreaMapTitle;
        
        if (PlayerUIResource.GetInstance().CurrAreaMapIndex == 0)
        {
            buttonTrans[0].gameObject.SetActive(false);
            buttonTrans.Remove(buttonTrans[0]);
            levelBeginIndex = 0;
            upMapIsTrue = false;
        }
        else 
        {
            buttonTrans[0].gameObject.SetActive(true);    
            /*if(buttonTrans[0].GetComponent<UIEventListener>()==null)
                buttonTrans[0].gameObject.AddComponent<UIEventListener>();
            UIEventListener.Get(buttonTrans[0].gameObject).onClick += BackToUpLevel;*/
            if (upLevelButton.GetComponent<UIEventListener>())
                upLevelButton.AddComponent<UIEventListener>();
            UIEventListener.Get(upLevelButton).onClick += BackToUpLevel;
            levelBeginIndex = 1;
            upMapIsTrue = true;
        }
        if (PlayerUIResource.GetInstance().CurrAreaMapIndex == (PlayerUIResource.GetInstance().AreaMapCount - 1))
        {
            buttonTrans[(buttonTrans.Count - 1)].gameObject.SetActive(false);
            buttonTrans.Remove(buttonTrans[(buttonTrans.Count - 1)]);
            nextMapIsTrue = false;
        }
        else
        {
            nextMapIsTrue = true;
        }
        for (int m = 0; m < buttonTrans.Count; ++m)
        {
            buttonTrans[m].name = m.ToString();
            if (buttonTrans[m].gameObject.GetComponent<UIEventListener>() == null)
                buttonTrans[m].gameObject.AddComponent<UIEventListener>();
            UIEventListener.Get(buttonTrans[m].gameObject).onClick += ButtonReactEvent;
        }
        int begincountIndex;
        for (int m = levelBeginIndex; m < buttonTrans.Count; ++m)
        {
            begincountIndex =m-levelBeginIndex;
            if (begincountIndex < PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable.Count)
            {
//                Debug.Log(" level begin now1111111111111111111111111   " + begincountIndex + "  state  " + PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[begincountIndex].state);
                if (PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[begincountIndex].state == LevelState.Invisible)
                {
                    buttonTrans[m].gameObject.SetActive(false);
                }
                else
                {
                    buttonTrans[m].gameObject.SetActive(true);
                    if (PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[begincountIndex].state == LevelState.Finished)
                    {
                        buttonTrans[m].GetComponent<UISprite>().spriteName = levelSpriteName[(int)PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[begincountIndex].highestRank];
                        buttonTrans[m].GetComponent<UIButton>().normalSprite = levelSpriteName[(int)PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[begincountIndex].highestRank];
                    }
                    else if (PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[begincountIndex].state == LevelState.Locked)
                    {
                       lockObjNow= Object.Instantiate(lockObj, Vector3.zero, Quaternion.identity) as GameObject;
                       lockObjNow.transform.parent = buttonTrans[m].transform;
                       lockObjNow.transform.localPosition = Vector3.zero;
                    }
                }
               
            }
            else
            {
                if (begincountIndex == (buttonTrans.Count - 1) && begincountIndex == PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable.Count)
                {
                    if (PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable.Count - 1].state == LevelState.Finished)
                    {
                        buttonTrans[m].gameObject.SetActive(true);
                    }
                    else
                    {
                        buttonTrans[m].gameObject.SetActive(false);
                    }
                }
                else
                {
                    buttonTrans[m].gameObject.SetActive(false);
                }
            }
          
           
        }

      
    }

    private void BackToUpLevel(GameObject go)
    {
        buttonTrans.Clear();
        PlayerUIResource.GetInstance().CurrAreaMapIndex--;
        DisAll();
        Init(PlayerUIResource.GetInstance().CurrAreaMapIndex);
        //PlayerUIResource.GetInstance().CurrLevelIndex = 0;

        PlayerUIResource.GetInstance().CurrLevelIndex = buttonTrans.Count - 1;//1-5 当是0时候开始，最后5时候结束 
        playerNowSubLevelIndex = buttonTrans.Count-1;
        Debug.Log("PlayerUIResource.GetInstance().CurrLevelIndex  " + PlayerUIResource.GetInstance().CurrLevelIndex);
        playerObj.transform.parent = buttonTrans[playerNowSubLevelIndex];
        playerObj.transform.localPosition = Vector3.zero;
        nextButton.SetActive(true);
        beginButton.SetActive(false);
        upLevelButton.SetActive(false);
        levelNameLabel.text = "";
        
    }
    GameObject lockObjNow;
    int subLevelIndex;//配置关卡时候，注意名字一定要正确，名字为对应关卡

    public Transform[] movePahts;
    int newTransIndex;
    public UILabel lockInfolabel;
    public GameObject lockOkButton;
    bool normalLevelButton;
    int lockNumJudege;
    void ButtonReactEvent(GameObject subLevelButton)
    {
        subLevelIndex = int.Parse(subLevelButton.name);
        normalLevelButton = true;
        beginButton.SetActive(true);
        nextButton.SetActive(false);
        upLevelButton.SetActive(false);      
        
        PlayerUIResource.GetInstance().CurrLevelIndex = subLevelIndex - levelBeginIndex;
        levelNameLabel.text = PlayerUIResource.GetInstance().CurrLevelAreaMapTitle;        
       
        //证明不是开始和结尾
        if(nextMapIsTrue)
        {
            if (subLevelIndex == (buttonTrans.Count - 1))
            {
                normalLevelButton = false;
                upLevelButton.SetActive(false);
                nextButton.SetActive(true);
                beginButton.SetActive(false);
                levelNameLabel.text = "";
                PlayerUIResource.GetInstance().CurrLevelIndex = (buttonTrans.Count - 1);
                levelNameLabel.text = "";
            }
        }
        if (upMapIsTrue)
        {
            if (subLevelIndex == 0)
            {
                normalLevelButton = false;
                upLevelButton.SetActive(true);
                nextButton.SetActive(false);
                beginButton.SetActive(false);
                levelNameLabel.text = "";
                PlayerUIResource.GetInstance().CurrLevelIndex = 0;
                levelNameLabel.text = "";
               
            }
        }
        if (normalLevelButton)
        {
            lockNumJudege=subLevelIndex-levelBeginIndex;
            if (PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[lockNumJudege].state == LevelState.Locked)
            {
                lockInfolabel.text = PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[lockNumJudege].lockReason;
                lockInfolabel.gameObject.SetActive(true);
                return;
            }
        }

        //Debug.Log("playerNowSubLevelIndex  " + playerNowSubLevelIndex + "  subLevelIndex  " + subLevelIndex);
        int tempLength;
        if (subLevelIndex < playerNowSubLevelIndex)
        {
            tempLength = playerNowSubLevelIndex  - subLevelIndex + 1;
            movePahts = new Transform[tempLength];
            newTransIndex = 0;
            for (int i = playerNowSubLevelIndex; i >= subLevelIndex; --i)
            {
                Debug.Log(newTransIndex + "  000000000000000  " + i + "  " + subLevelIndex);
                movePahts[newTransIndex] = buttonTrans[i];
                ++newTransIndex;
            }
            endTrans = buttonTrans[subLevelIndex];
            StartPlayerMove(movePahts);
            playerNowSubLevelIndex = subLevelIndex;
            UILocker.GetInstance().Lock(gameObject);
        }
        else if (subLevelIndex > playerNowSubLevelIndex)
        {
            tempLength = subLevelIndex - (playerNowSubLevelIndex)+ 1;
            movePahts = new Transform[tempLength];
            newTransIndex = 0;           
            for (int i = (playerNowSubLevelIndex); i <= subLevelIndex; ++i) 
            {
                Debug.Log(newTransIndex + "  111111111111  " + i + "  " + subLevelIndex);
                movePahts[newTransIndex] = buttonTrans[i];
                ++newTransIndex;
            }
            endTrans = buttonTrans[subLevelIndex];
            StartPlayerMove(movePahts);
            playerNowSubLevelIndex = subLevelIndex;
            UILocker.GetInstance().Lock(gameObject);
        }
      // +" " + PlayerUIResource.GetInstance().CurrAreaMapLevelUIInfos[PlayerUIResource.GetInstance().CurrLevelIndex].levelInfo.name;
        //PlayerUIResource.GetInstance().CurrLevelId = subLevelIndex + 1;
        //Debug.Log("****************************************  "+playerNowSubLevelIndex);
      

    }

    void LockButtonEvent(GameObject button)
    {
        lockInfolabel.gameObject.SetActive(false);
    }
    public void ChangeToNextLevel(GameObject button)
    {
        buttonTrans.Clear();
        PlayerUIResource.GetInstance().CurrAreaMapIndex++;
        PlayerUIResource.GetInstance().CurrLevelIndex = 0;
        DisAll();
        Init(PlayerUIResource.GetInstance().CurrAreaMapIndex);
        //PlayerUIResource.GetInstance().CurrLevelIndex = 0;
        playerObj.transform.parent = buttonTrans[PlayerUIResource.GetInstance().CurrLevelIndex];
        playerObj.transform.localPosition = Vector3.zero;
        playerNowSubLevelIndex = PlayerUIResource.GetInstance().CurrLevelIndex;
        nextButton.SetActive(false);
        beginButton.SetActive(false);
        upLevelButton.SetActive(true);
        levelNameLabel.text = "";

    }
  
    Hashtable args;
    void StartPlayerMove(Transform[] paths)
    {
        args = new Hashtable();
        args.Add("path", paths);
        args.Add("easeType", iTween.EaseType.linear);
        args.Add("speed", 5f);
        args.Add("movetopath", true);
        args.Add("oncomplete","CompleteMove");
        args.Add("oncompletetarget",gameObject);
        iTween.MoveTo(playerObj, args);
    }
    void CompleteMove()
    {
        // complete event
        playerObj.transform.parent = endTrans;
        UILocker.GetInstance().UnLock(gameObject);
    }
}

