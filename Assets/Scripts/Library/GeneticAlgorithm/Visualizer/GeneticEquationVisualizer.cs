using UnityEngine;
using UnityEngine.UI;

namespace Library.GeneticAlgorithm.Visualizer
{
    [RequireComponent(typeof(Text))]
    public class GeneticEquationVisualizer : MonoBehaviour
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
        private void Start()
        {
            m_Text = GetComponent<Text>();

            UpdateText();
        }

        private void Update()
        {
            UpdateText();
        }

        // Update is called once per frame
        private void UpdateText()
        {
            m_Text.text = m_GeneticSolver.currentGeneticEquation.expression.stringValue;
        }
    }
}
