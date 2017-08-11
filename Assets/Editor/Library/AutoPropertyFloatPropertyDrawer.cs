namespace Library
{
    using UnityEngine;
    using UnityEditor;

    //[CustomPropertyDrawer(typeof(AutoPropertyFloat))]
    public class AutoPropertyFloatPropertyDrawer : AutoPropertyGenericPropertyDrawer<float>
    {
        protected override float DrawDataField(Rect position, SerializedProperty property)
        {
            var data = property.FindPropertyRelative("m_Data");

            return
                EditorGUI.FloatField(
                    position, property.displayName, data.floatValue);
        }
    }
}
