namespace Library
{
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(AutoPropertyVector3))]
    public class AutoPropertyVector3PropertyDrawer : AutoPropertyGenericPropertyDrawer<Vector3>
    {
        protected override Vector3 DrawDataField(Rect position, SerializedProperty property)
        {
            var data = property.FindPropertyRelative("m_Data");

            return
                EditorGUI.Vector3Field(
                    position, property.displayName, data.vector3Value);
        }
    }
}
