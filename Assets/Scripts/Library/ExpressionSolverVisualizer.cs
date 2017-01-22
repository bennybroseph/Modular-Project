namespace Library
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Text))]
    public class ExpressionSolverVisualizer : MonoBehaviour
    {
        [SerializeField]
        private Expression m_Expression;

        private Text m_Text;

        public Expression expression
        {
            get { return m_Expression; }
            set { m_Expression = value; }
        }

        // Use this for initialization
        private void Awake()
        {
            m_Text = GetComponent<Text>();
            if (m_Text.font == null)
                m_Text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // Update is called once per frame
        private void Update()
        {
            m_Expression.UpdateStringValue();
            m_Text.text = m_Expression.stringValue;
        }
    }
}
