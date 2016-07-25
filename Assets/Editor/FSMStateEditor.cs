using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;

[CustomEditor(typeof(FSMState))]
public class FSMStateEditor : Editor 
{
	private FSMState m_State;

	private ReorderableList m_ReorderableList;

	private SerializedProperty m_DisplayName;
	private SerializedProperty m_Tag;

	private SerializedProperty m_FromTransitions;

	void OnEnable()
	{
		ScriptableFSMWindow.repaintEvent += Repaint;

		m_State = target as FSMState;

		m_DisplayName = serializedObject.FindProperty ("m_DisplayName");
		m_Tag = serializedObject.FindProperty ("tag");
		m_FromTransitions = serializedObject.FindProperty ("m_FromTransitions");

		m_ReorderableList = new ReorderableList(serializedObject, m_FromTransitions, true, true, false, true);
		m_ReorderableList.drawElementCallback = DrawTransitionElement;
		m_ReorderableList.drawHeaderCallback = 
			(Rect rect) => 
			{
				EditorGUI.LabelField(rect, "Transitions");
			};

		m_ReorderableList.onRemoveCallback = 
			(ReorderableList list) => 
			{
				if (EditorUtility.DisplayDialog("Warning!", 
					"Are you sure you want to delete this transition?", "Yes", "No")) 
				{
					var element = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(m_ReorderableList.index);
					var transitionAtIndex = element.objectReferenceValue as FSMTransition;
					m_State.RemoveTransition(transitionAtIndex);
				}
			};		
	}

	public override void OnInspectorGUI ()
	{		
		serializedObject.Update();
		{
			EditorGUILayout.BeginHorizontal ();
			{
				EditorGUILayout.PrefixLabel (m_DisplayName.displayName);
				m_DisplayName.stringValue = EditorGUILayout.DelayedTextField (m_DisplayName.stringValue);
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			{
				EditorGUILayout.PrefixLabel (m_Tag.displayName);
				m_Tag.stringValue = EditorGUILayout.DelayedTextField (m_Tag.stringValue);
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.Space();
			m_ReorderableList.DoLayoutList();
			EditorGUILayout.Space();

			if (GUILayout.Button ("Delete This State")) 
			{
				if (EditorUtility.DisplayDialog ("Warning!", 
					    "Are you sure you want to delete this state?", "Yes", "No")) 
				{
					Selection.activeObject = null;
					DestroyImmediate (m_State, true);
				}
			}
		}
		if (m_State == null)
			return;

		serializedObject.ApplyModifiedProperties ();

		m_State.name = m_DisplayName.stringValue;
	} 

	private void DrawTransitionElement(Rect a_Rect, int a_Index, bool a_IsActive, bool a_IsFocused)
	{
		a_Rect.y += 2f;
		var element = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(a_Index);

		var transitionAtIndex = element.objectReferenceValue as FSMTransition;

		EditorGUI.LabelField(
			new Rect(a_Rect.x, a_Rect.y, a_Rect.width, EditorGUIUtility.singleLineHeight),
			new GUIContent(transitionAtIndex.displayName));
	}
}
