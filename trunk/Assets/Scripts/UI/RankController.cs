using UnityEngine;
using System.Collections;

public class RankController : MonoBehaviour {


    UISprite[] hearts = null;
    Vector2 heartSize = Vector2.zero;

	// Use this for initialization
	void Start () 
    {

	}
	
    public void SetRank( int num )
    {
        if (hearts == null)
        {
            hearts = GetComponentsInChildren<UISprite>();
            if (hearts.Length > 0)
            {
                heartSize = new Vector2(hearts[0].width, hearts[0].height);
            }

            foreach (var h in hearts)
            {
                h.gameObject.SetActive(false);
            }
        }

        if( num < 0)
        {
            num = 0;
        }
        if( num > hearts.Length )
        {
            num = hearts.Length;
        }

        float x = -( heartSize.x * (float)num ) * 0.5f;
        x += heartSize.x * 0.5f;

        for( int i = 0 ; i < num ; i++ )
        {
            hearts[i].gameObject.SetActive(true);
            hearts[i].gameObject.transform.localPosition = new Vector3(x + ((float)i) * heartSize.x , 0 , 0); 
        }

        for( int i = num ; i < hearts.Length ; i++)
        {
            hearts[i].gameObject.SetActive(false);
        }
    }

	// Update is called once per frame
	void Update () 
    {
	
	}
}
