namespace TestScripts
{
    using Library;

    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Toggle))]
    public class ToggleYieldEvaluation : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_Toggle;
        [SerializeField]
        private Text m_Text;

        // Use this for initialization
        private void Awake()
        {
            m_Text = GetComponentInChildren<Text>();
            m_Text.text = "Yield Evaluation";

            m_Toggle = GetComponent<Toggle>();
            m_Toggle.isOn = Expression.yieldEvaluation;

            m_Toggle.onValueChanged.AddListener(
                newValue => { Expression.yieldEvaluation = newValue; });
        }
    }
}
