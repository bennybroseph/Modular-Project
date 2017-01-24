namespace Library.GeneticAlgorithm.Visualizer
{
    using System.Linq;

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
            if (m_GeneticSolver.currentGeneticEquation.solvingCandidate ==
                m_GeneticSolver.currentlyEvaluatedCandidate)
                m_Text.color = Color.green;
            else
                m_Text.color = Color.white;

            m_Text.text = "Generation " + m_GeneticSolver.currentGeneticEquation.generations + "\n\n";
            m_Text.text +=
                "Candidate " +
                (m_GeneticSolver.currentGeneticEquation.currentGeneration.candidates.
                    FindIndex(candidate => candidate == m_GeneticSolver.currentlyEvaluatedCandidate) + 1) +
                    " / " + m_GeneticSolver.currentGeneticEquation.currentGeneration.candidates.Count +
                    ":\n";

            var sortedChromosomes =
                m_GeneticSolver.currentlyEvaluatedCandidate.chromosomes.
                OrderBy(chromosome => chromosome.name).ToList();

            foreach (var chromosome in sortedChromosomes)
            {
                if (chromosome.inherited)
                    m_Text.text += "<color=#3333FFFF>";

                m_Text.text += chromosome.name + " = " + chromosome.value + "\n";

                if (chromosome.inherited)
                    m_Text.text += "</color>";
            }
        }
    }
}
