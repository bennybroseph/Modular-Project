using UnityEngine;
using UnityEditor;
using System.Collections;

namespace DynamicStateMachine
{
	[CustomEditor(typeof(DSMTransition))]
	public class DSMTransitionEditor : Editor 
	{
		private void OnEnable()
		{
			DSMEditorWindow.repaintEvent += Repaint;
		}

		public override void OnInspectorGUI ()
		{
			DSMTransition transition = target as DSMTransition;

			DrawDefaultInspector ();
			serializedObject.Update ();
			{
				EditorGUILayout.BeginHorizontal ();
				{
					EditorGUILayout.PrefixLabel (serializedObject.FindProperty ("m_DisplayName").displayName);
					serializedObject.FindProperty ("m_DisplayName").stringValue = 
						EditorGUILayout.DelayedTextField (serializedObject.FindProperty ("m_DisplayName").stringValue);	
				}
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.Space ();
				if(GUILayout.Button("Delete This Transition"))
					transition.states.fromState.RemoveTransition(transition);
			}
			if (transition == null)
				return;

			serializedObject.ApplyModifiedProperties ();

			transition.name = transition.displayName;
		}
	}
}
