using UnityEngine;
using UnityEngine.UI;

public class UITestScript : MonoBehaviour
{
    [SerializeField]
    private Text m_Text;

    private TestScript m_TestScript;

    private void Awake()
    {
        m_Text = GetComponent<Text>();

        m_TestScript = FindObjectOfType<TestScript>();
    }

    // Use this for initialization
    private void Start()
    {
        m_TestScript.m_TestValueInt.onChangeEvent.AddListener(OnValueChanged);
    }

    private void OnValueChanged<T>(T newValue)
    {
        m_Text.text = newValue.ToString();
    }
}
