using UnityEngine;
using UnityEngine.UI;

public class UITestScript : MonoBehaviour
{
    [SerializeField]
    private Text m_Text;

    private void Awake()
    {
        m_Text = GetComponent<Text>();
    }

    public void OnValueChanged()
    {
        Debug.Log("Change");
    }
}
