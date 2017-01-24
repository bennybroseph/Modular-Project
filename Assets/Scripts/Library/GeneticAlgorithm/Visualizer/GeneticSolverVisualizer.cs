namespace Library.GeneticAlgorithm.Visualizer
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class GeneticSolverVisualizer : MonoBehaviour
    {
        [SerializeField]
        private GeneticSolver m_GeneticSolver;

        [Space, SerializeField]
        private GameObject m_EquationVisualizerPrefab;

        private List<HorizontalOrVerticalLayoutGroup> m_LayoutGroups;

        private List<GeneticCandidateVisualizer> m_CandidateVisualizers =
            new List<GeneticCandidateVisualizer>();

        private void Awake()
        {
            m_LayoutGroups = GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>().ToList();
        }

        // Use this for initialization
        private void Start()
        {
            var newEquationVisualizer =
                Instantiate(m_EquationVisualizerPrefab).AddComponent<GeneticEquationVisualizer>();
            newEquationVisualizer.geneticSolver = m_GeneticSolver;

            newEquationVisualizer.transform.SetParent(m_LayoutGroups.First().transform);
            newEquationVisualizer.transform.SetAsFirstSibling();

            var candidateVisualizer =
                Instantiate(m_EquationVisualizerPrefab).AddComponent<GeneticCandidateVisualizer>();
            candidateVisualizer.geneticSolver = m_GeneticSolver;

            candidateVisualizer.transform.SetParent(m_LayoutGroups.Last().transform);
            candidateVisualizer.transform.SetAsFirstSibling();

            m_CandidateVisualizers.Add(candidateVisualizer);

            var expressionSolverVisualizer =
                Instantiate(m_EquationVisualizerPrefab).AddComponent<ExpressionSolverVisualizer>();

            expressionSolverVisualizer.transform.SetParent(m_LayoutGroups.First().transform);
            expressionSolverVisualizer.transform.SetAsFirstSibling();

            expressionSolverVisualizer.geneticSolver = m_GeneticSolver;
        }
    }
}
