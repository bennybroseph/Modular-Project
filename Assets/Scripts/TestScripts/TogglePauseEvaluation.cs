namespace TestScripts
{
    using Library;

    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Toggle))]
    public class TogglePauseEvaluation : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_Toggle;
        [SerializeField]
        private Text m_Text;

        // Use this for initialization
        private void Awake()
        {
            m_Text = GetComponentInChildren<Text>();
            m_Text.text = "Pause Evaluation";

            m_Toggle = GetComponent<Toggle>();
            m_Toggle.isOn = Expression.yieldEvaluation;

            m_Toggle.onValueChanged.AddListener(
                newValue => { Expression.pauseEvaluation = newValue; });

            Expression.onPauseChanged += newValue => m_Toggle.isOn = newValue;
        }
    }
}
