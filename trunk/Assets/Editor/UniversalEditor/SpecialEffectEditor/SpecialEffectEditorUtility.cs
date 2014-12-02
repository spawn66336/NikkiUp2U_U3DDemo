using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;

public class SpecialEffectEditorUtility  
{ 

    public static bool GetLastRect(ref Rect lastRect)
    {
        if (Event.current.type == EventType.Repaint)
        {
            lastRect = GUILayoutUtility.GetLastRect();
            return true;
        } 
        return false; 
    }

    public static void EnableSpecialEffect( SpecialEffect spe ,bool enable = true )
    {
        spe.gameObject.SetActive(enable); 
        foreach (var e in spe.elems)
        {
            e.SetEnable(enable);
        }
    }

    public static void MarkSpecialEffectDirty( SpecialEffect spe )
    {
        if (Application.isPlaying)
            return; 
         
        foreach (var e in spe.elems)
        {
            EditorUtility.SetDirty(e);
        }
        EditorUtility.SetDirty(spe);
    }


    public static void UndoRecordSpecialEffect( IEditorCommand cmd, SpecialEffect spe )
    {
        if (Application.isPlaying)
            return;

        List<UnityEngine.Object> objList = new List<UnityEngine.Object>();

        foreach (var e in spe.elems)
        {
            objList.Add(e);
        }
        objList.Add(spe);

        Undo.RecordObjects(objList.ToArray(), cmd.Name);
        //Undo.RegisterCompleteObjectUndo(objList.ToArray(), cmd.Name);
    }

    //判断特效脚本属性值是否相等
    //前置条件：脚本所挂接的gameObject除特效之外的其他属性必需相等
    public static bool IsSpecialEffectValueEqual( SpecialEffect lhs , SpecialEffect rhs)
    {
        return lhs.Equals(rhs); 
    }

    //将src脚本值拷贝至dst中
    //前置条件：脚本所挂接的gameObject除特效之外的其他属性必需相等
    public static bool CopySpecialEffectValues( SpecialEffect src , SpecialEffect dst )
    {
        return dst._CopyValues(src);
    }

    //获取当前GameObject所属的特效
    public static SpecialEffect GetBelongSpecialEffect( GameObject o )
    {
        if (o == null)
            return null;

        GameObject go = o;
        while( go != null )
        {
            SpecialEffect spe = go.GetComponent<SpecialEffect>();
            if (spe != null)
                return spe;

            //继续向上查找
            if( null != go.transform.parent )
            {
                go = go.transform.parent.gameObject;
            }
            else
            {
                go = null;
            }
        }
         
        return null;
    }

    

    //刷新特效下的所有特效元素，如果元素没有绑定特定
    //的特效元素脚本会自动绑定，不对的脚本也会自动调整。
    public static void RefreshSpecialEffect(GameObject go)
    {
        if (go == null)
            return;

        SpecialEffect spe = go.GetComponent<SpecialEffect>();
        
        if (spe == null)
            return;

        spe.elems.Clear();

        Queue<Transform> q = new Queue<Transform>();
        q.Enqueue(go.transform);

        while(q.Count > 0)
        {
            Transform t = q.Dequeue();
            for(int i = 0 ; i < t.childCount ; i++)
            {
                Transform child = t.GetChild(i);
                TryAddSpecialElemComponent(child.gameObject);
                spe.elems.Add(child.gameObject.GetComponent<SpecialEffectElement>());
                q.Enqueue(child);
            }
        }

    }

    private static void TryAddSpecialElemComponent(GameObject go )
    {
        if (go == null)
            return;

        SpecialEffect spe = go.GetComponent<SpecialEffect>();

        //特效根节点
        if (spe != null)
            return;

        SpecialEffectElement speElem = go.GetComponent<SpecialEffectElement>();

        if( speElem != null )
        {
            //若绑定脚本非法先卸除，重新绑定
            if( !IsSpecialEffectElementLegal(go) )
            {
                UnityEngine.Object.DestroyImmediate(speElem);
                speElem = null;
            }
        }

        
        if (speElem == null)
        {
            ParticleSystem p = go.GetComponent<ParticleSystem>();

            if (go.GetComponent<ParticleSystem>() != null )
            {
                go.AddComponent<SpecialEffectParticleSys>();
            }
            else if (go.GetComponent<Animation>() != null )
            {
                go.AddComponent<SpecialEffectAnimation>();
            }
            else if(go.GetComponent<Animator>() != null )
            {
                go.AddComponent<SpecialEffectAnimator>();
            }
            else
            {
                go.AddComponent<SpecialEffectElement>();
            } 
        }
    }

    private static bool IsSpecialEffectElementLegal( GameObject go )
    {
        if (go == null)
            return false;

        SpecialEffect spe = go.GetComponent<SpecialEffect>();

        //特效根节点
        if (spe != null)
            return false;

        SpecialEffectElement speElem = go.GetComponent<SpecialEffectElement>();
        Type elemType = speElem.GetType();

        if (elemType == typeof(SpecialEffectParticleSys))
        {
            ParticleSystem particleSys = go.GetComponent<ParticleSystem>();
            if (particleSys == null)
                return false;
        }
        else if (elemType == typeof(SpecialEffectAnimation))
        {
            Animation anim = go.GetComponent<Animation>();
            if (anim == null)
                return false;
        }
        else if(elemType == typeof(SpecialEffectAnimator))
        {
            Animator anim = go.GetComponent<Animator>();
            if (anim == null)
                return false;
        }

        return true;
    }
}
