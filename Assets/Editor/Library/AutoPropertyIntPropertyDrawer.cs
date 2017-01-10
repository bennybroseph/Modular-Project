﻿namespace Library
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(AutoPropertyInt))]
    public class AutoPropertyIntPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var self = fieldInfo.GetValue(property.serializedObject.targetObject) as AutoPropertyInt;
            if (self == null)
                return;

            EditorGUI.BeginProperty(position, label, property);
            {
                var newValue = EditorGUI.IntField(position, label, self.value);
                if (self.value != newValue)
                    self.value = newValue;
            }
            EditorGUI.EndProperty();
        }
    }
}
