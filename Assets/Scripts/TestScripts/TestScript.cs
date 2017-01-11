using Library;

using UnityEngine;
using UnityEngine.Events;

public class TestScript : MonoBehaviour
{
    public AutoPropertyString m_TestValueString;

    public AutoPropertyInt m_TestValueInt;
    public AutoPropertyFloat m_TestValueFloat;

    public AutoPropertyVector2 m_TestValueVector2;
    public AutoPropertyVector3 m_TestValueVector3;


    // Use this for initialization
    private void Awake()
    {
        //m_TestValueString.onChangeEvent.AddListener(OnTestValueChanged);

        //m_TestValueInt.onChangeEvent.AddListener(OnTestValueChanged);
        //m_TestValueFloat.onChangeEvent.AddListener(OnTestValueChanged);

        //m_TestValueVector2.onChangeEvent.AddListener(OnTestValueChanged);
        //m_TestValueVector3.onChangeEvent.AddListener(OnTestValueChanged);
    }

    private static void OnTestValueChanged<T>(T newValue)
    {
        Debug.Log(newValue);
    }

    private void OnTestValueChanged()
    {
        Debug.Log(m_TestValueVector3.newValue);
    }
}
