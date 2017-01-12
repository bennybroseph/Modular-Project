namespace Library
{
    using System;

#if UNITY_5
    using UnityEngine;
    using UnityEngine.Events;
#endif

    [Serializable]
#if UNITY_5
    public abstract class AutoProperty<T>
#else
    public class AutoProperty<T>
#endif
    {
#if UNITY_5
        [SerializeField]
#endif
        private T m_Data;

#if UNITY_5
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
                m_Data = value;

                if (onChangeEvent != null)
                    onChangeEvent.Invoke(value);
            }
        }
    }

    // Useful for serialization purposes in Unity
#if UNITY_5
    [Serializable]
    public sealed class AutoPropertyString : AutoProperty<string>
    {
        [Serializable]
        public class UnityEventString : UnityEvent<string> { }
        public UnityEventString onChangeSubEvent = new UnityEventString();

        public AutoPropertyString() { onChangeEvent.AddListener(onChangeSubEvent.Invoke); }
    }

    [Serializable]
    public sealed class AutoPropertyInt : AutoProperty<int>
    {
        [Serializable]
        public class UnityEventInt : UnityEvent<int> { }
        public UnityEventInt onChangeSubEvent = new UnityEventInt();

        public AutoPropertyInt() { onChangeEvent.AddListener(onChangeSubEvent.Invoke); }
    }
    [Serializable]
    public sealed class AutoPropertyFloat : AutoProperty<float>
    {
        [Serializable]
        public class UnityEventFloat : UnityEvent<float> { }
        public UnityEventFloat onChangeSubEvent = new UnityEventFloat();

        public AutoPropertyFloat() { onChangeEvent.AddListener(onChangeSubEvent.Invoke); }
    }

    [Serializable]
    public sealed class AutoPropertyVector2 : AutoProperty<Vector2>
    {
        [Serializable]
        public class UnityEventVector2 : UnityEvent<Vector2> { }
        public UnityEventVector2 onChangeSubEvent = new UnityEventVector2();

        public AutoPropertyVector2() { onChangeEvent.AddListener(onChangeSubEvent.Invoke); }
    }

    [Serializable]
    public sealed class AutoPropertyVector3 : AutoProperty<Vector3>
    {
        [Serializable]
        public class UnityEventVector3 : UnityEvent<Vector3> { }
        public UnityEventVector3 onChangeSubEvent = new UnityEventVector3();

        public AutoPropertyVector3() { onChangeEvent.AddListener(onChangeSubEvent.Invoke); }
    }
#endif
}
