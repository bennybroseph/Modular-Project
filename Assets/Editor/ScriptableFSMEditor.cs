using UnityEngine;
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
        editor.titleContent = new GUIContent("FSM");

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

        for (int i = 0; i < s_ScriptableFSM.dynamicFSM.states.Count; ++i)
        {
            if (s_ScriptableFSM.windowPositions.Count <= i)
                s_ScriptableFSM.windowPositions.Add(new Rect(10, 10 + i * 100, 100, 100));

            s_ScriptableFSM.windowPositions[i] =
                GUI.Window(i, s_ScriptableFSM.windowPositions[i], DrawNodeWindow, s_ScriptableFSM.dynamicFSM.states[i]);
        }
        EndWindows();

        switch (Event.current.type)
        {
            case EventType.ContextClick:
                {
                    CreateGeneralContextMenu();
                    s_ContextMenu.ShowAsContext();
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
        switch (Event.current.type)
        {
            case EventType.MouseDown:
                {
                    if (Event.current.button == 1)
                    {
                        CreateWindowContextMenu(a_WindowID);
                        s_ContextMenu.ShowAsContext();
                    }
                }
                break;
        }
        GUI.DragWindow();
    }

    private static void AddState(object a_Obj)
    {
        s_ScriptableFSM.dynamicFSM.AddState();
    }
    private static void RemoveState(object a_Obj)
    {
        string state = s_ScriptableFSM.dynamicFSM.states[(int)a_Obj];

        s_ScriptableFSM.windowPositions.RemoveAt((int)a_Obj);
        s_ScriptableFSM.dynamicFSM.RemoveState(state);
    }

    private static void CreateGeneralContextMenu()
    {
        s_ContextMenu = new GenericMenu();
        s_ContextMenu.AddItem(new GUIContent("Create/New State"), false, AddState, null);
    }

    private static void CreateWindowContextMenu(int a_ID)
    {
        s_ContextMenu = new GenericMenu();
        s_ContextMenu.AddItem(new GUIContent("Delete/" + s_ScriptableFSM.dynamicFSM.states[a_ID]), false, RemoveState, a_ID);
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
