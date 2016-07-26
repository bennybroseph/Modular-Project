﻿using System;
using UnityEngine;
using UnityEditor;

[Serializable]
public class FSMTransition : ScriptableObject
{
    [Serializable]
    public class TransitionState
    {
        public FSMState fromState;
        public FSMState toState;
    }

	[SerializeField]
	private ScriptableFSM m_Parent;

    [SerializeField]
    private string m_DisplayName;

    [SerializeField, HideInInspector]
    private TransitionState m_State;

    public string displayName
    {
        get
        {
            return
                string.IsNullOrEmpty(m_DisplayName) ?
                    m_State.fromState.displayName + " -> " + m_State.toState.displayName :
                    m_DisplayName;
        }
    }

    public TransitionState state
    {
        get { return m_State; }
    }

	public void Init(ScriptableFSM a_Parent, FSMState a_From, FSMState a_To, string a_DisplayName = null)
    {
		m_Parent = a_Parent;

        m_DisplayName = a_DisplayName;

        m_State = new TransitionState
        {
            fromState = a_From,
            toState = a_To,
        };
			
        name = m_State.fromState.displayName + " -> " + m_State.toState.displayName;
    }

    private void OnDestroy()
    {
		m_State.fromState.OnTransitionDestroyed (this);
		m_State.toState.OnTransitionDestroyed (this);

		if (Selection.activeObject == this)
			Selection.activeObject = null;
		
		if (m_State.fromState.attribute == FSMState.Attribute.Entry)
			m_Parent.m_EntryPoint = null;
    }

    public void OnStateDestroyed()
    {
        DestroyImmediate(this, true);
    }
}