using System;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicStateMachine
{
	public interface IMonoDSM
	{
		IDSMObject dsmObject { get; }
		IDSMState currentState { get; }

		void Transition(string a_From, string a_To);
	}

	public interface IDSMObject
	{
		IDSMState entryPoint { get; set; }
		List<IDSMState> states { get; }

		bool AddState (string a_DisplayName = "New State", Vector2 a_Position = default(Vector2));
		void RemoveState (IDSMState a_State);
	}
	public interface IDrawableDSMObject : IDSMObject
	{
		List<IDrawableDSMState> drawableStates { get; }
	}

	public enum StateAttribute
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

	public interface IDSMState
	{
		string displayName { get; }
		string tag { get; }

		List<IDSMTransition> transitions { get; }

		StateAttribute attribute { get; }
		AllowedTransitionType allowedTransitionTypes { get; }
		int maxTransitions { get; }

		IDSMTransition AddTransition (IDSMState a_Other);
		bool RemoveTransition (IDSMTransition a_Transition);
	}
	public interface IDrawableDSMState : IDSMState
	{
		Vector2 position { get; set; }

	}

	public static class IDSMExtensions
	{
		public static bool IsAllowedTransition(this IDSMState a_From, IDSMState a_To)
		{
			if (a_From.maxTransitions != -1 &&
				a_From.transitions.Count >= a_From.maxTransitions)
				return false;

			if (a_To.maxTransitions != -1 &&
				a_To.transitions.Count >= a_To.maxTransitions) 
				return false;

			if ((a_To.allowedTransitionTypes & AllowedTransitionType.To) == 0)
				return false;

			if ((a_From.allowedTransitionTypes & AllowedTransitionType.From) == 0)
				return false;

			switch (a_From.attribute) 
			{
			case StateAttribute.Exit:
			case StateAttribute.FromAny:
			case StateAttribute.Entry:
				{
					if ((a_To.allowedTransitionTypes & AllowedTransitionType.FromSpecial) == 0)
						return false;					
				}
				break;
			}

			switch (a_To.attribute) 
			{
			case StateAttribute.Exit:
			case StateAttribute.FromAny:
			case StateAttribute.Entry:
				{
					if ((a_From.allowedTransitionTypes & AllowedTransitionType.ToSpecial) == 0)
						return false;
				}
				break;
			}

			return true;
   		}
	}

	[Serializable]
	public class TransitionStates
	{
		public IDSMState fromState;
		public IDSMState toState;
	}

	public interface IDSMTransition
	{
		string displayName { get; }

		TransitionStates states { get; }
  	}
}

