using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        Color color = GUI.color;

        GUI.enabled = false;
        GUI.color = new Color(0.8f, 0.8f, 1.0f);
        EditorGUI.PropertyField(position, property, label, true);
        GUI.color = color;
        GUI.enabled = true;
    }
}