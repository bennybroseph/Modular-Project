using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Library;
using UnityEditor;
using UnityEngine;

namespace DynamicStateMachine
{
	[ExecuteInEditMode]
	public class MonoDSM : MonoBehaviour, ISerializationCallbackReceiver
	{
	    /// <summary>
	    /// Delegate that will be used to determine if a state change is valid by the user.
	    /// Returns true or false and takes no parameters
	    /// </summary>
	    /// <returns> Whether or not the transition is valid based on the user's specification </returns>
	    public delegate bool IsValidCheck();

	    [SerializeField]
		private DSMObject m_DSMObject;

	    private string m_CurrentState;

	    private Dictionary<string, IsValidCheck> m_TransitionsWithChecks = new Dictionary<string, IsValidCheck>();
	    [SerializeField]
	    private List<string> m_Keys = new List<string>();
	    [SerializeField]
	    private List<IsValidTransition> m_Values = new List<IsValidTransition>();

	    public DSMObject dsmObject
	    {
	        get { return m_DSMObject; }
	    }

	    public string currentState
	    {
	        get { return m_CurrentState; }
	        private set { m_CurrentState = value; }
	    }

	    [Serializable]
	    public class IsValidTransition
	    {
	        [SerializeField]
	        private Component m_CallbackObject;
	        [SerializeField]
	        private int m_CallbackObjectID;
	        [SerializeField]
	        private string m_CallbackObjectType;
	        [SerializeField]
	        private string m_CallbackMethodName;

	        public IsValidTransition()
	        {
	            m_CallbackObject = null;
	            m_CallbackObjectID = -1;
	            m_CallbackObjectType = null;
	            m_CallbackMethodName = null;
	        }
	        public IsValidTransition(Component a_CallbackObject, MethodInfo a_CallbackMethodInfo)
	        {
	            m_CallbackObject = a_CallbackObject;
	            m_CallbackObjectID = m_CallbackObject.GetInstanceID();
	            m_CallbackObjectType = m_CallbackObject.GetType().FullName;
	            m_CallbackMethodName = a_CallbackMethodInfo.Name;
	        }
	        public IsValidTransition(Type a_CallbackObjectType, MethodInfo a_CallbackMethodInfo)
	        {
	            m_CallbackObject = null;
	            m_CallbackObjectID = -1;
	            m_CallbackObjectType = a_CallbackObjectType.FullName;
	            m_CallbackMethodName = a_CallbackMethodInfo.Name;
	        }

	        public static bool IsValidMethodSignature(MethodInfo a_CallMethodInfo)
	        {
	            if (a_CallMethodInfo.ReturnType != typeof(bool))
	                return false;
	            if (a_CallMethodInfo.GetParameters().Length != 0)
	                return false;

	            return true;
	        }

	        public IsValidCheck CreateIsValidCheck()
	        {
	            if (string.IsNullOrEmpty(m_CallbackMethodName))
	                return () => true;

	            if (m_CallbackObjectID == -1)
	                return (IsValidCheck)Delegate.CreateDelegate(
	                    typeof(IsValidCheck),
	                    Type.GetType(m_CallbackObjectType).GetMethod(m_CallbackMethodName));

	            if (m_CallbackObject == null)
	                m_CallbackObject = EditorUtility.InstanceIDToObject(m_CallbackObjectID) as Component;

	            return (IsValidCheck)Delegate.CreateDelegate(
	                typeof(IsValidCheck),
	                m_CallbackObject,
	                Type.GetType(m_CallbackObjectType).GetMethod(m_CallbackMethodName));
	        }

	        public IsValidCheck DeSerialize()
	        {
	            return CreateIsValidCheck();
	        }
	    }

	    void Awake()
	    {
	        
	    }
	    // Use this for initialization
	    void Start()
	    {
	        
	    }

	    // Update is called once per frame
	    void Update()
	    {

	    }

	    public bool AddTransition(string a_From, string a_To, IsValidCheck a_IsValidCheck)
	    {
	        //string transition = DynamicFSM.CreateKey(a_From, a_To);

	        //if (!m_ScriptableFSM.dynamicFSM.transitions.Contains(transition))
	        //    return false;

	        //m_TransitionsWithChecks[transition] = a_IsValidCheck;
	        return true;
	    }

	    private bool OnCheckTransition(string a_Transition)
	    {
	        if (!m_TransitionsWithChecks.ContainsKey(a_Transition))
	            return true;

	        return m_TransitionsWithChecks[a_Transition]();
	    }
	    private void OnTransitionChanged(string a_OldKey, string a_NewKey)
	    {
	        if (!m_TransitionsWithChecks.ContainsKey(a_OldKey))
	            return;

	        var value = m_TransitionsWithChecks[a_OldKey];

	        m_TransitionsWithChecks.Remove(a_OldKey);

	        if (!string.IsNullOrEmpty(a_NewKey))
	            m_TransitionsWithChecks[a_NewKey] = value;
	    }

	    public bool MethodCheck()
	    {
	        Debug.Log("Method Transition Check Successful");
	        return true;
	    }

	    public static bool StaticCheck()
	    {
	        Debug.Log("Static Transition Check Successful");
	        return true;
	    }

	    public void OnBeforeSerialize()
	    {
	        m_Keys.Clear();
	        m_Values.Clear();

	        foreach (var keyValuePair in m_TransitionsWithChecks)
	        {
	            m_Keys.Add(keyValuePair.Key);

	            var newIsValidTransition = new IsValidTransition();
	            if (keyValuePair.Value.Target != null)
	            {
	                var targetAsObject = keyValuePair.Value.Target as Component;

	                newIsValidTransition = targetAsObject != null
	                    ? new IsValidTransition(targetAsObject, keyValuePair.Value.Method)
	                    : new IsValidTransition(keyValuePair.Value.Target.GetType(), keyValuePair.Value.Method);
	            }
	            else if (keyValuePair.Value.Method.DeclaringType != null &&
	                     !keyValuePair.Value.Method.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any())
	            {
	                newIsValidTransition =
	                    new IsValidTransition(keyValuePair.Value.Method.DeclaringType, keyValuePair.Value.Method);
	            }

	            m_Values.Add(newIsValidTransition);
	        }
	    }
	    public void OnAfterDeserialize()
	    {
	        for (int i = 0; i < m_Keys.Count; ++i)
	        {
	            //try
	            //{
	                m_TransitionsWithChecks[m_Keys[i]] = m_Values[i].DeSerialize();
	            //}
	            //catch (ArgumentException exception)
	           // {
	            //    Debug.LogWarning("Argument Exception: " + exception.Message);
	            //}
	        }
	    }
	}
}