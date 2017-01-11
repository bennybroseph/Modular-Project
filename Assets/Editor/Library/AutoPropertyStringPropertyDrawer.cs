namespace Library
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(AutoPropertyString))]
    public class AutoPropertyStringPropertyDrawer : AutoPropertyGenericPropertyDrawer<string> { }

    public class AutoPropertyGenericPropertyDrawer<T> : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var self = fieldInfo.GetValue(property.serializedObject.targetObject) as AutoProperty<T>;
            if (self == null)
                return;

            var oldValue = self.newValue;
            EditorGUI.BeginProperty(position, label, property);
            {
                var originalWidth = position.size.x;

                position.size = new Vector2(5f, position.size.y);
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, string.Empty);
                position.size = new Vector2(originalWidth, EditorGUIUtility.singleLineHeight);

                var data = property.FindPropertyRelative("m_Data");
                EditorGUI.PropertyField(position, data);

                if (property.isExpanded)
                {
                    position.position =
                        new Vector2(
                            position.position.x,
                            position.position.y +
                                EditorGUIUtility.singleLineHeight +
                                EditorGUIUtility.standardVerticalSpacing);

                    EditorGUI.PropertyField(
                        position, property.FindPropertyRelative("onChangeEvent"));
                }
            }
            EditorGUI.EndProperty();

            if (!self.value.Equals(oldValue))
                self.value = self.value;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);

            if (property.isExpanded)
            {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onChangeEvent"));
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }
    }
}
