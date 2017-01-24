namespace Library
{
    using System.Linq;

    using GeneticAlgorithm;

    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Text))]
    public class ExpressionSolverVisualizer : MonoBehaviour
    {
        [SerializeField]
        private GeneticSolver m_GeneticSolver;

        private Text m_Text;

        public GeneticSolver geneticSolver
        {
            get { return m_GeneticSolver; }
            set { m_GeneticSolver = value; }
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
            if (m_GeneticSolver.currentlyEvaluatedExpression == null)
                return;

            m_GeneticSolver.currentlyEvaluatedExpression.UpdateStringValue();

            m_Text.text = string.Empty;

            UpdateText(m_GeneticSolver.currentlyEvaluatedExpression);
        }

        private void UpdateText(Expression expression)
        {
            var expressionObjects = expression.expressionObjects;
            for (var i = 0; i < expressionObjects.Count; ++i)
            {
                var currentlyEvaluated =
                    expression.currentlyEvaluatedObjects.Any(obj => obj == expressionObjects[i]);

                var nextObject =
                    i + 1 < expressionObjects.Count ? expressionObjects[i + 1] : null;

                if (expressionObjects[i] is Expression)
                {
                    if (currentlyEvaluated)
                        m_Text.text += "<color=#3333FFFF>";

                    UpdateText(expressionObjects[i] as Expression);

                    if (currentlyEvaluated)
                        m_Text.text += "</color>";
                }
                else
                {
                    var stringValue =
                        expressionObjects[i] is Delimiter || nextObject is Delimiter ||
                        expressionObjects[i] is Not
                            ? expressionObjects[i].stringValue
                            : expressionObjects[i].stringValue + " ";

                    if (currentlyEvaluated)
                        m_Text.text += "<color=#33FF33FF>" + stringValue + "</color>";
                    else
                        m_Text.text += stringValue;
                }
            }
        }
    }
}
