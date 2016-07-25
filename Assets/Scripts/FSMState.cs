using System;
using UnityEngine;
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

    public Vector2 position;
    public string tag;

    [SerializeField]
    private string m_DisplayName;
    public List<FSMTransition> m_Transitions = new List<FSMTransition>();

    [SerializeField, HideInInspector]
    private ScriptableFSM m_Parent;

    [SerializeField]
    private Attribute m_Attribute;

    public string displayName
    {
        get { return m_DisplayName; }
    }
    public List<FSMTransition> transitions
    {
        get { return m_Transitions; }
    }

    public Attribute attribute
    {
        get { return m_Attribute; }
    }

    public bool AddTransition(FSMState a_Other)
    {
        var newTransition = m_Parent.AddChildAsset<FSMTransition>();

        newTransition.Init(this, a_Other);
        
        m_Transitions.Add(newTransition);
        return true;
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
        List<FSMTransition> tempList = m_Transitions.ToList();
        foreach (var transition in tempList)
            transition.OnStateDestroyed();
    }

    public void OnTransitionDestroyed(FSMTransition a_FSMTransition)
    {
        m_Transitions.Remove(a_FSMTransition);
    }
}
