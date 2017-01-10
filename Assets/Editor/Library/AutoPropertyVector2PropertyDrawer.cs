namespace Library
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(AutoPropertyVector2))]
    public class AutoPropertyVector2PropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var self = fieldInfo.GetValue(property.serializedObject.targetObject) as AutoPropertyVector2;
            if (self == null)
                return;

            EditorGUI.BeginProperty(position, label, property);
            {
                var newValue = EditorGUI.Vector2Field(position, property.displayName, self.value);
                if (self.value != newValue)
                    self.value = newValue;
            }
            EditorGUI.EndProperty();
        }
    }
}
