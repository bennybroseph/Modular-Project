using UnityEngine;
using System.Collections;
using System.Reflection;
using Library;
using UnityEditor;
using System.Collections.Generic;

public class ScriptableFSMEditor : EditorWindow
{
    private static DynamicFSM s_DynamicFSM;

	private List<Rect> m_WindowRects;

    [MenuItem("Window/Finite State Machine")]
    private static void ShowEditor()
    {
        var editor = GetWindow<ScriptableFSMEditor>();
        s_DynamicFSM = FindObjectOfType<FiniteStateMachine>().scriptableFSM.dynamicFSM;
    }

	private ScriptableFSMEditor()
	{
		m_WindowRects = new List<Rect> ();
	}

    private void OnGUI()
    {
        if(s_DynamicFSM == null)
            return;

        BeginWindows();

		for(int i = 0; i < s_DynamicFSM.m_States.Count; ++i)
        {
			if (m_WindowRects.Count <= i)
				m_WindowRects.Add (new Rect(10, 10 + i * 100, 100, 100));
			
			m_WindowRects[i] = GUI.Window(i, m_WindowRects[i], DrawNodeWindow, s_DynamicFSM.m_States[i]);            
        }
        
        EndWindows();
    }

    private void DrawNodeWindow(int a_WindowID)
    {
        GUI.DragWindow();
    }
}
