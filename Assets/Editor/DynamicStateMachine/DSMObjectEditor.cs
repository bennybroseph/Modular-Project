using UnityEngine;
using UnityEditor;
using Library;
using UnityEditorInternal;

namespace DynamicStateMachine
{
	[CustomEditor(typeof(DSMObject))]
	public class DSMObjectEditor : Editor
	{
		private DSMObject m_DSMObject;

	    private string m_FocusedControl;
	    private string m_CurrentState;

	    private ReorderableList m_ReorderableList;

	    void OnEnable()
	    {
	        DSMEditorWindow.repaintEvent += Repaint;

	        m_DSMObject = target as DSMObject;
	    }

	    public override void OnInspectorGUI()
	    {
	        DrawDefaultInspector();
	        serializedObject.Update();
	        {
				
	        }
	        serializedObject.ApplyModifiedProperties();

	        if(GUILayout.Button("Initialize"))
	            m_DSMObject.Init();
	    }
	}
}
