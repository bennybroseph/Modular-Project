namespace Library
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(AutoPropertyString))]
    public class AutoPropertyStringPropertyDrawer : AutoPropertyGenericPropertyDrawer<string>
    {
        protected override string DrawDataField(Rect position, SerializedProperty property)
        {
            var data = property.FindPropertyRelative("m_Data");

            return 
                EditorGUI.DelayedTextField(
                    position, property.displayName, data.stringValue);
        }
    }
}
