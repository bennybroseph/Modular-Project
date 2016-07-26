using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;

namespace DynamicStateMachine
{
	[CustomEditor(typeof(DSMState))]
	public class DSMStateEditor : Editor 
	{
		private DSMState m_State;

		private ReorderableList m_ReorderableList;

		private SerializedProperty m_DisplayName;
		private SerializedProperty m_Tag;

		private SerializedProperty m_FromTransitions;

		void OnEnable()
		{
			DSMEditorWindow.repaintEvent += Repaint;

			m_State = target as DSMState;

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
					var element = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(m_ReorderableList.index);
					var transitionAtIndex = element.objectReferenceValue as DSMTransition;
					m_State.RemoveTransition(transitionAtIndex);
				};		
		}

		public override void OnInspectorGUI ()
		{		
			DrawDefaultInspector ();
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

				EditorGUILayout.Space ();
				if (GUILayout.Button ("Delete This State")) 
				{
					DSMObject parent = serializedObject.FindProperty ("m_Parent").objectReferenceValue as DSMObject;
					parent.RemoveState (m_State);
				}

				EditorGUILayout.Space();
				m_ReorderableList.DoLayoutList();
				EditorGUILayout.Space();
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

			var transitionAtIndex = element.objectReferenceValue as DSMTransition;

			EditorGUI.LabelField(
				new Rect(a_Rect.x, a_Rect.y, a_Rect.width, EditorGUIUtility.singleLineHeight),
				new GUIContent(transitionAtIndex.displayName));
		}
	}
}