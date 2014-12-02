using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComboItem
{
    public ComboItem( string n , int o ) 
    {
        name = n;
        option = o;
    }

    public string name = "";
    public int option = 0;
}

public class ComboBoxCtrl : EditorControl 
{
    public ComboBoxCtrl() { }

    public void AddItem( string name , int option )
    {
        options.Add(new ComboItem(name, option));
        _UpdateOptions();
    }

    public void Clear()
    {
        options.Clear();
        _UpdateOptions();
    }
    
    void _UpdateOptions()
    {
        List<string> dispOptions = new List<string>();
        List<int> optValues = new List<int>();

        foreach( var item in options )
        {
            dispOptions.Add(item.name);
            optValues.Add(item.option);
        }

        displayOptions = dispOptions.ToArray();
        optionValues = optValues.ToArray();
    }

    public string[] DisplayOptions { get { return displayOptions; } }
    public int[] OptionValues { get { return optionValues; } }
    public int CurrOption { get { return currOption; } set { currOption = value; } }

    List<ComboItem> options = new List<ComboItem>();
    string[] displayOptions = new string[]{""};
    int[] optionValues = new int[]{0};
    int currOption = -1;
}
