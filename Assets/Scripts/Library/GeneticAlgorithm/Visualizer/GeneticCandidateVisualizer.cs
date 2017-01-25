namespace Library.GeneticAlgorithm.Visualizer
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Text))]
    public class GeneticCandidateVisualizer : MonoBehaviour
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
        private void LateUpdate()
        {
            m_Text.color = m_GeneticSolver.currentGeneticEquation.solvingCandidate ==
                           m_GeneticSolver.currentlyEvaluatedCandidate ? Color.green : Color.white;

            m_Text.text = "<b>Generation " + m_GeneticSolver.currentGeneticEquation.generations + "</b>\n\n";
            m_Text.text +=
                "<b><i>Candidate " +
                (m_GeneticSolver.currentGeneticEquation.currentGeneration.candidates.
                    FindIndex(candidate => candidate == m_GeneticSolver.currentlyEvaluatedCandidate) + 1) +
                    " / " + m_GeneticSolver.currentGeneticEquation.currentGeneration.candidates.Count +
                    " Age - " + m_GeneticSolver.currentlyEvaluatedCandidate.age + ":</i></b>\n";

            foreach (var chromosome in m_GeneticSolver.currentlyEvaluatedCandidate.chromosomes)
            {
                if (chromosome.inherited)
                    m_Text.text += "<color=#3333FFFF>";

                m_Text.text += chromosome.name + " = " + chromosome.value + "\n";

                if (chromosome.inherited)
                    m_Text.text += "</color>";
            }

            if (m_GeneticSolver.currentGeneticEquation.solvingCandidate !=
                m_GeneticSolver.currentlyEvaluatedCandidate)
                return;

            m_Text.text += "\n\n";
            foreach (var chromosome in m_GeneticSolver.currentlyEvaluatedCandidate.chromosomes)
                m_Text.text += chromosome.value ? "1 " : "0 ";
        }
    }
}
