namespace Library
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(AutoPropertyString))]
    public class AutoPropertyStringPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var self = fieldInfo.GetValue(property.serializedObject.targetObject) as AutoPropertyString;
            if (self == null)
                return;

            EditorGUI.BeginProperty(position, label, property);
            {
                var newValue = EditorGUI.DelayedTextField(position, property.displayName, self.value);
                if (self.value != newValue)
                    self.value = newValue;
            }
            EditorGUI.EndProperty();
        }
    }
}
