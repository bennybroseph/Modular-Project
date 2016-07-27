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

		private void OnValidate()
		{
			if (m_DSMObject == null)
				return;

			m_CurrentState = m_DSMObject.entryPoint as DSMState;
			m_DSMObject.entryPointChanged.AddListener (OnEntryPointChanged);
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
				Transition ("Walk");

			if (Input.GetAxisRaw ("Vertical") != 0)
				Transition ("Idle");

			if (Input.GetAxisRaw ("Jump") != 0)
				Transition ("Run");
		}

		public bool Transition(string a_To)
		{
			if (m_DSMObject == null)
				return false;

			DSMTransition transition = m_CurrentState.transitions.Find (x => x.states.toState.displayName == a_To) as DSMTransition;
			if (transition == null)
				return false;

			var newCurrentState = transition.states.toState as DSMState;
			if (newCurrentState == null)
				return false;

			m_CurrentState = newCurrentState;
			return true;
		}

		private void OnEntryPointChanged()
		{
			m_CurrentState = m_DSMObject.entryPoint as DSMState;
		}	    
	}
}