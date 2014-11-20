using UnityEngine;
using System.Collections;

public class GameItemDatabaseModule : GameLogicModule , IGameItemDatabase
{
    public void GetGameItemInfoById(int id, GetGameItemInfoByIdCallback callback) { }

    public void GetGameItemsByType(GameItemType type, GetGameItemInfosCallback callback) { }
}
