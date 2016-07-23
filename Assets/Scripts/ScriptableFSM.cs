using System;
using Library;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

[Serializable]
[CreateAssetMenu(fileName = "NewFSM", menuName = "Scriptable FSM")]
public class ScriptableFSM : ScriptableObject
{
    public DynamicFSM dynamicFSM;
    public List<Vector2> windowPositions;

    [SerializeField]
    private List<string> m_Keys;
    [SerializeField]
    private List<IsValidTransition> m_Values;

    [Serializable]
    public class IsValidTransition
    {
        [SerializeField]
        private Object m_CallbackObject;
        [SerializeField]
        private string m_CallbackMethodName;

        public IsValidTransition(Object a_CallbackObject, MethodInfo a_CallbackMethod)
        {
            m_CallbackObject = a_CallbackObject;

            m_CallbackMethodName = a_CallbackMethod.Name;
        }
        public IsValidTransition()
        {
            
        }

        public bool IsValidMethodSignature()
        {
            if (m_CallbackMethod.ReturnType != typeof(bool))
                return false;
            if (m_CallbackMethod.GetParameters().Length != 0)
                return false;

            return true;
        }

        public DynamicFSM.IsValidCheck CreateIsValidCheck()
        {
            if (!IsValidMethodSignature())
                return null;

            return
                (DynamicFSM.IsValidCheck)Delegate.CreateDelegate(
                    typeof(DynamicFSM.IsValidCheck),
                    m_CallbackObject,
                    m_CallbackMethod);
        }

        public DynamicFSM.IsValidCheck DeSerialize()
        {
            DynamicFSM.IsValidCheck parsedDelegate = () => true;

            if (!string.IsNullOrEmpty(m_CallbackObjectName))
            {
                foreach (var foundObject in FindObjectsOfType(Type.GetType(m_CallbackObjectType)))
                {
                    if (foundObject.name == m_CallbackObjectName)
                        parsedDelegate =
                            (DynamicFSM.IsValidCheck)Delegate.CreateDelegate(
                                typeof(DynamicFSM.IsValidCheck),
                                foundObject,
                                Type.GetType(m_CallbackObjectType).GetMethod(m_CallbackMethodName));
                }
            }
            return parsedDelegate;
        }
    }

    public void OnEnable()
    {
        for (int i = 0; i < m_Keys.Count; ++i)
        {
            string[] states = DynamicFSM.ParseStates(m_Keys[i]);

            dynamicFSM.AddTransition(states[0], states[1], m_Values[i].DeSerialize());
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Destroyed " + name);
    }

    public void OnDisable()
    {
        AssetDatabase.Refresh();

        m_Keys = new List<string>();
        m_Values = new List<IsValidTransition>();

        foreach (var keyValuePair in dynamicFSM.transitions)
        {
            m_Keys.Add(keyValuePair.Key);
            m_Values.Add(new IsValidTransition(keyValuePair.Value.Target as Object, keyValuePair.Value.Method));
        }
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
}
