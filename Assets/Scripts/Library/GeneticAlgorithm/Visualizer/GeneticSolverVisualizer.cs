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
            newEquationVisualizer.geneticEquation = m_GeneticSolver.geneticEquation;

            newEquationVisualizer.transform.SetParent(m_LayoutGroups.First().transform);

            for (var i = 0; i < 2; ++i)
            {
                var candidateVisualizer = new GameObject().AddComponent<GeneticCandidateVisualizer>();
                candidateVisualizer.transform.SetParent(m_LayoutGroups.Last().transform);

                m_CandidateVisualizers.Add(candidateVisualizer);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            for (var i = 0; i < m_CandidateVisualizers.Count; ++i)
            {
                m_CandidateVisualizers[i].candidate =
                    m_GeneticSolver.geneticEquation.generations.Last().candidates[i];
            }
        }
    }
}
