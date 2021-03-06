﻿using System;                       // Required for the type 'Enum'
using System.Collections.Generic;   // Required to use 'List<T>' and 'Dictionary<T, T>'
using System.Linq;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

using Debug = Library.Contextual.Debug;

namespace Library
{
    // Usage:
    // enum MyStates { Init, Idle };
    // FiniteStateMachine<MyStates> MyFSM = new FiniteStateMachine<MyStates>(OPTIONAL: MyStates.Idle);
    /// <summary>
    /// My Finite State Machine
    /// </summary>
    [Serializable]
    public sealed class DynamicFSM
    {
        public delegate bool TransitionEvent(string a_Transition);
        private event TransitionEvent transitionEvent;

        public delegate void TransitionChangedEvent(string a_OldKey, string a_NewKey);
        private event TransitionChangedEvent transitionChangedEvent;

#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
        private Type m_TestType;

#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
        private string m_CurrentState;

        /// <summary> Cached list of all states in the enumeration </summary>
#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
        private List<string> m_States;


        /// <summary> Dynamic dictionary of all transitions as dictated by the user </summary>
#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
        private List<string> m_Transitions;

        /// <summary>
        /// Dictionary which holds all of the transitions which are valid from any other state
        /// ex. Going from any state to 'Dead' would be a common use
        /// </summary>
#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
        private List<string> m_TransitionsFromAny;

        /// <summary>
        /// Read-Only property for the current state 'm_CurrentState'.
        /// Look at me. I'm the captain now.
        /// </summary>
        public string currentState
        {
            get { return m_CurrentState; }
            private set { m_CurrentState = value; }
        }

        public List<string> states
        {
            get { return m_States; }
        }
        public List<string> transitions
        {
            get { return m_Transitions; }
        }
        public List<string> transitionsFromAny
        {
            get { return m_TransitionsFromAny; }
        }

        /// <summary> Default constructor which will initialize the list and dictionary </summary>
        public DynamicFSM()
        {
            m_States = new List<string>();
            m_Transitions = new List<string>();
            m_TransitionsFromAny = new List<string>();
        }

        /// <summary>
        /// Parameterized constructor which allows a state other than 'm_States[0]' to initialize 'm_CurrentState'
        /// </summary>
        /// <param name="a_InitialState">Used as the current state 'm_CurrentState' on creation</param>
        public DynamicFSM(string a_InitialState) : this()
        {
            if (m_States.Contains(a_InitialState))
                currentState = a_InitialState;
        }

        public void AddState(string a_State = null, int a_Index = -1)
        {
            if (a_State == null)
                a_State = "State";

            string newState = a_State;

            int i = 0;
            while (m_States.Contains(newState))
            {
                newState = a_State + " " + i;
                ++i;
            }

            if (a_Index == -1)
                a_Index = m_States.Count;
            m_States.Insert(a_Index, newState);

            if (currentState == "")
                currentState = newState;
        }
        public void RenameState(string a_OldName, string a_NewName)
        {
            int index = m_States.FindIndex(x => x == a_OldName);
            m_States.Remove(a_OldName);

            if (currentState == a_OldName)
                currentState = "";
            AddState(a_NewName, index);

            for (int i = 0; i < m_Transitions.Count; ++i)
            {
                string[] parsedStates = ParseStates(m_Transitions[i]);

                if (parsedStates[0] == a_OldName)
                {
                    string newKey = CreateKey(a_NewName, parsedStates[1]);

                    if (transitionChangedEvent != null)
                        transitionChangedEvent(m_Transitions[i], newKey);

                    m_Transitions[i] = newKey;
                }
                if (parsedStates[1] == a_OldName)
                {
                    string newKey = CreateKey(parsedStates[0], a_NewName);

                    if (transitionChangedEvent != null)
                        transitionChangedEvent(m_Transitions[i], newKey);

                    m_Transitions[i] = newKey;
                }
            }
        }
        public bool RemoveState(string a_State)
        {
            if (!m_States.Contains(a_State))
                return false;

            m_States.Remove(a_State);

            if (currentState == a_State)
                currentState = "";

            var tempList = m_Transitions.ToList();
            foreach (string transition in tempList)
            {
                string[] parsedStates = ParseStates(transition);

                if (parsedStates[0] == a_State || parsedStates[1] == a_State)
                    RemoveTransition(transition);
            }
            return true;
        }

        /// <summary>
        /// Attempts to add a new transition to the current list of transitions
        /// </summary>
        /// <param name="a_From">The state to come from</param>
        /// <param name="a_To">The state to go to</param>
        /// <param name="a_IsValidTransition">An optional delegate with no parameters that returns true when 
        ///                                   the state change is valid and false when it is not</param>
        /// <returns>Returns true if the transition was able to be added and false otherwise</returns>
        public bool AddTransition(string a_From, string a_To)
        {
            // if 'a_From' and 'a_To' are the same state
            if (a_From.Equals(a_To))
            {
                Debug.Warning("'" + a_From + "'" + " is the same state as " + "'" + a_To + "'");
                return false;
            }

            // if 'a_From' or 'a_To' is not in the list of states
            if (!m_States.Contains(a_From) || !m_States.Contains(a_To))
            {
                string invalidState;   // Will decipher which state is invalid
                if (!m_States.Contains(a_From))
                    invalidState = a_From;
                else
                    invalidState = a_To;

                Debug.Warning("'" + invalidState + "' does not currently exist as a state");
                return false;
            }

            // Properly serializes 'a_From' and 'a_To' into the expected key format
            string transition = CreateKey(a_From, a_To);
            // if the key 'key' does not currently exist in 'm_Transitions'
            if (!m_Transitions.Contains(transition))
            {
                m_Transitions.Add(transition);
                return true;
            }
            else
            {
                Debug.Warning("'" + transition + "' already exists as a transition");
                return false;
            }
        }
        public bool RemoveTransition(string a_Transition)
        {
            if (!m_Transitions.Contains(a_Transition))
                return false;

            if (transitionChangedEvent != null)
                transitionChangedEvent(a_Transition, null);

            m_Transitions.Remove(a_Transition);
            return true;
        }

        /// <summary>
        /// Attempts to add a new transition to the current list which is able to be transitioned to 
        /// from any other state
        /// </summary>
        /// <param name="a_To">The state to transition to from any other state</param>
        /// <param name="a_IsValidateTransition">An optional delegate with no parameters that returns true when 
        ///                                      the state change is valid and false when it is not</param>
        /// <returns>Returns true if the transition was able to be added and false otherwise</returns>
        public bool AddTransitionFromAny(string a_To)
        {
            if (!m_States.Contains(a_To))
            {
                Debug.Warning("'" + a_To + "' does not currently exist as a state");
                return false;
            }

            if (!m_TransitionsFromAny.Contains(a_To))
            {
                m_TransitionsFromAny.Add(a_To);
                return true;
            }
            else
            {
                Debug.Warning("'" + a_To + "' already exists as a transition key");
                return false;
            }
        }

        /// <summary>
        /// Attempts to transition from the current state to the passed parameter
        /// </summary>
        /// <param name="a_To">The state to transition to</param>
        /// <returns>Returns true if the transition completed and false otherwise</returns>
        public bool Transition(string a_To)
        {
            // Converts the current state and the state to transition to into a valid key
            var transition = CreateKey(currentState, a_To);
            // if they key exists in the transitions list
            if (!m_Transitions.Contains(transition) && !m_Transitions.Contains(a_To))
                return false;

            var canTransition = true;
            if (transitionEvent != null)
                canTransition = transitionEvent(transition);

            if (!canTransition)
                return false;

            currentState = a_To;    // Set the state
            return true;            // Success
        }

        public static string CreateKey(string a_From, string a_To)
        {
            string key = a_From + "->" + a_To;

            return key;
        }
        public static string[] ParseStates(string a_Key)
        {
            string[] parsedKey =
            {
                a_Key.Substring(0, a_Key.IndexOf("->")),
                a_Key.Substring(a_Key.LastIndexOf("->") + 2)
            };

            return parsedKey;
        }

        [Flags]
        public enum TransitionType
        {
            To = 1,
            From = 2,
        }

        public List<string> GetTransitionsOnState(string a_State, TransitionType a_Type)
        {
            List<string> transitions = new List<string>();

            foreach (string transition in m_Transitions)
            {
                string[] states = ParseStates(transition);

                if (a_Type == (TransitionType.To | TransitionType.From))
                {
                    if (states[0] == a_State || states[1] == a_State)
                        transitions.Add(transition);
                }
                else if (a_Type == TransitionType.To)
                {
                    if (states[1] == a_State)
                        transitions.Add(transition);
                }
                else if (a_Type == TransitionType.From)
                {
                    if (states[0] == a_State)
                        transitions.Add(transition);
                }
            }
            return transitions;
        }

        public bool Subscribe(TransitionEvent a_Subscription)
        {
            if (IsTransitionRegistered(transitionEvent, a_Subscription))
                return false;

            transitionEvent += a_Subscription;
            return true;
        }
        public bool Subscribe(TransitionChangedEvent a_Subscription)
        {
            if (IsTransitionRegistered(transitionEvent, a_Subscription))
                return false;

            transitionChangedEvent += a_Subscription;
            return true;
        }
        private bool IsTransitionRegistered(Delegate a_Event, Delegate a_Subscription)
        {
            return
                a_Event != null &&
                a_Event.GetInvocationList().Any(transition => transition == a_Subscription);
        }

        /// <summary>
        /// Prints the cached states in the format:
        /// ORDER - STATE
        /// </summary>
        public void PrintStates()
        {
            for (var i = 0; i < m_States.Count; ++i)
                Debug.Message(i + " - " + m_States[i]);
        }
        /// <summary>
        /// Prints the currently defined transitions int the format:
        /// ORDER - STATE_FROM->STATE_TO
        /// </summary>
        public void PrintTransitions()
        {
            var i = 0;
            foreach (var transition in m_Transitions)
            {
                Debug.Message(i + " - " + transition);
                i++;
            }
        }
    }
}
