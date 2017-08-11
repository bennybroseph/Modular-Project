using System.Linq;

namespace UI
{
    using System.Reflection;

    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteInEditMode]
    public abstract class AutoDisplayValue : MonoBehaviour
    {
        [SerializeField]
        private Object m_SelectedObject;
        private Object m_PrevSelectedObject;
        public const string selectedObjectName = "m_SelectedObject";

        private PropertyInfo[] m_Properties;
        private int m_SelectedIndex;

        public Object selectedObject { get { return m_SelectedObject; } }

        public PropertyInfo[] properties { get { return m_Properties; } }
        public int selectedIndex { get { return m_SelectedIndex; } set { m_SelectedIndex = value; } }

        public PropertyInfo selectedProperty { get { return m_Properties[selectedIndex]; } }

        private void OnValidate()
        {
            UpdatePropertyInfo();
            UpdateGraphicObject();
        }

        // Use this for initialization
        private void Awake()
        {
            UpdatePropertyInfo();
            UpdateGraphicObject();
        }

        private void Update()
        {
            UpdateDisplayValue();
        }

        private void UpdatePropertyInfo()
        {
            m_Properties = selectedObject.GetType().GetProperties().Where(CheckProperty).ToArray();

            if (m_PrevSelectedObject != m_SelectedObject ||
                m_SelectedIndex < 0 || m_SelectedIndex > m_Properties.Length)
                m_SelectedIndex = 0;

            m_PrevSelectedObject = m_SelectedObject;
        }

        protected abstract void UpdateGraphicObject();
        protected abstract void UpdateDisplayValue();

        protected abstract bool CheckProperty(PropertyInfo property);
    }
}
