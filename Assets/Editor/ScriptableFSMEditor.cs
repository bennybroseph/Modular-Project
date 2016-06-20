using UnityEngine;
using System.Collections;
using System.Reflection;
using Library;
using UnityEditor;

public class ScriptableFSMEditor : EditorWindow
{
    private static DynamicFSM s_DynamicFSM;

    [MenuItem("Window/Finite State Machine")]
    private static void ShowEditor()
    {
        var editor = GetWindow<ScriptableFSMEditor>();
        s_DynamicFSM = FindObjectOfType<FiniteStateMachine>().scriptableFSM.dynamicFSM;
    }

    private void OnGUI()
    {
        if(s_DynamicFSM == null)
            return;

        BeginWindows();

        int i = 0;
        foreach (var state in s_DynamicFSM.m_States)
        {
            GUI.Window(i, new Rect(10, 10 + i * 100, 100, 100), DrawNodeWindow, state);
            i++;
        }
        
        EndWindows();
    }

    private void DrawNodeWindow(int a_WindowID)
    {
        GUI.DragWindow();
    }
}
