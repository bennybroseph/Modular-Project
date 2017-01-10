using UnityEngine;
using UnityEngine.Events;

public class AutoProperty<T>
{
#if UNITY_5
    [SerializeField]
#endif
    private T m_Data;

#if UNITY_5
    [SerializeField]
    public UnityEvent<T> onChangeEvent;
#else
    public delegate void OnChangeDelegate(T value);

    public event OnChangeDelegate onChangeEvent;
#endif

    public T value
    {
        get { return m_Data; }
        set
        {
            m_Data = value;

            if (onChangeEvent != null)
                onChangeEvent.Invoke(value);
        }
    }
}
