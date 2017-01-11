namespace Library
{
    using System;

#if UNITY_5
    using UnityEngine;
    using UnityEngine.Events;
#endif

    [Serializable]
    public class AutoProperty<T>
    {
#if UNITY_5
        [SerializeField]
#endif
        private T m_Data;

#if UNITY_5
        [HideInInspector]
        public T newValue;

        [Serializable]
        public class UnityEventGeneric : UnityEvent<T> { }

        public UnityEventGeneric onChangeEvent = new UnityEventGeneric();
#else
        public delegate void OnChangeDelegate(T value);

        public event OnChangeDelegate onChangeEvent;
#endif

        public T value
        {
            get { return m_Data; }
            set
            {
#if UNITY_5
                newValue = value;
                m_Data = newValue;

                if (onChangeEvent != null)
                    onChangeEvent.Invoke(value);
#else
                if (onChangeEvent != null)
                    onChangeEvent.Invoke(value);
#endif
            }
        }
    }

    // Useful for serialization purposes in Unity
#if UNITY_5
    [Serializable]
    public class AutoPropertyString : AutoProperty<string> { }

    [Serializable]
    public class AutoPropertyInt : AutoProperty<int> { }
    [Serializable]
    public class AutoPropertyFloat : AutoProperty<float> { }

    [Serializable]
    public class AutoPropertyVector2 : AutoProperty<Vector2> { }
    [Serializable]
    public class AutoPropertyVector3 : AutoProperty<Vector3> { }
#endif
}
