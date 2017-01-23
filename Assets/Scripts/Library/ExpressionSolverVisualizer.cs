namespace Library
{
    using System.Linq;
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

            m_Text.text = string.Empty;
            foreach (var expressionObject in m_Expression.expressionObjects)
            {
                if (m_Expression.currentlyEvaluatedObjects.Any(obj => obj == expressionObject))
                {
                    m_Text.text += "<color=#AAAAAAFF>" + expressionObject.stringValue + "</color>";
                }
                else
                    m_Text.text += expressionObject.stringValue;
            }
        }
    }
}
