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

                OnChange(value);
            }
        }

#if UNITY_5
        protected abstract void OnChange(T newValue);
#else
        private void OnChange(T newValue)
        {
            if (onChangeEvent != null)
                onChangeEvent.Invoke(newValue);
        }
#endif
    }

    // Useful for serialization purposes in Unity
#if UNITY_5
    [Serializable]
    public sealed class AutoPropertyString : AutoProperty<string>
    {
        [Serializable]
        public class UnityEventString : UnityEvent<string> { }

        public new UnityEventString onChangeEvent = new UnityEventString();

        protected override void OnChange(string newValue)
        {
            if (onChangeEvent != null)
                onChangeEvent.Invoke(newValue);
        }
    }

    [Serializable]
    public class AutoPropertyInt : AutoProperty<int>
    {
        [Serializable]
        public class UnityEventInt : UnityEvent<int> { }

        public new UnityEventInt onChangeEvent = new UnityEventInt();

        protected override void OnChange(int newValue)
        {
            if (onChangeEvent != null)
                onChangeEvent.Invoke(newValue);
        }
    }
    [Serializable]
    public class AutoPropertyFloat : AutoProperty<float>
    {
        [Serializable]
        public class UnityEventFloat : UnityEvent<float> { }

        public new UnityEventFloat onChangeEvent = new UnityEventFloat();

        protected override void OnChange(float newValue)
        {
            if (onChangeEvent != null)
                onChangeEvent.Invoke(newValue);
        }
    }

    [Serializable]
    public class AutoPropertyVector2 : AutoProperty<Vector2>
    {
        [Serializable]
        public class UnityEventVector2 : UnityEvent<Vector2> { }

        public new UnityEventVector2 onChangeEvent = new UnityEventVector2();

        protected override void OnChange(Vector2 newValue)
        {
            if (onChangeEvent != null)
                onChangeEvent.Invoke(newValue);
        }
    }

    [Serializable]
    public class AutoPropertyVector3 : AutoProperty<Vector3>
    {
        [Serializable]
        public class UnityEventVector3 : UnityEvent<Vector3> { }

        public new UnityEventVector3 onChangeEvent = new UnityEventVector3();

        protected override void OnChange(Vector3 newValue)
        {
            if (onChangeEvent != null)
                onChangeEvent.Invoke(newValue);
        }
    }
#endif
}
