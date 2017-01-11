namespace Library
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(AutoPropertyInt))]
    public class AutoPropertyIntPropertyDrawer : AutoPropertyGenericPropertyDrawer<int>
    {
        protected override int DrawDataField(Rect position, SerializedProperty property)
        {
            var data = property.FindPropertyRelative("m_Data");

            return
                EditorGUI.IntField(
                    position, property.displayName, data.intValue);
        }
    }
}
