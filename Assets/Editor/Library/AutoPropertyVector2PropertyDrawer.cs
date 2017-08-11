namespace Library
{
    using UnityEngine;
    using UnityEditor;

    //[CustomPropertyDrawer(typeof(AutoPropertyVector2))]
    public class AutoPropertyVector2PropertyDrawer : AutoPropertyGenericPropertyDrawer<Vector2>
    {
        protected override Vector2 DrawDataField(Rect position, SerializedProperty property)
        {
            var data = property.FindPropertyRelative("m_Data");

            return
                EditorGUI.Vector2Field(
                    position, property.displayName, data.vector2Value);
        }
    }
}
