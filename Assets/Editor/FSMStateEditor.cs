using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;

[CustomEditor(typeof(FSMState))]
public class FSMStateEditor : Editor 
{
	private ReorderableList m_ReorderableList;

	private SerializedProperty m_DisplayName;
	private SerializedProperty m_Tag;

	void OnEnable()
	{
		ScriptableFSMWindow.repaintEvent += Repaint;

		m_ReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_FromTransitions"), true, true, false, false);
		m_ReorderableList.drawElementCallback = DrawTransitionElement;
		m_ReorderableList.drawHeaderCallback = 
			(Rect rect) => 
			{ 
				EditorGUI.LabelField(rect, "Transitions");
			};
		m_DisplayName = serializedObject.FindProperty ("m_DisplayName");
		m_Tag = serializedObject.FindProperty ("tag");
	}

	public override void OnInspectorGUI ()
	{
		FSMState state = target as FSMState;

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
		}
		serializedObject.ApplyModifiedProperties ();

		state.name = m_DisplayName.stringValue;
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
