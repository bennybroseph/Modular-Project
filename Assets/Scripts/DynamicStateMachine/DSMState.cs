using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace DynamicStateMachine
{
	[Serializable]
	public class DSMState : ScriptableObject
	{
	    public enum Attribute
	    {
	        None,
	        Entry,
	        FromAny,
	        Exit,
	    }
		[Flags]
		public enum AllowedTransitionType
		{
			None = 0,
			To = 1 << 0,
			From = 1 << 1,
			ToSpecial = 1 << 2,
			FromSpecial = 1 << 3,
			All = ~0,
		}

		[HideInInspector]
	    public Vector2 position;
		[HideInInspector]
	    public string tag;

		[SerializeField, HideInInspector]
	    private string m_DisplayName;
		[SerializeField, HideInInspector]
	    private List<DSMTransition> m_FromTransitions = new List<DSMTransition>();
		[SerializeField, HideInInspector]
		private List<DSMTransition> m_ToTransitions = new List<DSMTransition>();

	    [SerializeField, HideInInspector]
	    private DSMObject m_Parent;

		[SerializeField, HideInInspector]
	    private Attribute m_Attribute;
		[SerializeField]
		private AllowedTransitionType m_AllowedTransitionTypes;
		[SerializeField]
		private int m_MaxTransitions;

	    public string displayName
	    {
	        get { return m_DisplayName; }
	    }
	    public List<DSMTransition> transitions
	    {
	        get { return m_FromTransitions; }
	    }

	    public Attribute attribute
	    {
	        get { return m_Attribute; }
	    }
		public AllowedTransitionType allowedTransitionTypes 
		{
			get{ return m_AllowedTransitionTypes; }
		}

		public DSMTransition AddFromTransition(DSMState a_Other)
	    {
			if(!IsAllowedTransition(this, a_Other))
				return null;

	        var newTransition = m_Parent.AddChildAsset<DSMTransition>();

			newTransition.Init (m_Parent, this, a_Other);

			a_Other.AddToTransition (newTransition);
			
	        m_FromTransitions.Add(newTransition);
					
			return newTransition;
	    }
		public void AddToTransition(DSMTransition a_Transition)
		{
			if (a_Transition.state.fromState.attribute == Attribute.Entry)
				m_Parent.m_EntryPoint = this;
			
			m_ToTransitions.Add(a_Transition);
		}

	    public bool RemoveTransition(DSMTransition a_Transition)
	    {
			if (!EditorUtility.DisplayDialog (
				    "Warning!", 
				    "Are you sure you want to delete this transition?", "Yes", "No"))
				return false;
			
	        DestroyImmediate(a_Transition, true);
	        return true;
	    }

		public static bool IsAllowedTransition(DSMState a_From, DSMState a_To)
		{
			if (a_From.m_MaxTransitions != -1 &&
			   (a_From.m_FromTransitions.Count >= a_From.m_MaxTransitions ||
				a_From.m_ToTransitions.Count >= a_From.m_MaxTransitions))
				return false;

			if (a_To.m_MaxTransitions != -1 &&
			   (a_To.m_FromTransitions.Count >= a_To.m_MaxTransitions ||
				a_To.m_ToTransitions.Count >= a_To.m_MaxTransitions)) 
				return false;

			if ((a_To.m_AllowedTransitionTypes & AllowedTransitionType.To) == 0)
				return false;

			if ((a_From.m_AllowedTransitionTypes & AllowedTransitionType.From) == 0)
				return false;
			
			switch (a_From.m_Attribute) 
			{
			case Attribute.Exit:
			case Attribute.FromAny:
			case Attribute.Entry:
				{
					if ((a_To.m_AllowedTransitionTypes & AllowedTransitionType.FromSpecial) == 0)
						return false;
				}
				break;
			}

			return true;
		}

		public void Init(
			DSMObject a_Parent,
			string a_DisplayName, 
			Vector2 a_Position, 
			Attribute a_Attribute, 
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
			List<DSMTransition> tempList = m_FromTransitions.ToList();
			foreach (var transition in tempList)
				transition.OnStateDestroyed();

			tempList = m_ToTransitions.ToList();
			foreach (var transition in tempList)
				transition.OnStateDestroyed();

			m_Parent.OnStateDestroyed (this);

			if (Selection.activeObject == this)
				Selection.activeObject = null;
			
			if (m_Parent.m_EntryPoint == this)
				m_Parent.m_EntryPoint = null;
	    }

	    public void OnTransitionDestroyed(DSMTransition a_DSMTransition)
	    {
			if(m_FromTransitions.Contains(a_DSMTransition))
	        	m_FromTransitions.Remove(a_DSMTransition);

			if(m_ToTransitions.Contains(a_DSMTransition))
				m_ToTransitions.Remove(a_DSMTransition);
	    }
	}
}
