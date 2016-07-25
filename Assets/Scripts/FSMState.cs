using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class FSMState : ScriptableObject
{
    public enum Attribute
    {
        None,
        Entry,
        ToAny,
        Exit,
    }

	[HideInInspector]
    public Vector2 position;
	[HideInInspector]
    public string tag;

	[SerializeField, HideInInspector]
    private string m_DisplayName;
	[SerializeField, HideInInspector]
    private List<FSMTransition> m_FromTransitions = new List<FSMTransition>();
	[SerializeField, HideInInspector]
	private List<FSMTransition> m_ToTransitions = new List<FSMTransition>();

    [SerializeField, HideInInspector]
    private ScriptableFSM m_Parent;

	[SerializeField, HideInInspector]
    private Attribute m_Attribute;

    public string displayName
    {
        get { return m_DisplayName; }
    }
    public List<FSMTransition> transitions
    {
        get { return m_FromTransitions; }
    }

    public Attribute attribute
    {
        get { return m_Attribute; }
    }

	public FSMTransition AddFromTransition(FSMState a_Other)
    {
        var newTransition = m_Parent.AddChildAsset<FSMTransition>();

		newTransition.Init (this, a_Other);

		a_Other.AddToTransition (newTransition);

        m_FromTransitions.Add(newTransition);
		return newTransition;
    }
	public void AddToTransition(FSMTransition a_Transition)
	{
		m_ToTransitions.Add(a_Transition);
	}

    public bool RemoveTransition(FSMTransition a_Transition)
    {
        DestroyImmediate(a_Transition, true);
        return true;
    }

    public void Init(string a_DisplayName, Vector2 a_Position, Attribute a_Attribute, ScriptableFSM a_Parent)
    {
        name = a_DisplayName;
        m_DisplayName = a_DisplayName;

        position = a_Position;

        m_Attribute = a_Attribute;

        m_Parent = a_Parent;
    }

    private void OnDestroy()
    {
		List<FSMTransition> tempList = m_FromTransitions.ToList();
		foreach (var transition in tempList)
			transition.OnStateDestroyed();

		tempList = m_ToTransitions.ToList();
		foreach (var transition in tempList)
			transition.OnStateDestroyed();

		m_Parent.OnStateDestroyed (this);
    }

    public void OnTransitionDestroyed(FSMTransition a_FSMTransition)
    {
		if(m_FromTransitions.Contains(a_FSMTransition))
        	m_FromTransitions.Remove(a_FSMTransition);

		if(m_ToTransitions.Contains(a_FSMTransition))
			m_ToTransitions.Remove(a_FSMTransition);
    }
}
