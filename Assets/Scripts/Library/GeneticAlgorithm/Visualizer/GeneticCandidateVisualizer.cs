using UnityEngine;
using UnityEngine.UI;

namespace Library.GeneticAlgorithm.Visualizer
{
    [RequireComponent(typeof(Text))]
    public class GeneticCandidateVisualizer : MonoBehaviour
    {
        [SerializeField]
        private Candidate m_Candidate;

        private Text m_Text;

        public Candidate candidate
        {
            get { return m_Candidate; }
            set { m_Candidate = value; }
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
            m_Text.text = string.Empty;
            foreach (var chromosome in m_Candidate.chromosomes)
                m_Text.text += chromosome.name + " = " + chromosome.value + "\n";
        }
    }
}
