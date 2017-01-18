using UnityEngine;

namespace Library.GeneticAlgorithm.Visualizer
{
    public class GeneticSolverVisualizer : MonoBehaviour
    {
        [SerializeField]
        private GeneticSolver m_GeneticSolver;

        [Space, SerializeField]
        private GameObject m_EquationVisualizerPrefab;

        // Use this for initialization
        private void Start()
        {
            foreach (var geneticEquation in m_GeneticSolver.geneticEquations)
            {
                var newEquationVisualizer =
                    Instantiate(m_EquationVisualizerPrefab).AddComponent<GeneticEquationVisualizer>();
                newEquationVisualizer.geneticEquation = geneticEquation;

                newEquationVisualizer.transform.SetParent(transform);
            }
        }

        // Update is called once per frame
        private void Update()
        {

        }
    }
}
