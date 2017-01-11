namespace Library
{
    using UnityEngine;
    using UnityEditor;

    public abstract class AutoPropertyGenericPropertyDrawer<T> : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var self = fieldInfo.GetValue(property.serializedObject.targetObject) as AutoProperty<T>;
            if (self == null)
                return;

            EditorGUI.BeginProperty(position, label, property);
            {
                var originalWidth = position.size.x;

                position.size = new Vector2(2f, position.size.y);
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, string.Empty);
                position.size = new Vector2(originalWidth, EditorGUIUtility.singleLineHeight);

                var data = DrawDataField(position, property);
                if (!self.value.Equals(data))
                    self.value = data;

                if (property.isExpanded)
                {
                    position.position =
                        new Vector2(
                            position.position.x,
                            position.position.y +
                            EditorGUIUtility.singleLineHeight +
                            EditorGUIUtility.standardVerticalSpacing);

                    EditorGUI.PropertyField(
                        position,
                        property.FindPropertyRelative("onChangeEvent"));
                }
            }
            EditorGUI.EndProperty();
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

        protected abstract T DrawDataField(Rect position, SerializedProperty property);
    }
}