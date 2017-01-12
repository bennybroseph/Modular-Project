using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using Object = UnityEngine.Object;

namespace DynamicStateMachine
{
	[Serializable]
	public class DSMState : ScriptableObject, IDrawableDSMState
	{
		#region PRIVATE FIELDS
		[SerializeField, HideInInspector]
	    private Vector2 m_Position;
		[SerializeField, HideInInspector]
		private string m_Tag;

		[SerializeField, HideInInspector]
	    private string m_DisplayName;
		[SerializeField, HideInInspector]
	    private List<DSMTransition> m_FromTransitions = new List<DSMTransition>();
		[SerializeField, HideInInspector]
		private List<DSMTransition> m_ToTransitions = new List<DSMTransition>();

	    [SerializeField, HideInInspector]
	    private DSMObject m_Parent;

		[SerializeField, HideInInspector]
	    private StateAttribute m_Attribute;
		[SerializeField]
		private AllowedTransitionType m_AllowedTransitionTypes;
		[SerializeField]
		private int m_MaxTransitions;
		#endregion

		#region PROPERTIES
		public Vector2 position
		{
			get { return m_Position; }
			set { m_Position = value; }
		}

	    public string displayName
	    {
	        get { return m_DisplayName; }
	    }
		public string tag
		{
			get { return m_Tag; }
		}

	    public List<IDSMTransition> transitions
	    {
			get 
			{
				var newList = new List<IDSMTransition>();
				foreach (var state in m_FromTransitions)
					newList.Add (state as IDSMTransition);

				return newList; 
			}
	    }

		public StateAttribute attribute
	    {
	        get { return m_Attribute; }
	    }
		public AllowedTransitionType allowedTransitionTypes 
		{
			get{ return m_AllowedTransitionTypes; }
		}
		public int maxTransitions 
		{
			get { return m_MaxTransitions; }
		}
		#endregion

		public IDSMTransition AddTransition(IDSMState a_Other)
	    {
			if(!this.IsAllowedTransition(a_Other))
				return null;

			var newTransition = m_Parent.AddChildAsset<DSMTransition> ();
			newTransition.Init (m_Parent, this, a_Other as DSMState);

			var other = a_Other as DSMState;
			other.AddToTransition (newTransition);
			
	        m_FromTransitions.Add(newTransition);
					
			return newTransition;
	    }
		public void AddToTransition(DSMTransition a_Transition)
		{
			if (a_Transition.states.fromState.attribute == StateAttribute.Entry)
				m_Parent.entryPoint = this;
			
			m_ToTransitions.Add(a_Transition);
		}

	    public bool RemoveTransition(IDSMTransition a_Transition)
	    {
			if (!EditorUtility.DisplayDialog (
				    "Warning!", 
				    "Are you sure you want to delete this transition?", "Yes", "No"))
				return false;
			
			DestroyImmediate(a_Transition as Object, true);
	        return true;
	    }

		public void Init(
			DSMObject a_Parent,
			string a_DisplayName, 
			Vector2 a_Position, 
			StateAttribute a_Attribute, 
			AllowedTransitionType a_AllowedTransitions = AllowedTransitionType.All,
			int a_MaxTransitions = -1)
	    {
			m_Parent = a_Parent;

	        name = a_DisplayName;
	        m_DisplayName = a_DisplayName;

	        position = a_Position;

	        m_Attribute = a_Attribute;
			m_AllowedTransitionTypes = a_AllowedTransitions;

			m_MaxTransitions = a_MaxTransitions;
	    }

	    private void OnDestroy()
	    {
			var tempFromList = m_FromTransitions.ToList();
			foreach (var transition in tempFromList)
				transition.OnStateDestroyed();

			var tempToList = m_ToTransitions.ToList();
			foreach (var transition in tempToList)
				transition.OnStateDestroyed();

			m_Parent.OnStateDestroyed (this);

			if (Selection.activeObject == this)
				Selection.activeObject = null;
			
			if (m_Parent.entryPoint == this)
				m_Parent.entryPoint = null;
	    }

	    public void OnTransitionDestroyed(DSMTransition a_DSMTransition)
	    {
			if(m_FromTransitions.Contains(a_DSMTransition))
				m_FromTransitions.Remove(a_DSMTransition);

			if(m_ToTransitions.Contains(a_DSMTransition))
				m_ToTransitions.Remove(a_DSMTransition);
	    }

		public DSMState Clone()
		{
			var newState = MemberwiseClone () as DSMState;

			newState.m_FromTransitions = new List<DSMTransition>();
			foreach (var transition in m_FromTransitions)
				newState.m_FromTransitions.Add (transition.Clone (newState));

			return newState;
		}
	}
}
