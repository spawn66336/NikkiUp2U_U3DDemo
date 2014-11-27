using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIResourceSyncRequest
{ 
    public UIResourceSyncRequest( int resId , string n )
    {
        id = resId;
        name = n;
    }

    public int ResID { get { return id; } set { id = value; } }

    public string Name { get { return name; } set { name = value; } }

    int id;
    string name;
}

public class UIResourceManager
{

    public virtual void Init()
    {

    }

    public void PostRequest( UIResourceSyncRequest request )
    {
        requestList.Add(request);
    }

    public virtual IEnumerator Sync()
    {
        yield return 0;
    }

    public virtual void Clear()
    {

    }

    protected List<UIResourceSyncRequest> requestList = new List<UIResourceSyncRequest>();
}
