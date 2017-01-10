namespace Library
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(AutoPropertyFloat))]
    public class AutoPropertyFloatPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var self = fieldInfo.GetValue(property.serializedObject.targetObject) as AutoPropertyFloat;
            if (self == null)
                return;

            EditorGUI.BeginProperty(position, label, property);
            {
                var newValue = EditorGUI.FloatField(position, property.displayName, self.value);
                if (Mathf.Abs(self.value - newValue) > float.Epsilon)
                    self.value = newValue;
            }
            EditorGUI.EndProperty();
        }
    }
}
