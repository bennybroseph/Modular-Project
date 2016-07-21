using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Library;

public class ScriptableFSMEditor : EditorWindow
{
    private static ScriptableFSM s_ScriptableFSM;

    private static GenericMenu s_ContextMenu;

    private static Vector2 s_BoxSize;

    private static string s_TransitionAnchor;
    private static bool s_AddingTransition;

    private static Vector2 s_MousePosition;

    [MenuItem("Window/Finite State Machine")]
    private static void ShowEditor()
    {
        var editor = GetWindow<ScriptableFSMEditor>();
        editor.titleContent = new GUIContent("FSM");
    }

    private ScriptableFSMEditor()
    {
        s_BoxSize = new Vector2(200f, 50f);

        SetReferencedDynamicFSM();
    }

    private void OnGUI()
    {
        if (s_ScriptableFSM == null)
            return;

        for (int i = 0; s_ScriptableFSM.windowPositions.Count < s_ScriptableFSM.dynamicFSM.states.Count; ++i)
        {
            s_ScriptableFSM.windowPositions.Add(new Vector2(10 + i * 25, 10 + i * 25));
        }

        Handles.BeginGUI();
        {
            foreach (string key in s_ScriptableFSM.dynamicFSM.transitions.Keys)
            {
                string[] states = DynamicFSM.ParseStates(key);

                List<int> index = new List<int>
                {
                    s_ScriptableFSM.dynamicFSM.states.FindIndex(x => x == states[0]),
                    s_ScriptableFSM.dynamicFSM.states.FindIndex(x => x == states[1])
                };
                int coefficient = 10;
                if (s_ScriptableFSM.windowPositions[index[0]].y + s_BoxSize.y / 2 >
                    s_ScriptableFSM.windowPositions[index[1]].y + s_BoxSize.y / 2)
                    coefficient = -coefficient;
                List<Vector2> linePositions = new List<Vector2>()
                {
                    new Vector2(
                        s_ScriptableFSM.windowPositions[index[0]].x + s_BoxSize.x / 2 - coefficient,
                        s_ScriptableFSM.windowPositions[index[0]].y + s_BoxSize.y / 2),
                    new Vector2(
                        s_ScriptableFSM.windowPositions[index[1]].x + s_BoxSize.x / 2 - coefficient,
                        s_ScriptableFSM.windowPositions[index[1]].y + s_BoxSize.y / 2)
                };

                Handles.DrawLine(linePositions[0], linePositions[1]);
            }
        }
        Handles.EndGUI();

        BeginWindows();
        {
            for (int i = 0; i < s_ScriptableFSM.dynamicFSM.states.Count; ++i)
            {
                Rect windowRect =
                    new Rect(
                        s_ScriptableFSM.windowPositions[i].x,
                        s_ScriptableFSM.windowPositions[i].y,
                        s_BoxSize.x,
                        s_BoxSize.y);

                windowRect =
                    GUI.Window(
                        i,
                        windowRect,
                        DrawNodeWindow,
                        s_ScriptableFSM.dynamicFSM.states[i],
                        GUI.skin.button);

                s_ScriptableFSM.windowPositions[i] = new Vector2(windowRect.x, windowRect.y);
            }
        }
        EndWindows();

        Handles.BeginGUI();
        {
            if (s_AddingTransition)
            {
                int index = s_ScriptableFSM.dynamicFSM.states.FindIndex(x => x == s_TransitionAnchor);
                Vector2 linePosition =
                    new Vector2(
                        s_ScriptableFSM.windowPositions[index].x + s_BoxSize.x / 2f,
                        s_ScriptableFSM.windowPositions[index].y + s_BoxSize.y / 2f);

                Handles.DrawLine(linePosition, Event.current.mousePosition);
            }
        }
        Handles.EndGUI();

        switch (Event.current.type)
        {
            case EventType.ContextClick:
                {
                    CreateGeneralContextMenu();
                    s_ContextMenu.ShowAsContext();
                }
                break;
                //default: s_AddingTransition = false; break;
        }

        s_MousePosition = Event.current.mousePosition;
    }

    private void Update()
    {
        if (s_AddingTransition)
            Repaint();
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
                        if (!s_AddingTransition)
                        {
                            CreateWindowContextMenu(a_WindowID);
                            s_ContextMenu.ShowAsContext();
                        }
                        else
                            s_AddingTransition = false;
                    }
                    if (Event.current.button == 0 && s_AddingTransition)
                    {
                        s_ScriptableFSM.dynamicFSM.AddTransition(
                            s_TransitionAnchor,
                            s_ScriptableFSM.dynamicFSM.states[a_WindowID]);
                        s_AddingTransition = false;
                    }
                }
                break;
        }
        GUI.DragWindow();
    }

    private static void AddState(object a_Obj)
    {
        s_ScriptableFSM.dynamicFSM.AddState();
        s_ScriptableFSM.windowPositions.Add(s_MousePosition);
    }
    private static void RemoveState(object a_Obj)
    {
        string state = s_ScriptableFSM.dynamicFSM.states[(int)a_Obj];

        s_ScriptableFSM.windowPositions.RemoveAt((int)a_Obj);
        s_ScriptableFSM.dynamicFSM.RemoveState(state);
    }

    private static void AddTransition(object a_Obj)
    {
        s_AddingTransition = true;
        s_TransitionAnchor = s_ScriptableFSM.dynamicFSM.states[(int)a_Obj];

    }
    private static void RemoveTransition(object a_Obj)
    {
        s_ScriptableFSM.dynamicFSM.RemoveTransition((string)a_Obj);
    }


    private static void CreateGeneralContextMenu()
    {
        s_ContextMenu = new GenericMenu();
        s_ContextMenu.AddItem(new GUIContent("Create/New State"), false, AddState, null);
    }

    private static void CreateWindowContextMenu(int a_ID)
    {
        s_ContextMenu = new GenericMenu();

        s_ContextMenu.AddItem(new GUIContent("Add/Transition"), false, AddTransition, a_ID);
        s_ContextMenu.AddItem(
            new GUIContent("Delete/'" + s_ScriptableFSM.dynamicFSM.states[a_ID] + "'"),
            false,
            RemoveState,
            a_ID);

        if (s_ScriptableFSM.dynamicFSM.transitions.Keys.Count != 0)
        {
            foreach (string key in s_ScriptableFSM.dynamicFSM.transitions.Keys)
            {
                string[] states = DynamicFSM.ParseStates(key);

                if (states[0] == s_ScriptableFSM.dynamicFSM.states[a_ID])
                    s_ContextMenu.AddItem(
                        new GUIContent("Delete/Transition/" + "To '" + states[1] + "'"),
                        false,
                        RemoveTransition,
                        key);
                else if (states[1] == s_ScriptableFSM.dynamicFSM.states[a_ID])
                    s_ContextMenu.AddItem(
                        new GUIContent("Delete/Transition/" + "From '" + states[0] + "'"),
                        false,
                        RemoveTransition,
                        key);
            }
        }
    }

    private static void SetReferencedDynamicFSM()
    {
        if (Selection.activeGameObject == null || Selection.activeGameObject.GetComponent<FiniteStateMachine>() == null)
            return;

        s_ScriptableFSM = Selection.activeGameObject.GetComponent<FiniteStateMachine>().scriptableFSM;

        if (s_ScriptableFSM.windowPositions == null)
            s_ScriptableFSM.windowPositions = new List<Vector2>();
    }
}
