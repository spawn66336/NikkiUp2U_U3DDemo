using UnityEngine;
using System.Collections;

public class UINumPos : MonoBehaviour {
	
	public Transform num;

	private float[] pos_y = new float[]{-479f, -367f, -273f, -172f, -73f, 34f, 137f, 238f, 339f, 443f};
	private Animation anim;
	private float curtime;
	// Use this for initialization
	void Start () {
		curtime = Time.time;
	}

	public void PlayAnimation()
	{
		anim = num.GetComponent<Animation>();
		if(anim != null)
		{
			AnimationClip defaultClip = anim.clip;
			
			if (defaultClip != null)
			{
				float speed = 1.5f;
				
				anim[defaultClip.name].wrapMode = WrapMode.Loop;
				anim[defaultClip.name].speed = speed;
				anim.Play();
			}
		}
	}

	public void SetPos(int i)
	{
		anim = num.GetComponent<Animation>();
		
		if(anim != null)
		{
			AnimationClip defaultClip = anim.clip;
			if(defaultClip != null)
			{
				anim[anim.clip.name].normalizedSpeed = 0f;
				anim[anim.clip.name].speed = 0f;
				anim.Stop();
			}
		}

		UITexture texture = num.GetComponent<UITexture>();
		if(texture != null)
		{
			Vector3 pos = new Vector3(num.localPosition.x, pos_y[i], num.localPosition.z); 
			num.localPosition = pos;
		}
	}
	
	// Update is called once per frame
	void Update () {
	}
}
