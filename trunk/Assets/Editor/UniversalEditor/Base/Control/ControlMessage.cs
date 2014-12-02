using UnityEngine;
using System.Collections;

 public class ControlMessage
 {
     public enum Message
     {
         _MESSAGE_START = 0,

         TIMELINECTRL_BEGIN_DRAG_ITEMS,
         TIMELINECTRL_DRAG_ITEMS,
         TIMELINECTRL_END_DRAG_ITEMS,

         TIMELINECTRL_BEGIN_DRAG_TAG ,
         TIMELINECTRL_DRAG_TAG,
         TIMELINECTRL_END_DRAG_TAG
     }

     private ControlMessage() { }

     public ControlMessage( EditorControl s , Message m , object p0 = null , object p1 = null )
     {
         sender = s;
         msg = m;
         param0 = p0;
         param1 = p1;
     }

     public EditorControl sender;
     public Message msg;
     public object param0;
     public object param1;
 }
