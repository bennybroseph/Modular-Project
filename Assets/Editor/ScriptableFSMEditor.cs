using UnityEngine;
using System.Collections;
using System.Reflection;
using Library;
using UnityEditor;
using System.Collections.Generic;

public class ScriptableFSMEditor : EditorWindow
{
    private static ScriptableFSM s_ScriptableFSM;

    private static GenericMenu s_ContextMenu;

    [MenuItem("Window/Finite State Machine")]
    private static void ShowEditor()
    {
        var editor = GetWindow<ScriptableFSMEditor>();
        SetReferencedDynamicFSM();
    }

    //private ScriptableFSMEditor()
    //{
    //    if (s_DynamicFSM == null)
    //        return;
    //}

    private void OnGUI()
    {
        if (s_ScriptableFSM == null)
            return;

        BeginWindows();

        for (int i = 0; i < s_ScriptableFSM.dynamicFSM.m_States.Count; ++i)
        {
            if (s_ScriptableFSM.windowPositions.Count <= i)
                s_ScriptableFSM.windowPositions.Add(new Rect(10, 10 + i * 100, 100, 100));

            s_ScriptableFSM.windowPositions[i] =
                GUI.Window(i, s_ScriptableFSM.windowPositions[i], DrawNodeWindow, s_ScriptableFSM.dynamicFSM.m_States[i]);
        }
        EndWindows();

        switch (Event.current.type)
        {
            case EventType.ContextClick:
                {
                    Debug.Log("Clicked The Window");
                    //CreateGeneralContextMenu();
                    //s_ContextMenu.ShowAsContext();
                }
                break;
        }
    }

    private void OnSelectionChange()
    {
        SetReferencedDynamicFSM();
        Repaint();
    }

    private void DrawNodeWindow(int a_WindowID)
    {        
        //switch (Event.current.type)
        //{
        //    case EventType.MouseDown:
        //        {
        //            if (Event.current.button != 1 ||
        //                !s_ScriptableFSM.windowPositions[a_WindowID].Contains(Event.current.mousePosition))
        //                return;
        //            Debug.Log("Clicked " + s_ScriptableFSM.dynamicFSM.m_States[a_WindowID]);
        //            //CreateWindowContextMenu(s_ScriptableFSM.dynamicFSM.m_States[a_WindowID]);
        //            //s_ContextMenu.ShowAsContext();
        //        }
        //        break;
        //}
        Debug.Log(Event.current.type);
        GUI.DragWindow();
        Debug.Log(Event.current.type);
    }

    private static void AddState(object a_Obj)
    {
        s_ScriptableFSM.dynamicFSM.AddState("New State");
    }

    private static void CreateGeneralContextMenu()
    {
        s_ContextMenu = new GenericMenu();
        s_ContextMenu.AddItem(new GUIContent("Create/New State"), false, AddState, "New State");
    }

    private static void CreateWindowContextMenu(string a_State)
    {
        Debug.Log("Clicked " + a_State);
    }

    private static void SetReferencedDynamicFSM()
    {
        if (Selection.activeGameObject == null || Selection.activeGameObject.GetComponent<FiniteStateMachine>() == null)
            return;

        s_ScriptableFSM = Selection.activeGameObject.GetComponent<FiniteStateMachine>().scriptableFSM;

        if (s_ScriptableFSM.windowPositions == null)
            s_ScriptableFSM.windowPositions = new List<Rect>();
    }
}
