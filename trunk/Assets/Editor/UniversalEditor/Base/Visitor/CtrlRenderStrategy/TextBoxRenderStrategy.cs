using UnityEngine;
using System.Collections;
using UnityEditor;

public class TextBoxRenderStrategy : EditorRenderStrategy
{
    public override void Visit(EditorControl c)
    {
        TextBoxCtrl textBox = c as TextBoxCtrl;


        textBoxContent.text = textBox.Caption;
        if (textBox.Icon != null)
        {
            textBoxContent.image = textBox.Icon;
        }
        Vector2 labelSize = EditorStyles.label.CalcSize(textBoxContent);
        EditorGUILayout.LabelField(textBoxContent, new GUILayoutOption[] {GUILayout.Width(labelSize.x)});
        string newText = EditorGUILayout.TextField(textBox.Text);

        if(!newText.Equals(textBox.Text))
        {
            textBox.Text = newText;
            textBox.frameTriggerInfo.isValueChanged = true;
        }
    }

    GUIContent textBoxContent = new GUIContent();
}
