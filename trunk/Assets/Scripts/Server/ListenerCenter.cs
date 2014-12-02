using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class ListenerCenter
{
    static ListenerCenter _instance;
    public static ListenerCenter getInstance()
    {
        if (_instance == null)
        {
            _instance = new ListenerCenter();
        }
        return _instance;
    }

    public void init()
    {
        new LevelCollectClothListener();
        new LevelCollectTypeClothListener();
        new LevelLevelInGradeListener();
    }
}

