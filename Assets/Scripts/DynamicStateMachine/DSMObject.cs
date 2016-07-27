using System;
using Library;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

using Object = UnityEngine.Object;

namespace DynamicStateMachine
{
	[Serializable]
	[CreateAssetMenu(fileName = "NewDSM", menuName = "DSM Asset")]
	public class DSMObject : ScriptableObject, IDrawableDSMObject
	{
		[HideInInspector]
		public UnityEvent entryPointChanged;

		[SerializeField, HideInInspector]
		private DSMState m_EntryPoint;

	    [SerializeField, HideInInspector]
	    private bool m_IsInitialized;

		[SerializeField, HideInInspector]
		private List<DSMState> m_States = new List<DSMState>();

		public IDSMState entryPoint 
		{
			get{ return m_EntryPoint as IDSMState; }
			set{ m_EntryPoint = value as DSMState; entryPointChanged.Invoke ();}
		}

		public List<IDSMState> states 
		{
			get 
			{
				var newList = new List<IDSMState>();
				foreach (var state in m_States)
					newList.Add (state as IDSMState);

				return newList; 
			}
		}
		public List<IDrawableDSMState> drawableStates 
		{
			get 
			{
				var newList = new List<IDrawableDSMState>();
				foreach (var state in m_States)
					newList.Add (state as IDrawableDSMState);

				return newList; 
			}
		}

	    public bool AddState(string a_DisplayName = "New State", Vector2 a_Position = default(Vector2))
	    {
	        var displayName = GenerateUniqueName (a_DisplayName);

	        return AddState(displayName, a_Position, StateAttribute.None);
	    }
		private bool AddState(
			string a_DisplayName, 
			Vector2 a_Position, 
			StateAttribute a_Attribute, 
			AllowedTransitionType a_AllowedTransitions = AllowedTransitionType.All,
			int a_MaxTransitions = -1)
	    {
			var newState = this.AddChildAsset<DSMState> ();
			newState.Init(this, a_DisplayName, a_Position, a_Attribute, a_AllowedTransitions, a_MaxTransitions);

	        m_States.Add(newState);
	        return true;
	    }
	    public void RemoveState(IDSMState a_State)
	    {
			if (!EditorUtility.DisplayDialog (
					"Warning!", 
				    "Are you sure you want to delete this state?", "Yes", "No"))
				return;
			
			DestroyImmediate(a_State as Object, true);
	    }

		public string GenerateUniqueName(string a_OldName)
		{
			var newName = a_OldName;
			for (int i = 0; m_States.Any(x => x.displayName == newName); ++i)
				newName = a_OldName + " " + i;

			return newName;
		}

	    public void Init()
	    {
	        if (m_IsInitialized)
	            return;

			entryPointChanged = new UnityEvent ();

			foreach (var state in m_States) 
			{
				if(state.GetType().IsAssignableFrom(typeof(Object)))
					DestroyImmediate (state as Object, true);
			}
	        m_States.Clear();

			AddState("Entry", new Vector2(200f, 400f), StateAttribute.Entry, AllowedTransitionType.From, 1);
			AddState("Any State", new Vector2(200f, 250f), StateAttribute.FromAny, AllowedTransitionType.From);

	        m_IsInitialized = true;
	    }

		private void OnDestroy()
		{
			if (Selection.activeObject == this)
				Selection.activeObject = null;
		}

		public void OnStateDestroyed(DSMState a_State)
		{
			m_States.Remove (a_State);
		}
	}

	public static class ScriptableExtension
	{
	    public static T AddChildAsset<T>(this ScriptableObject self) where T : ScriptableObject
	    {
	        var newChild = ScriptableObject.CreateInstance<T>();
	        newChild.hideFlags = HideFlags.HideInHierarchy;

	        AssetDatabase.AddObjectToAsset(newChild, self);
	        AssetDatabase.SaveAssets();
	        AssetDatabase.Refresh();

	        return newChild;
	    }
	}
}
