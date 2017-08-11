namespace UI
{
    using System;
    using System.Reflection;

    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteInEditMode]
    [RequireComponent(typeof(Image))]
    public class AutoDisplayValueImage : AutoDisplayValue
    {
        private Image m_Image;

        [SerializeField]
        private float m_MaxValue;

        protected override void UpdateGraphicObject()
        {
            m_Image = GetComponent<Image>();
        }
        protected override void UpdateDisplayValue()
        {
            m_Image.fillAmount = (float)selectedProperty.GetValue(selectedObject, null) / m_MaxValue;
        }

        protected override bool CheckProperty(PropertyInfo property)
        {
            var type = property.PropertyType;
            return type == typeof(int) || type == typeof(float);
        }
    }
}
