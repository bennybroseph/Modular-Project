using UnityEngine;
using UnityEngine.UI;

namespace Library.GeneticAlgorithm.Visualizer
{
    [RequireComponent(typeof(Text))]
    public class GeneticEquationVisualizer : MonoBehaviour
    {
        [SerializeField]
        private GeneticEquation m_GeneticEquation;

        private Text m_Text;

        public GeneticEquation geneticEquation
        {
            get { return m_GeneticEquation; }
            set { m_GeneticEquation = value; }
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
            m_Text.text = m_GeneticEquation.expression.stringValue;
        }
    }
}
