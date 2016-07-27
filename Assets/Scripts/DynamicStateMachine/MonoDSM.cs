using UnityEngine;
using System;
using System.Collections.Generic;
using Library;

namespace DynamicStateMachine
{
	[ExecuteInEditMode]
	public class MonoDSM : ExecuteInEditor, IMonoDSM
	{
	    [SerializeField]
		private DSMObject m_DSMObject;
		[SerializeField]
		private DSMState m_CurrentState;	    

		public IDSMObject dsmObject
	    {
			get { return m_DSMObject as IDSMObject; }
	    }

		public IDSMState currentState
	    {
			get { return m_CurrentState as IDSMState; }
	    }	    

		protected override void OnEditorStart()
		{
			if (m_DSMObject == null)
				return;

			m_CurrentState = m_DSMObject.entryPoint as DSMState;
			m_DSMObject.entryPointChanged.AddListener (OnEntryPointChanged);
		}
		protected override void OnEditorUpdateSelected()
		{
			
		}
		protected override void OnEditorUpdate()
		{
			
		}

		protected override void OnGameStart()
		{
			
		}
		protected override void OnGameUpdate()
		{
			if (Input.GetAxisRaw ("Horizontal") != 0)
				Transition ("Walk", "Idle");

			if (Input.GetAxisRaw ("Vertical") != 0)
				Transition ("Idle", "Walk");
		}

		public void Transition(string a_From, string a_To)
		{
			if (m_DSMObject == null)
				return;
			
			DSMState state = m_DSMObject.states.Find (x => x.displayName == a_From) as DSMState;
			if (state == null)
				return;

			DSMTransition transition = state.transitions.Find (x => x.states.toState.displayName == a_To) as DSMTransition;
			if (transition == null)
				return;

			m_CurrentState = transition.states.toState as DSMState;
		}

		private void OnEntryPointChanged()
		{
			m_CurrentState = m_DSMObject.entryPoint as DSMState;
		}	    
	}
}