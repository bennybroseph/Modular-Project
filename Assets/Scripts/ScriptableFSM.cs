using System;
using Library;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "NewFSM", menuName = "Scriptable FSM")]
public class ScriptableFSM : ScriptableObject
{
    [SerializeField, HideInInspector]
    private bool m_IsInitialized;

    public List<FSMState> m_States = new List<FSMState>();

	public FSMState m_EntryPoint;

    public bool AddState(string a_DisplayName = "New State", Vector2 a_Position = default(Vector2))
    {
        var displayName = a_DisplayName;

        for (int i = 0; m_States.Any(x => x.displayName == displayName); ++i)
            displayName = a_DisplayName + " " + i;

        return AddState(displayName, a_Position, FSMState.Attribute.None);
    }
	private bool AddState(
		string a_DisplayName, 
		Vector2 a_Position, 
		FSMState.Attribute a_Attribute, 
		FSMState.AllowedTransitionType a_AllowedTransitions = FSMState.AllowedTransitionType.All,
		int a_MaxTransitions = -1)
    {
        var newState = this.AddChildAsset<FSMState>();

		newState.Init(this, a_DisplayName, a_Position, a_Attribute, a_AllowedTransitions, a_MaxTransitions);

        m_States.Add(newState);
        return true;
    }
    public void RemoveState(FSMState a_State)
    {
		if (!EditorUtility.DisplayDialog (
				"Warning!", 
			    "Are you sure you want to delete this state?", "Yes", "No"))
			return;
		
        DestroyImmediate(a_State, true);
    }

    public void Init()
    {
        if (m_IsInitialized)
            return;

        foreach (var state in m_States)
            DestroyImmediate(state, true);
        m_States.Clear();

		AddState("Entry", new Vector2(200f, 400f), FSMState.Attribute.Entry, FSMState.AllowedTransitionType.From, 1);
		AddState("Any State", new Vector2(200f, 250f), FSMState.Attribute.FromAny, FSMState.AllowedTransitionType.From);

        m_IsInitialized = true;
    }

	private void OnDestroy()
	{
		if (Selection.activeObject == this)
			Selection.activeObject = null;
	}

	public void OnStateDestroyed(FSMState a_State)
	{
		m_States.Remove (a_State);
	}
}

public static class ScriptableExtension
{
    public static T AddChildAsset<T>(this ScriptableObject self) where T : ScriptableObject
    {
        var newChild = ScriptableObject.CreateInstance<T>();
        //newChild.hideFlags = HideFlags.HideInHierarchy;

        AssetDatabase.AddObjectToAsset(newChild, self);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return newChild;
    }
}
