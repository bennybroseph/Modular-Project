namespace TestScripts
{
    using System;
    using System.Linq;

    using Library.GeneticAlgorithm;

    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Dropdown))]
    public class PopulationGenerationDropdown : MonoBehaviour
    {
        [SerializeField]
        private Dropdown m_Dropdown;

        private GeneticSolver m_GeneticSolver;

        // Use this for initialization
        private void Start()
        {
            m_GeneticSolver = FindObjectOfType<GeneticSolver>();

            if (m_GeneticSolver == null)
                gameObject.SetActive(false);

            m_Dropdown = GetComponent<Dropdown>();

            m_Dropdown.ClearOptions();
            m_Dropdown.AddOptions(Enum.GetNames(typeof(GeneticSolver.PopulationGenerationType)).ToList());

            m_Dropdown.value = (int)m_GeneticSolver.populationGenerationType;

            m_Dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        private void OnDropdownValueChanged(int newValue)
        {
            m_GeneticSolver.populationGenerationType = (GeneticSolver.PopulationGenerationType)newValue;
        }
    }
}
