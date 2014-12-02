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
    List<Transform> buttonTrans = new List<Transform>();
    public string[] levelSpriteName;
    GameObject playerObj;
    Transform endTrans;
    public UILabel levelNameLabel;

    public GameObject beginButton;
    public GameObject nextButton;
    void OnEnable()
    {
        levelNowIndex = PlayerUIResource.GetInstance().CurrAreaMapIndex;
        maxSubLevel = PlayerUIResource.GetInstance().CurrAreaMapLevelUIInfos.Count;
        Init(levelNowIndex);
        playerObj = GameObject.FindGameObjectWithTag("Player");
        playerObj.transform.parent = buttonTrans[PlayerUIResource.GetInstance().CurrLevelIndex];
        playerObj.transform.localPosition = Vector3.zero;
        UIEventListener.Get(nextButton).onClick += ChangeToNextLevel;
    }
    void OnDisable()
    {
        DisAll();
        UIEventListener.Get(nextButton).onClick -= ChangeToNextLevel;
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
                    if(childTrans[j].gameObject.GetComponent<UIEventListener>()==null)
                        childTrans[j].gameObject.AddComponent<UIEventListener>();
                    if (!buttonTrans.Contains(childTrans[j]))
                        buttonTrans.Add(childTrans[j]);
                    UIEventListener.Get(childTrans[j].gameObject).onClick += ButtonReactEvent;
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
        levelNameLabel.text = PlayerUIResource.GetInstance().CurrAreaMapLevelUIInfos[levelNameIndex].levelInfo.name;
       /* for (int i = 0; i < buttonTrans.Count; ++i)
        {
            if (i <= maxSubLevel)
            {
                buttonTrans[i].gameObject.SetActive(true);
            }
            else
            {
                buttonTrans[i].gameObject.SetActive(false);
            }

        }*/
        Debug.Log(buttonTrans.Count + "  111111111111111111111111  " + PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable.Count);
        for (int i = 0; i < buttonTrans.Count; ++i)
        {
            if (i < PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable.Count)
            {
                if (PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[i].state == LevelState.Invisible)
                {
                    buttonTrans[i].gameObject.SetActive(false);
                }
                else
                {
                    buttonTrans[i].gameObject.SetActive(true);
                    if (PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[i].state == LevelState.Finished)
                    {
                        buttonTrans[i].GetComponent<UISprite>().spriteName = levelSpriteName[(int)PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[i].highestRank];
                        buttonTrans[i].GetComponent<UIButton>().normalSprite = levelSpriteName[(int)PlayerUIResource.GetInstance().CurrAreaMapLevelRecordTable[i].highestRank];

                    }
                }
            }
            else
            {
                buttonTrans[i].gameObject.SetActive(false);
            }
        }
      
    }
    int subLevelIndex;//配置关卡时候，注意名字一定要正确，名字为对应关卡

    Transform[] movePahts;
    int newTransIndex;
    void ButtonReactEvent(GameObject subLevelButton)
    {
       
        subLevelIndex =int.Parse(subLevelButton.name);
        LoadSubLevel(subLevelIndex);
        int tempLength;
        if (subLevelIndex < playerNowSubLevelIndex)
        {
            tempLength = playerNowSubLevelIndex - subLevelIndex  +1;
            movePahts = new Transform[tempLength];
            newTransIndex = 0;
            for (int i = playerNowSubLevelIndex; i >= subLevelIndex; --i)
            {
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
            tempLength=subLevelIndex - playerNowSubLevelIndex+1;
            movePahts = new Transform[tempLength];
            newTransIndex = 0;
            for (int i = playerNowSubLevelIndex; i <= subLevelIndex; ++i)
            {
                movePahts[newTransIndex] = buttonTrans[i];
                ++newTransIndex;
            }
            endTrans = buttonTrans[subLevelIndex];
            StartPlayerMove(movePahts);
            playerNowSubLevelIndex = subLevelIndex;
            UILocker.GetInstance().Lock(gameObject);
        }
        PlayerUIResource.GetInstance().CurrLevelIndex = subLevelIndex;
        levelNameLabel.text = PlayerUIResource.GetInstance().CurrAreaMapLevelUIInfos[PlayerUIResource.GetInstance().CurrLevelIndex].levelInfo.name;
        //PlayerUIResource.GetInstance().CurrLevelId = subLevelIndex + 1;
        //Debug.Log("****************************************  "+playerNowSubLevelIndex);
        if (playerNowSubLevelIndex == maxSubLevel)
        {
            beginButton.SetActive(false);
            nextButton.SetActive(true);
        }
        else
        {
            beginButton.SetActive(true);
            nextButton.SetActive(false);
        }
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
    }
    void LoadSubLevel(int sublevelIndex)
    {
        //此处与服务器对接用于，加载此关任务
//        Debug.Log("sub level is " + sublevelIndex);
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

