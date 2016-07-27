using System;
using UnityEngine;
using UnityEditor;

namespace DynamicStateMachine
{
	[Serializable]
	public class DSMTransition : ScriptableObject, IDSMTransition
	{
		[SerializeField, HideInInspector]
		private DSMObject m_Parent;

		[SerializeField, HideInInspector]
	    private string m_DisplayName;

		[SerializeField, HideInInspector]
		private DSMState m_From;
		[SerializeField, HideInInspector]
		private DSMState m_To;

	    public string displayName
	    {
	        get
	        {
	            return
	                string.IsNullOrEmpty(m_DisplayName) ?
						m_From.displayName + " -> " + m_To.displayName :
	                    m_DisplayName;
	        }
	    }

	    public TransitionStates states
	    {
			get 
			{ 
				return new TransitionStates
				{
					fromState = m_From as IDSMState,
					toState = m_To as IDSMState,
				}; 
			}
	    }

		public void Init(DSMObject a_Parent, DSMState a_From, DSMState a_To, string a_DisplayName = null)
	    {
			m_Parent = a_Parent;

	        m_DisplayName = a_DisplayName;

			m_From = a_From;
			m_To = a_To;
				
			name = m_From.displayName + " -> " + m_To.displayName;
	    }

	    private void OnDestroy()
	    {
			m_From.OnTransitionDestroyed (this);
			m_To.OnTransitionDestroyed (this);

			if (Selection.activeObject == this)
				Selection.activeObject = null;
			
			if (m_From.attribute == StateAttribute.Entry)
				m_Parent.entryPoint = null;
	    }

	    public void OnStateDestroyed()
	    {
	        DestroyImmediate(this, true);
	    }
	}
}