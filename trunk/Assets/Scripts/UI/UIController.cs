using UnityEngine;
using System.Collections;

public class UIController : MonoBehaviour 
{
    
    public virtual void OnEnterUI()
    {
        this.gameObject.SetActive(true);
    }

    public virtual void OnLeaveUI()
    {
        this.gameObject.SetActive(false);
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
