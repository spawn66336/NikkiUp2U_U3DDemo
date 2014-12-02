using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


public class SpeEditorAction
{
    private SpeEditorAction() { }

    public SpeEditorAction( AnimationClip anim )
    {
        clip = anim;
        length = clip.length;
        currPlayTime = 0.0f;
    }


    public AnimationClip AnimClip
    {
        get { return clip; }
    }

    public float Length
    {
        get { return length; }
    }

    public float CurrPlayTime
    {
        get { return currPlayTime; }
        set { currPlayTime = value; }
    }

    //动画片段
     AnimationClip clip;
    //动画片段长度
     float length = 0.0f;
    //当前播放时间
     float currPlayTime = 0.0f;
}

public class SpeEditorActionManager
{
    class TransformInfo
    {
        public string path;
        public Vector3 localPos;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    public SpeEditorActionManager()
    {
        _InitActionResources();
    }


    //记录模型的默认姿态
    public void RegisterDefaultBindPose( GameObject refModel )
    {
        defaultBindPose.Clear();

        Queue<Transform> q = new Queue<Transform>();
        List<string> nameList = new List<string>();

        Transform root =  refModel.transform; 
        q.Enqueue(root);

        while( q.Count > 0 )
        {
            Transform t = q.Dequeue();
            TransformInfo tinfo = new TransformInfo();
            tinfo.path = _CalcPath(t, root);
            tinfo.localPos = t.localPosition;
            tinfo.localRotation = t.localRotation;
            tinfo.localScale = t.localScale;
            defaultBindPose.Add(tinfo);

            for( int i = 0 ; i < t.childCount ; i++)
            {
                q.Enqueue( t.GetChild(i) );
            }
        }
        
    }

    private string _CalcPath(Transform t , Transform root )
    {
        string finalPath = "";
        while( t != null && t != root )
        {
            finalPath = '/'+t.name+finalPath;
            t = t.parent;
        }
        int i = finalPath.IndexOf('/');
        if( i != -1 )
        {
            finalPath = finalPath.Substring(i + 1);
        }
        return finalPath;
        
    }
    

    public void SetCurrPlayTime( GameObject refModel, float t)
    {
        Animator refModelAnimator = refModel.GetComponent<Animator>();

        //若当前模型没有Animator直接返回
        if (refModelAnimator == null)
        {
            return;
        }

        float localPlayTime = 0.0f; 
        //根据设置时间查找当前该播动画
        SpeEditorAction currPlayAction = _FindOutCurrAction( t , ref localPlayTime);
        //装载当前应该播放的动画
        _SetCurrPlayAction(refModelAnimator, currPlayAction);

        if (currPlayAction != null)
        {
            //设置当前动作播放时间
            _SetCurrPlayActionTime(refModelAnimator, localPlayTime);
        }
    }

    public void AddAction(AnimationClip clip)
    {
        SpeEditorAction newAction = new SpeEditorAction(clip);
        actions.Add(newAction);

    }

    public void Clear()
    {
        actions.Clear();
    }


    //加载程序必要资源
    private void _InitActionResources()
    {
        if (refModelAnimController == null)
        {
            refModelAnimController = Resources.Load("RefModelAnimController") as RuntimeAnimatorController;
        }
        refModeAnimOverrideController.name = "RefModelController";
        refModeAnimOverrideController.runtimeAnimatorController = refModelAnimController;

        nullClip = Resources.Load("RefModelEmptyAnim") as AnimationClip;
    }

    private void _ApplyDefaultBindPose(GameObject refModel)
    { 
        Transform root = refModel.transform;
        foreach( var tinfo in defaultBindPose )
        {
            Transform t = root.Find(tinfo.path);
            t.localPosition = tinfo.localPos;
            t.localRotation = tinfo.localRotation;
            t.localScale = tinfo.localScale;
        }
    }

    private SpeEditorAction _FindOutCurrAction( float t , ref float localTime )
    {
        float curr = 0.0f;
        localTime = 0.0f;
        SpeEditorAction findAction = null;
        foreach( var a in actions )
        {
            if( t >= curr &&  t < curr + a.Length )
            {
                findAction = a;
                localTime = t - curr;
                break;
            }
            curr += a.Length;
            //如果超时则定位到最后一个动作
            findAction = a;
            localTime = a.Length;
        } 
        return findAction;
    }

    private void _ResetAnimatorOverrideController()
    {
        refModeAnimOverrideController.PerformOverrideClipListCleanup();
        refModeAnimOverrideController = new AnimatorOverrideController();
        refModeAnimOverrideController.name = "RefModelController";
        refModeAnimOverrideController.runtimeAnimatorController = refModelAnimController;
    }
     

    private void _SetCurrPlayAction( Animator refModelAnimator , SpeEditorAction action )
    { 
        RuntimeAnimatorController finalController = null;
        if (action == null)
        {
            if (refModeAnimOverrideController["RefModelEmptyAnim"] != nullClip )
            {
                _ResetAnimatorOverrideController();
                refModeAnimOverrideController["RefModelEmptyAnim"] = nullClip;
            }
            finalController = refModeAnimOverrideController;

            //应用记录的初始姿态
            _ApplyDefaultBindPose(refModelAnimator.gameObject);
        }
        else
        {
            if (refModeAnimOverrideController["RefModelEmptyAnim"] != action.AnimClip)
            {
                _ResetAnimatorOverrideController();
                refModeAnimOverrideController["RefModelEmptyAnim"] = action.AnimClip;
            }
            finalController = refModeAnimOverrideController;
        }
        refModelAnimator.runtimeAnimatorController = finalController;

        //初始化播放状态
        {
            refModelAnimator.speed = 0f;
            refModelAnimator.Play("RefModelAnim", 0, 0f);
            refModelAnimator.Update(0.0f);
        }
    }

    private void _SetCurrPlayActionTime( Animator refModelAnimator , float localTime )
    {
        refModelAnimator.speed = 0f;
        float normalizedTime = localTime / refModelAnimator.GetCurrentAnimationClipState(0)[0].clip.length;

        if (normalizedTime > 1.0f)
            normalizedTime = 1.0f;

        refModelAnimator.speed = 0f;
        refModelAnimator.Play("RefModelAnim", 0, normalizedTime);
        refModelAnimator.Update(0.0f); 
    }


     
    //当前动作
    private SpeEditorAction currAction = null;
    //参考模型动画控制器
    private RuntimeAnimatorController refModelAnimController;
    //用于替换动画
    private AnimatorOverrideController refModeAnimOverrideController = new AnimatorOverrideController();

    private AnimationClip nullClip;

    private List<TransformInfo> defaultBindPose = new List<TransformInfo>();

    //列表中动作的总时间
    public float TotalTime
    {
        get
        {
            float total = 0f;
            foreach( var a in actions )
            {
                total += a.Length;
            }
            return total;
        } 
    }

    public List<SpeEditorAction> ActionList
    {
        get { return actions; }
    }

    List<SpeEditorAction> actions = new List<SpeEditorAction>();
}


public class SpecialEffectEditProxy
{
    private SpecialEffectEditProxy() { }

    public SpecialEffectEditProxy( SpecialEffect targetSpe , Object prefab ,bool edit )
    {
        editTarget = edit;
        realSpe = targetSpe;
        realSpePrefab = prefab;
        realSpe.PlayInEditModeInit();
                
        targetSpeOldParent = realSpe.gameObject.transform.parent; 
    }

    public bool BindTarget( GameObject obj )
    {
        return realSpe.BindTarget(obj);
    }

    public void ForceBindTarget( GameObject obj )
    {
     

        Vector3 localPos = realSpe.transform.localPosition;
        Quaternion localRotation = realSpe.transform.localRotation;
        Vector3 localScale = realSpe.transform.localScale;

        if (obj == null)
        {
            realSpe.transform.parent = null;
        }
        else
        { 
            realSpe.transform.parent = obj.transform;
        }

        realSpe.transform.localPosition = localPos;
        realSpe.transform.localRotation = localRotation;
        realSpe.transform.localScale = localScale;
    }


    public bool GetItemName( int i , ref string n )
    {
        if (i < 0 && i >= realSpe.elems.Count)
            return false;
        n = realSpe.elems[i].name;
        return true;
    }

    public bool ShowSelectElemInspector( int i )
    {
        if (i < 0 && i >= realSpe.elems.Count)
        { 
            return false;
        }
        Selection.activeGameObject = realSpe.elems[i].gameObject;
        return true;
    }

    public void ShowSpecialEffectInspector()
    {
        Selection.activeGameObject = realSpe.gameObject;
    }

    public bool GetItemTimeInfo( int i , ref float start , ref float length )
    {
        if (i < 0 && i >= realSpe.elems.Count)
            return false;

        start = _Trans2GlobalTime(realSpe.elems[i].startTime);
        length = realSpe.elems[i].playTime;
        return true;
    }

    public bool SetItemTimeInfo( int i , float start , float length )
    {
        if (i < 0 && i >= realSpe.elems.Count)
            return false;

        realSpe.elems[i].startTime = _Trans2LocalTime(start);
        if (realSpe.elems[i].startTime < 0f)
        {
            realSpe.elems[i].startTime = 0f;

            //忽略时长的修改
            _RecalcTotalTime();
            isDirty = true;
            return true;
        }

        realSpe.elems[i].playTime = length;
        if (realSpe.elems[i].playTime < 0f)
        {
            realSpe.elems[i].playTime = 0f;
        }

        _RecalcTotalTime(); 
        IsDirty = true;
        return true;
    }

    public bool GetItemStateInfo( int i , ref bool loop )
    {
        if (i < 0 && i >= realSpe.elems.Count)
            return false;

        loop = realSpe.elems[i].isLoop;
        return true;
    }

    public bool SetItemStateInfo( int i , bool loop )
    {
        if (i < 0 && i >= realSpe.elems.Count)
            return false;
        realSpe.elems[i].isLoop = loop;
        IsDirty = true;
        return true;
    }

    public int GetItemCount()
    {
        return realSpe.elems.Count;
    }

    public void Destroy()
    {
        if (!editTarget)
        {//若为非编辑特效直接删除
            //直接销毁prefab生成的编辑用实例特效
            GameObject.DestroyImmediate(realSpe.gameObject);
            return;
        }

        if (targetSpeOldParent == null)
        {
            ForceBindTarget(null);
        }
        else
        {
            ForceBindTarget(targetSpeOldParent.gameObject);
        }

        SpecialEffectEditorUtility.EnableSpecialEffect(realSpe);
        SpecialEffectEditorUtility.MarkSpecialEffectDirty(realSpe);

        //若此特效来自Prefab
        if (realSpePrefab != null )
        {
            if (IsDirty)
            {
                if (EditorUtility.DisplayDialog("警告！", "此特效为Prefab，编辑过程中有变动，是否保存至Prefab？", "是", "否"))
                {
                    //保存编辑结果至Prefab文件
                    PrefabUtility.ReplacePrefab(realSpe.gameObject, realSpePrefab);
                }
            }

            //直接销毁prefab生成的编辑用实例特效
            GameObject.DestroyImmediate(realSpe.gameObject);
        }

        targetSpeOldParent = null;
        realSpe = null;
        realSpePrefab = null;
    }


    public void _DebugPrintElemTimes()
    {
        Debug.Log("******************************");
        foreach( var e in realSpe.elems)
        {
            Debug.Log("s = " + e.startTime + " l = " + e.playTime);
        }
    }

    private float _Trans2GlobalTime( float t )
    {
        return t + startTime;
    }

    private float _Trans2LocalTime( float t )
    {
        return t - startTime;
    }

    private void _RecalcTotalTime()
    {
        float t = 0.0f;
        foreach (var e in realSpe.elems)
        {
            if (e.startTime + e.playTime > t)
                t = e.startTime + e.playTime;
        }
        realSpe.totalTime = t;
    }

    public string Name
    {
        get { return realSpe.name; }
    }
    
    public float TotalTime
    {
        get { return realSpe.totalTime; } 
    }

    public bool IsDirty
    {
        get { return isDirty; }
        set 
        { 
            isDirty = value;
            if( isDirty )
            {
                SpecialEffectEditorUtility.MarkSpecialEffectDirty(realSpe);
            }
        }
    }

    public string BindingTargetPath
    {
        get { return realSpe.BindingTargetPath; }
        set 
        { 
            realSpe.BindingTargetPath = value;
            IsDirty = true;
        }
    }

    public SpecialEffect.PlayStyle Style
    {
        get { return realSpe.style; }
        set 
        { 
            realSpe.style = value;
            IsDirty = true;
        }
    }

    public bool PlayOnAwake
    {
        get { return realSpe.playOnAwake; }
        set 
        { 
            realSpe.playOnAwake = value;
            IsDirty = true;
        }
    }
     
    public bool IsEditTarget
    {
        get { return editTarget; }
    }

    public float StartTime
    {
        get { return startTime; } 
        set
        {
            if( value < 0f )
            {
                startTime = 0f;
            }
            else
            {
                startTime = value;
            }
        }
    }

    public float CurrPlayTime
    {
        set
        {
            float localTime = _Trans2LocalTime(value);
            if( localTime < 0f )
            {
                realSpe.gameObject.SetActive(false);
            }
            else
            {
                if (localTime <= realSpe.totalTime)
                {
                    if (!realSpe.gameObject.activeInHierarchy)
                    {
                        realSpe.gameObject.SetActive(true);
                    }
                    realSpe.CurrPlayTime = localTime;
                }
                else
                {//播放时间超过总长直接消失
                    if( realSpe.gameObject.activeInHierarchy )
                    {
                        realSpe.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public SpecialEffect RealSpe
    {
        get { return realSpe; }
    }

    //标记是否为编辑特效，全局只可有一个编辑特效
    //其余特效均为参考特效
    bool editTarget = false;

    bool isDirty = false;

    //整体特效开始时间
    float startTime = 0.0f;

    //包裹的真实特效对象
    SpecialEffect realSpe = null;

    //特效Prefab
    Object realSpePrefab = null;

    //特效旧目标
    Transform targetSpeOldParent = null;
}


//参考特效管理器
public class RefSpecialEffectManager
{
    public RefSpecialEffectManager() { }

    public void Add( SpecialEffectEditProxy spe )
    {
        refSpeList.Add(spe);
    }

    public SpecialEffectEditProxy GetAt( int i )
    {
        if (i < 0 || i >= GetCount())
            return null;
        return refSpeList[i];
    }

    public void Clear()
    {
        foreach( var spe in refSpeList )
        {
            spe.Destroy();
        }
        refSpeList.Clear();
    }

    public int GetCount()
    {
        return refSpeList.Count;
    }

    public void SetCurrPlayTime( float t )
    {
        foreach( var spe in refSpeList )
        {
            spe.CurrPlayTime = t;
        }
    }

    public float GetPreviewTotalTime()
    {
        float totalTime = 0f;
        foreach( var spe in refSpeList )
        {
            totalTime = Mathf.Max(totalTime, spe.StartTime + spe.TotalTime);
        }
        return totalTime;
    }

    public void BindToTarget( GameObject go )
    {
        foreach( var spe in refSpeList )
        {
            spe.BindTarget(go);
        }
    }

    public void ForceBindToTarget( GameObject go )
    {
        foreach( var spe in refSpeList )
        {
            spe.ForceBindTarget(go);
        }
    }

    //尝试绑定到target，如果绑定不成功则强制绑定到backup
    public void TryBindToTarget(GameObject target, GameObject backup)
    {
        foreach( var spe in refSpeList )
        {
            if( !spe.BindTarget(target) )
            {
                if (backup != null)
                {
                    spe.ForceBindTarget(backup);
                }
            }
        }
    }

    private List<SpecialEffectEditProxy> refSpeList = new List<SpecialEffectEditProxy>();
}

public class SpecialEffectEditorModel
{ 

    /*
    * ==============================
    * 特效参考模型
    * ==============================
    */

    //参考模型所属Prefab
    private Object refModelPrefab;
    //参考模型实例
    private GameObject refModelInst;
    //参考模型路径
    private string refModelPath;  

    //动作管理器，用于支持动作序列播放
    private SpeEditorActionManager actionMgr = new SpeEditorActionManager();
    /*
     * ==============================
     * 编辑特效相关
     * ==============================
     */
     
    //目标特效
    private SpecialEffectEditProxy targetSpe;  
    //当前的播放时间
    private float currPlayTime = 0.0f;

    private RefSpecialEffectManager refSpeMgr = new RefSpecialEffectManager();

    /*
    * ==============================
    *  编辑器状态相关
    * ==============================
    */

    public int selectItem = -1;

    public delegate void ModelChangeNotify(SpecialEffectEditorModel m);

    public ModelChangeNotify onRefModelOpen;
    public ModelChangeNotify onRefModelDestroy;
     
    public ModelChangeNotify onSetNewEditTarget;
    public ModelChangeNotify onEditTargetSaved;
    public ModelChangeNotify onEditTargetValueChange;
    public ModelChangeNotify onCurrPlayTimeChange;
    public ModelChangeNotify onActionListChange;
    public ModelChangeNotify onRefSpeListChange;

    //在编辑目标销毁之前调用
    public ModelChangeNotify onEditTargetDestroy;
    public ModelChangeNotify onEditTargetDirty;

    public ModelChangeNotify onItemSelectChange;
     

    public void SetDirty( bool b = true )
    { 
        if( targetSpe != null )
        {
            targetSpe.IsDirty = true; 
        }

        if( onEditTargetDirty != null )
        {
            onEditTargetDirty(this);
        }
    }

    public float GetPreviewTotalTime()
    {
        float totalTime = 0f;

        if( targetSpe != null )
        {
            totalTime = targetSpe.StartTime + targetSpe.TotalTime;
        }

        float refSpeTotalTime = 
        refSpeMgr.GetPreviewTotalTime();

        totalTime = Mathf.Max(totalTime, refSpeTotalTime);

        return totalTime;
    }

    public void SetItemSelectIndx( int i )
    {
        selectItem = i;

        int speElems = 0;
        if( HasEditTarget() )
        {
            speElems = GetEditTarget().GetItemCount(); 
        }

        if( i < 0 && i >= speElems + refSpeMgr.GetCount() )
        {
            selectItem = -1;
        }

        NotifySelectItemChanged();
    }
    
    public void Destroy()
    {
        //销毁参考特效列表
        ClearRefSpeList();
        //清空动作列表
        ClearActionList();
        NotifyEditTargetDestroy();
        RetargetSpeToOldTarget();
        DestroyRefModel();
    }

    /*
     * 参照模型相关函数
     */
    public GameObject GetRefModel()
    {
        return refModelInst;
    }

    public bool HasRefModel()
    {
        return refModelInst != null;
    }

    public void OpenRefModelFromFile()
    {
        string absPath = EditorUtility.OpenFilePanel("打开参考模型", "Assets", "FBX");

        if (absPath.Length == 0)
            return;
         

        string prefabPath = absPath.Substring(absPath.IndexOf("Assets"));

        if (!OpenRefModelFromFile(prefabPath))
        {
            EditorUtility.DisplayDialog("", "没有找到指定的FBX!", "确定");
        }
    }

    public bool OpenRefModelFromFile( string path )
    {
        Object newPrefab = Resources.LoadAssetAtPath<Object>(path);
        if (newPrefab == null)
        { 
            return false;
        }

        //销毁旧模型
        DestroyRefModel();

        refModelPath = path;
        refModelPrefab = newPrefab; 
        refModelInst = GameObject.Instantiate(newPrefab) as GameObject; 

        actionMgr.RegisterDefaultBindPose(refModelInst); 
        NotifyRefModelOpen();
        return true;
    }

    public void DestroyRefModel()
    {
        //清空动作列表
        ClearActionList();

        NotifyRefModelDestroy();

        refModelPrefab = null;
        refModelPath = null;
        if( refModelInst != null )
        {
            GameObject.DestroyImmediate(refModelInst);
            refModelInst = null;
        }

        //清空未使用的Asset
        Resources.UnloadUnusedAssets();
    }
	     



    /*
     * 待编辑特效相关函数
     */

    //设置编辑目标特效
    public bool SetEditTarget( GameObject go )
    {
        SpecialEffect newSpe = go.GetComponent<SpecialEffect>();
        if (newSpe == null)
            return false;

        RetargetSpeToOldTarget();

        Object prefab = null;

        //若打开特效为Prefab
        if( PrefabUtility.GetPrefabType(go) == PrefabType.Prefab )
        {
            GameObject speInst = PrefabUtility.InstantiatePrefab(go) as GameObject; 
            prefab = go;
            newSpe = speInst.GetComponent<SpecialEffect>();
        }
          
        targetSpe = new SpecialEffectEditProxy(newSpe, prefab, true);
        targetSpe.ShowSpecialEffectInspector(); 

        NotifyEditTargetChange();    
        return true;
    }

    //获得编辑目标
    public SpecialEffectEditProxy GetEditTarget()
    {
        return targetSpe;
    }

    public void SetEditTargetStartTime( float t )
    {
        if( targetSpe == null ) 
            return; 

        targetSpe.StartTime = t;
        NotifyEditTargetValueChange();
    }


    public void RetargetSpeToOldTarget()
    {
        if (targetSpe == null)
            return;

        targetSpe.Destroy();
        targetSpe = null;

        NotifyEditTargetChange();
    }

    //是否有编辑对象
    public bool HasEditTarget()
    {
        return targetSpe != null;
    }

    
    /*
     * 预览特效相关函数 
     */
    public void SetRefSpeStartTime( int i , float t )
    {
        SpecialEffectEditProxy refSpe = refSpeMgr.GetAt(i);
        if( refSpe != null )
        {
            refSpe.StartTime = t; 
        }
    }

    public bool GetRefSpeStartTime( int i , ref float t )
    {
        SpecialEffectEditProxy refSpe = refSpeMgr.GetAt(i);
        if (refSpe != null)
        {
            t = refSpe.StartTime;
            return true;
        }
        return false;
    }

    public bool GetRefSpeLength( int i , ref float len )
    {
        SpecialEffectEditProxy refSpe = refSpeMgr.GetAt(i);
        if (refSpe != null)
        {
            len = refSpe.TotalTime;
            return true;
        }
        return false;
    }

    public bool GetRefSpeName( int i , ref string n )
    {
         SpecialEffectEditProxy refSpe = refSpeMgr.GetAt(i);
         if (refSpe != null)
         {
             n = refSpe.Name;
             return true;
         }
         return false;
    }

    public int GetRefSpeCount()
    {
        return refSpeMgr.GetCount();
    }

    public void AddRefSpe(GameObject go)
    {
        SpecialEffect newSpe = go.GetComponent<SpecialEffect>();
        if (newSpe == null)
            return;

        if (PrefabUtility.GetPrefabType(go) != PrefabType.Prefab)
        {
            EditorUtility.DisplayDialog("警告!", "请使用来自Prefab的特效作为参考特效！", "确定");
            return;
        }

        GameObject speInst = PrefabUtility.InstantiatePrefab(go) as GameObject; 
        newSpe = speInst.GetComponent<SpecialEffect>();

        SpecialEffectEditProxy newSpeProxy = new SpecialEffectEditProxy(newSpe, null, false);
        refSpeMgr.Add(newSpeProxy);
        NotifyEditTargetChange();
        NotifyRefSpeListChange();
    }

    public void ClearRefSpeList()
    {
        refSpeMgr.Clear();
        NotifyEditTargetChange();
        NotifyRefSpeListChange();
    }

    public void ShowSelectRefSpeInspector( int i )
    {
        SpecialEffectEditProxy refSpe = refSpeMgr.GetAt(i);
        if (refSpe != null)
        {
            refSpe.ShowSpecialEffectInspector();
        }
    }

    public void SetRefSpeCurrPlayTime( float t )
    {
        refSpeMgr.SetCurrPlayTime(t);
    }

    public void BindRefSpeToTarget( GameObject go )
    {
        refSpeMgr.BindToTarget(go);
    }

    public void ForceBindRefSpeToTarget( GameObject go )
    {
        refSpeMgr.ForceBindToTarget(go);
    }

    public void TryBindToTarget(GameObject target, GameObject backup)
    {
        refSpeMgr.TryBindToTarget(target, backup);
    }

    /*
     * 引用模型动作列表相关函数
     */ 

    public void AddRefModelAction( AnimationClip clip )
    {
        if( refModelInst == null )
        {
            EditorUtility.DisplayDialog("警告!", "载入动作前请先载入参照模型！", "确定");
            return;
        }

        actionMgr.AddAction(clip);
        ApplyCurrPlayTime();
        NotifyActionListChange();
    }

    public List<SpeEditorAction> RefModelActionList()
    {
        return actionMgr.ActionList;
    }

    public void ClearActionList()
    {
        //清空动作列表
        actionMgr.Clear();
        SetCurrPlayTime(0f);
        NotifyActionListChange();
    }

    public void LoadActionFromFile()
    {
        if (refModelInst == null)
        {
            EditorUtility.DisplayDialog("", "请先载入一个模型!", "确定");
            return;
        }
          
        string absPath = EditorUtility.OpenFilePanel("打开动作文件", "Assets", "FBX");

        if (absPath.Length == 0)
            return;

        string actionPath = absPath.Substring(absPath.IndexOf("Assets")); 
        AnimationClip animClip = Resources.LoadAssetAtPath<AnimationClip>(actionPath);
        AddRefModelAction(animClip); 
    }


    /*
     * 预览播放相关函数
     */
    public float GetCurrPlayTime()
    {
        return currPlayTime;
    }
      
    //设置当前播放时间，此函数会同时设置特效与参考模型
    //当前播放时间
    public void SetCurrPlayTime( float t )
    { 
        currPlayTime = t;
        ApplyCurrPlayTime(); 
        NotifyCurrPlayTimeChange();
    }

    //此函数会用记录的当前播放时间（currPlayTime）来
    //设置特效与参考模型当前时间
    public void ApplyCurrPlayTime()
    {
        //通知动作模型变化(注意需要先更新动作，否则后续特效要有绑定骨骼会播放异常)
        if (refModelInst != null)
        {
            actionMgr.SetCurrPlayTime(refModelInst, currPlayTime);
        }

        //参考特效播放时间更新
        refSpeMgr.SetCurrPlayTime(currPlayTime);

        //若已打开特效，设置当前播放时间
        if (targetSpe != null)
        {
            targetSpe.CurrPlayTime = currPlayTime;
        }  
    }
 
    



     

    /*
     * 回调通知函数
     */
    public void NotifyRefModelOpen()
    {
        if( onRefModelOpen != null )
        {
            onRefModelOpen(this);
        }
    }

    public void NotifyRefModelDestroy()
    {
        if (onRefModelDestroy != null)
        {
            onRefModelDestroy(this);
        }
    }
    
    public void NotifySetNewEditTarget()
    {
        if(onSetNewEditTarget != null )
        {
            onSetNewEditTarget(this);
        }
    }

    public void NotifyEditTargetChange()
    {
        NotifySetNewEditTarget();
        SetCurrPlayTime(0);
        NotifyActionListChange();
        SetItemSelectIndx(-1);

    }

    public void NotifyEditTargetSaved()
    {
        if (onEditTargetSaved != null)
        {
            onEditTargetSaved(this);
        }
    }

    public void NotifyEditTargetValueChange()
    {
        if( onEditTargetValueChange != null)
        {
            onEditTargetValueChange(this);
        }
    }

    public void NotifyEditTargetDestroy()
    {
        if (onEditTargetDestroy != null)
        {
            onEditTargetDestroy(this);
        }
    }

    public void NotifySelectItemChanged()
    {
        if( onItemSelectChange != null )
        {
            onItemSelectChange(this);
        }
    }

    public void NotifyCurrPlayTimeChange()
    {
        if(onCurrPlayTimeChange != null )
        {
            onCurrPlayTimeChange(this);
        }
    }

    public void NotifyActionListChange()
    {
        if( onActionListChange != null )
        {
            onActionListChange(this);
        }
    }

    public void NotifyRefSpeListChange()
    {
        if( onRefSpeListChange != null )
        {
            onRefSpeListChange(this);
        }
    }

    /*
     * 对Command函数的响应函数
     */
    public void OnBeforeCmdExecute( IEditorCommand cmd)
    {
        if (!HasEditTarget())
            return;

        //GetEditTarget()._DebugPrintElemTimes(); 
        SpecialEffectEditorUtility.UndoRecordSpecialEffect(cmd, GetEditTarget().RealSpe);
    }

    public void OnAfterCmdExecute( IEditorCommand cmd )
    {
        if (!HasEditTarget())
            return;
        SpecialEffectEditorUtility.MarkSpecialEffectDirty(GetEditTarget().RealSpe);
    }

    public void OnUndoRedo()
    {
        if( HasEditTarget() )
        {
            //GetEditTarget()._DebugPrintElemTimes();
        }
        NotifyEditTargetChange();
    }


    private static SpecialEffectEditorModel s_instance = null;
    public static SpecialEffectEditorModel GetInstance()
    {
        if( s_instance == null )
        {
            s_instance = new SpecialEffectEditorModel();
        }
        return s_instance;
    }
}
