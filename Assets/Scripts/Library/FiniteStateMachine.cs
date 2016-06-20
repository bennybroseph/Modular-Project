using System;                       // Required for the type 'Enum'
using System.Collections.Generic;   // Required to use 'List<T>' and 'Dictionary<T, T>'
using Library.Contextual;

namespace Library
{
    // Usage:
    // enum MyStates { Init, Idle };
    // FiniteStateMachine<MyStates> MyFSM = new FiniteStateMachine<MyStates>(OPTIONAL: MyStates.Idle);
    /// <summary>
    /// My Finite State Machine
    /// </summary>
    /// <typeparam name="T">A 'System.Type' in which 'T.IsEnum()' is true</typeparam>
    [Serializable]
    public sealed class FiniteStateMachine<T>
    {
        /// <summary>
        /// Delegate that will be used to determine if a state change is valid by the user.
        /// Returns true or false and takes no parameters
        /// </summary>
        /// <returns> Whether or not the transition is valid based on the user's specification </returns>
        public delegate bool IsValidateTransition();

        /// <summary> Cached list of all states in the enumeration </summary>
        private readonly List<T> m_States;

        /// <summary> Dynamic dictionary of all transitions as dictated by the user </summary>
        private readonly Dictionary<string, IsValidateTransition> m_Transitions;
        /// <summary>
        /// Dictionary which holds all of the transitions which are valid from any other state
        /// ex. Going from any state to 'Dead' would be a common use
        /// </summary>
        private readonly Dictionary<T, IsValidateTransition> m_TransitionsFromAny;

        /// <summary>
        /// Read-Only property for the current state 'm_CurrentState'.
        /// Look at me. I'm the captain now.
        /// </summary>
        public T currentState { get; private set; }

        /// <summary> Default constructor which will initialize the list and dictionary </summary>
        public FiniteStateMachine()
        {
            m_States = new List<T>();
            m_Transitions = new Dictionary<string, IsValidateTransition>();
            m_TransitionsFromAny = new Dictionary<T, IsValidateTransition>();

            StoreStates();
        }
        /// <summary>
        /// Parameterized constructor which allows a state other than 'm_States[0]' to initialize 'm_CurrentState'
        /// </summary>
        /// <param name="a_InitialState">Used as the current state 'm_CurrentState' on creation</param>
        public FiniteStateMachine(T a_InitialState) : this()
        {
            currentState = a_InitialState;
        }

        /// <summary>
        /// Attempts to add a new transition to the current list of transitions
        /// </summary>
        /// <param name="a_From">The state to come from</param>
        /// <param name="a_To">The state to go to</param>
        /// <param name="a_IsValidTransition">An optional delegate with no parameters that returns true when the state change is valid and false when it is not</param>
        /// <returns>Returns true if the transition was able to be added and false otherwise</returns>
        public bool AddTransition(T a_From, T a_To, IsValidateTransition a_IsValidTransition = null)
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
                T invalidKey;   // Will decipher which state is invalid
                if (!m_States.Contains(a_From))
                    invalidKey = a_From;
                else
                    invalidKey = a_To;

                Debug.Warning("'" + invalidKey + "' does not exist in '" + typeof(T) + "'");
                return false;
            }

            // Properly serializes 'a_From' and 'a_To' into the expected key format
            string key = a_From.ToString() + "->" + a_To.ToString();
            // if the key 'key' does not currently exist in 'm_Transitions'
            if (!m_Transitions.ContainsKey(key))
            {
                // if the user did not pass in a delegate to check the transition
                if (a_IsValidTransition == null)
                    m_Transitions[key] = () => true;            // Set a default one that always allows the transition
                else
                    m_Transitions[key] = a_IsValidTransition;   // Otherwise use the one they passed in
                return true;
            }
            else
            {
                Debug.Warning("'" + key + "' already exists as a transition key");
                return false;
            }
        }
        /// <summary>
        /// Attempts to add a new transition to the current list which is able to be transitioned to from any other state
        /// </summary>
        /// <param name="a_To">The state to transition to from any other state</param>
        /// <param name="a_IsValidateTransition">An optional delegate with no parameters that returns true when the state change is valid and false when it is not</param>
        /// <returns>Returns true if the transition was able to be added and false otherwise</returns>
        public bool AddTransitionFromAny(T a_To, IsValidateTransition a_IsValidateTransition = null)
        {
            if (!m_States.Contains(a_To))
            {
                Debug.Warning("'" + a_To + "'does not exist in '" + typeof(T) + "'");
                return false;
            }

            if (!m_TransitionsFromAny.ContainsKey(a_To))
            {
                if (a_IsValidateTransition == null)
                    m_TransitionsFromAny[a_To] = () => true;
                else
                    m_TransitionsFromAny[a_To] = a_IsValidateTransition;
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
        public bool Transition(T a_To)
        {
            // Converts the current state and the state to transition to into a valid key
            string key = currentState + "->" + a_To;
            // if they key exists in the transition dictionary
            if (m_Transitions.ContainsKey(key) && m_Transitions[key]() ||
                m_TransitionsFromAny.ContainsKey(a_To) && m_TransitionsFromAny[a_To]())
            {
                currentState = a_To;    // Set the state
                return true;            // Success
            }

            return false;
        }

        /// <summary>
        /// Grabs each state from the type of enumeration and caches it into a list
        /// </summary>
        private void StoreStates()
        {
            // if 'T' is an enumeration type
            if (typeof(T).IsEnum)
            {
                // Iterate through each parsed state 'iState'
                foreach (var iState in Enum.GetValues(typeof(T)))
                    m_States.Add((T)iState);    // Cache it

                currentState = m_States[0];   // Set the current state to the first found state
                return;
            }

            Debug.Error("Incorrect type '" + typeof(T) + "'");
        }

        /// <summary>
        /// Prints the cached states in the format:
        /// ORDER - STATE
        /// </summary>
        public void PrintStates()
        {
            for (var i = 0; i < m_States.Count; ++i)
                Debug.Message(i + " - " + m_States[i].ToString());
        }
        /// <summary>
        /// Prints the currently defined transitions int the format:
        /// ORDER - STATE_FROM->STATE_TO
        /// </summary>
        public void PrintTransitions()
        {
            var i = 0;
            foreach (var iPair in m_Transitions)
            {
                Debug.Message(i + " - " + iPair.Key);
                i++;
            }
        }
    }
}
