using System.Reflection;
using UnityEngine;

namespace UI
{
    using System.Linq;
    using UnityEditor;

    [CustomEditor(typeof(AutoDisplayValue))]
    public abstract class AutoDisplayValueEditor : Editor
    {
        private AutoDisplayValue m_AutoDisplayValue;

        private void OnEnable()
        {
            m_AutoDisplayValue = target as AutoDisplayValue;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var iter = serializedObject.GetIterator();
            iter.NextVisible(true);

            while (iter.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iter, true);

                if (iter.name != AutoDisplayValue.selectedObjectName)
                    continue;

                var selectedObject = m_AutoDisplayValue.selectedObject;
                if (selectedObject == null)
                    continue;

                m_AutoDisplayValue.selectedIndex =
                    EditorGUILayout.Popup(
                        "Property",
                        m_AutoDisplayValue.selectedIndex,
                        m_AutoDisplayValue.properties.Select(property => property.Name).ToArray());
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
