using UnityEngine;
using System.Collections;

public class UIPrepareCallback 
{ 
    public static IEnumerator PrepareLoginScene(OnProgressChange progressCallback ,OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        { 
            finishedCallback();
        } 
    }

    public static IEnumerator CleanLoginScene(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        {
            finishedCallback();
        }
        Debug.Log("CleanLoginScene Finished");
    }
	 
    public static IEnumerator PrepareMapScene(OnProgressChange progressCallback ,OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        {
            finishedCallback();
        } 
    }

    public static IEnumerator CleanMapScene(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        {
            finishedCallback();
        } 
    }

    public static IEnumerator PrepareCoreGameScene(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        {
            finishedCallback();
        } 
    }

    public static IEnumerator CleanCoreGameScene(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        {
            finishedCallback();
        } 
    }

    public static IEnumerator PrepareAreaMapUI(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    { 
        yield return 0;
        if (finishedCallback != null)
        {
            finishedCallback();
        }
    }

    public static IEnumerator CleanAreaMapUI(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    { 
        yield return 0;
        if (finishedCallback != null)
        {
            finishedCallback();
        }
    }


    public static IEnumerator PrepareLevelDialogUI(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        {
            finishedCallback();
        }
    }

    public static IEnumerator CleanLevelDialogUI(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        {
            finishedCallback();
        }
    }

    public static IEnumerator PrepareDressUI(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        {
            finishedCallback();
        }
    }

    public static IEnumerator CleanDressUI(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        {
            finishedCallback();
        }
    }

    public static IEnumerator PrepareRatingUI(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        {
            finishedCallback();
        }
    }

    public static IEnumerator CleanRatingUI(OnProgressChange progressCallback, OnPrepareFinished finishedCallback)
    {
        yield return new WaitForSeconds(2.0f);
        if (finishedCallback != null)
        {
            finishedCallback();
        }
    }
}
