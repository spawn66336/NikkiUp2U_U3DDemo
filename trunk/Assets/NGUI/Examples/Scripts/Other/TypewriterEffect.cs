//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Trivial script that fills the label's contents gradually, as if someone was typing.
/// </summary>

[RequireComponent(typeof(UILabel))]
[AddComponentMenu("NGUI/Examples/Typewriter Effect")]
public class TypewriterEffect : MonoBehaviour
{
	public int charsPerSecond = 40;

	UILabel mLabel;
	string mText;
	int mOffset = 0;
	float mNextChar = 0f;
    [System.NonSerialized]
	public bool mReset = true;
    void Awake()
    {
        mLabel = GetComponent<UILabel>();
    }
	/*void OnEnable () 
    {
        mReset = true;
    }*/

	void Update ()
	{
        
		if (mReset)
		{
			mOffset = 0;
			mReset = false;			
			//mText = mLabel.processedText;
		}

		if (mOffset < mText.Length && mNextChar <= RealTime.time)
		{
			charsPerSecond = Mathf.Max(1, charsPerSecond);

			// Periods and end-of-line characters should pause for a longer time.
			float delay = 1f / charsPerSecond;
			char c = mText[mOffset];
			if (c == '.' || c == '\n' || c == '!' || c == '?') delay *= 4f;

			// Automatically skip all symbols
			NGUIText.ParseSymbol(mText, ref mOffset);

			mNextChar = RealTime.time + delay;
			mLabel.text = mText.Substring(0, ++mOffset);
		}
	}
    public int rightNowIndex;
    public void ChangeNextTex(string newStr)
    {
        
        if (mOffset < mText.Length)
        {
            mOffset = mText.Length;
            mLabel.text = mText.Substring(0, mOffset);
        }
        else
        {
            ++rightNowIndex;
            mText = newStr;
            mReset = true;
        }
    }
    public void ShowLastTex()
    {
        ++rightNowIndex;
        mOffset = mText.Length;
        mLabel.text = mText.Substring(0, mOffset);
    }

    internal void SetInit(string str)
    {
        mText = str;
        mReset = true;
        
    }
}
