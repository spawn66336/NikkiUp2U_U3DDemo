using UnityEngine;
using System.Collections;
using UnityEditor;

public class EditorRenderVisitor : EditorCtrlVisitor 
{


    private EditorRenderStrategy labelStrategy = new LabelCtrlRenderStrategy();
    private EditorRenderStrategy btnStrategy = new ButtonRenderStrategy();
    private EditorRenderStrategy hboxStrategy = new HBoxRenderStrategy();
    private EditorRenderStrategy vboxStrategy = new VBoxRenderStrategy();
    private EditorRenderStrategy hspliterStrategy = new HSpliterRenderStrategy();
    private EditorRenderStrategy vspliterStrategy = new VSpliterRenderStrategy();
    private EditorRenderStrategy mainviewStrategy = new MainViewRenderStrategy();
    private EditorRenderStrategy listviewStrategy = new ListViewRenderStrategy();
    private EditorRenderStrategy timelineviewStrategy = new TimeLineViewRenderStrategy();
    private EditorRenderStrategy inspectorStrategy = new InspectorRenderStrategy();
    private EditorRenderStrategy sliderStrategy = new SliderRenderStrategy();
    private EditorRenderStrategy playctrlStrategy = new PlayCtrlRenderStrategy();
    private EditorRenderStrategy colorctrlStrategy = new ColorCtrlRenderStrategy();
    private EditorRenderStrategy tabviewStrategy = new TabViewRenderStrategy();
    private EditorRenderStrategy treeviewStrategy = new TreeViewRenderStrategy();
    private EditorRenderStrategy comboxStrategy = new ComboBoxRenderStrategy();
    private EditorRenderStrategy textboxStrategy = new TextBoxRenderStrategy();

    private EditorRenderStrategy _GetStrategy(EditorControl c)
    {
        System.Type ctrlType = c.GetType();
        
        if( ctrlType == typeof(ButtonCtrl) )
        {
            return btnStrategy;
        }
        else if (ctrlType == typeof(HBoxCtrl))
        {
            return hboxStrategy;
        }
        else if (ctrlType == typeof(VBoxCtrl))
        {
            return vboxStrategy;
        }
        else if (ctrlType == typeof(HSpliterCtrl))
        {
            return hspliterStrategy;
        }
        else if (ctrlType == typeof(VSpliterCtrl))
        {
            return vspliterStrategy;
        }
        else if (ctrlType == typeof(MainViewCtrl))
        {
            return mainviewStrategy;
        }
        else if (ctrlType == typeof(ListViewCtrl))
        {
            return listviewStrategy;
        }
        else if (ctrlType == typeof(TimeLineViewCtrl))
        {
            return timelineviewStrategy;
        }
        else if (ctrlType == typeof(InspectorViewCtrl))
        {
            return inspectorStrategy;
        }else if( ctrlType == typeof(SliderCtrl))
        {
            return sliderStrategy;
        }else if( ctrlType == typeof(PlayCtrl))
        {
            return playctrlStrategy;
        }else if( ctrlType == typeof(LabelCtrl))
        {
            return labelStrategy;
        }
        else if (ctrlType == typeof(ColorCtrl))
        {
            return colorctrlStrategy;
        }else if( ctrlType == typeof(TabViewCtrl))
        {
            return tabviewStrategy;
        }else if( ctrlType == typeof(TreeViewCtrl))
        {
            return treeviewStrategy;
        }else if( ctrlType == typeof(ComboBoxCtrl) )
        {
            return comboxStrategy;
        }else if( ctrlType == typeof(TextBoxCtrl))
        {
            return textboxStrategy;
        }

        return null;
    }
 

    public override bool PreVisit(EditorControl c) 
    {
        EditorRenderStrategy s = _GetStrategy(c);
        if( s != null )
        {
            return s.PreVisit(c);
        }
        return true;
    }
    public override void Visit(EditorControl c) 
    {
        EditorRenderStrategy s = _GetStrategy(c);
        if (s != null)
        {
            s.Visit(c);
        }
    }
    public override void AfterVisit(EditorControl c) 
    {
        EditorRenderStrategy s = _GetStrategy(c);
        if (s != null)
        {
            s.AfterVisit(c);
        }
    }
    public override bool PreVisitChildren(EditorControl c) 
    {
        EditorRenderStrategy s = _GetStrategy(c);
        if (s != null)
        {
            return s.PreVisitChildren(c);
        }
        return true;
    }
    public override void AfterVisitChildren(EditorControl c) 
    {
        EditorRenderStrategy s = _GetStrategy(c);
        if (s != null)
        {
            s.AfterVisitChildren(c);
        }
    }

    public override bool PreVisitChild(EditorControl c, int ichild) 
    {
        EditorRenderStrategy s = _GetStrategy(c);
        if (s != null)
        {
            return s.PreVisitChild(c, ichild);
        }
        return true;
    }
    public override void AfterVisitChild(EditorControl c, int ichild)
    {
        EditorRenderStrategy s = _GetStrategy(c);
        if (s != null)
        {
            s.AfterVisitChild(c, ichild);
        }
    }
 
}
