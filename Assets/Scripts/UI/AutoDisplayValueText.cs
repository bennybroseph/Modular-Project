using System.Reflection;

namespace UI
{
    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteInEditMode]
    [RequireComponent(typeof(Text))]
    public class AutoDisplayValueText : AutoDisplayValue
    {
        private Text m_Text;

        protected override void UpdateGraphicObject()
        {
            m_Text = GetComponent<Text>();
        }
        protected override void UpdateDisplayValue()
        {
            m_Text.text = selectedProperty.GetValue(selectedObject, null).ToString();
        }

        protected override bool CheckProperty(PropertyInfo property)
        {
            return true;
        }
    }
}
