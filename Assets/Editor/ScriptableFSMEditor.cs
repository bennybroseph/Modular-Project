using System;
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

    private static GUISkin s_GUISkin;

    [MenuItem("Window/Finite State Machine")]
    private static void ShowEditor()
    {
        var editor = GetWindow<ScriptableFSMEditor>();
        editor.titleContent = new GUIContent("FSM");

        SetReferencedDynamicFSM();
    }

    private ScriptableFSMEditor()
    {
        s_BoxSize = new Vector2(200f, 40f);

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

                float radius = 10f;
                float angle =
                    Mathf.PI / 2 + Mathf.Atan2(
                        s_ScriptableFSM.windowPositions[index[1]].y - s_ScriptableFSM.windowPositions[index[0]].y,
                        s_ScriptableFSM.windowPositions[index[1]].x - s_ScriptableFSM.windowPositions[index[0]].x);
                List<Vector2> linePositions = new List<Vector2>
                {
                    new Vector2(
                        s_ScriptableFSM.windowPositions[index[0]].x + s_BoxSize.x / 2
                        + radius * Mathf.Cos(angle),
                        s_ScriptableFSM.windowPositions[index[0]].y + s_BoxSize.y / 2
                        + radius * Mathf.Sin(angle)),
                    new Vector2(
                        s_ScriptableFSM.windowPositions[index[1]].x + s_BoxSize.x / 2
                        + radius * Mathf.Cos(angle),
                        s_ScriptableFSM.windowPositions[index[1]].y + s_BoxSize.y / 2
                        + radius * Mathf.Sin(angle))
                };
                Handles.DrawLine(linePositions[0], linePositions[1]);

                Vector2 between =
                    new Vector2(
                        linePositions[1].x - linePositions[0].x,
                        linePositions[1].y - linePositions[0].y);
                between /= 2f;
                between += linePositions[0];
                List<Vector3> vertices = new List<Vector3>
                {
                    ScalePosition(this, new Vector2(
                        between.x + radius * Mathf.Cos(angle + 3 * Mathf.PI / 2),
                        between.y + radius * Mathf.Sin(angle + 3 * Mathf.PI / 2))),
                    ScalePosition(this, new Vector2(
                        between.x + radius * Mathf.Cos(angle + 3 * Mathf.PI / 4),
                        between.y + radius * Mathf.Sin(angle + 3 * Mathf.PI / 4))),
                    ScalePosition(this, new Vector2(
                        between.x + radius * Mathf.Cos(angle + Mathf.PI / 4),
                        between.y + radius * Mathf.Sin(angle + Mathf.PI / 4))),
                };

                GL.PushMatrix();
                GL.LoadOrtho();
                GL.Begin(GL.TRIANGLES);
                {
                    GL.Color(Color.white);
                    GL.Vertex3(vertices[0].x, vertices[0].y, vertices[0].z);
                    GL.Vertex3(vertices[1].x, vertices[1].y, vertices[1].z);
                    GL.Vertex3(vertices[2].x, vertices[2].y, vertices[2].z);
                    //GL.Vertex3(0f, 0.1f, 0f);
                    //GL.Vertex3(0f, 0.2f, 0f);
                    //GL.Vertex3(0.1f, 0.15f, 0f);
                }
                GL.End();
                GL.PopMatrix();
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

                GUI.color = s_ScriptableFSM.dynamicFSM.currentState == s_ScriptableFSM.dynamicFSM.states[i] ?
                    new Color(251f / 255f, 140f / 255f, 0f, 1f) :
                    Color.gray;

                windowRect =
                    GUI.Window(
                        i,
                        windowRect,
                        DrawNodeWindow,
                        "",
                        s_GUISkin.button);

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
            case EventType.MouseDown:
                {
                    if (Event.current.button == 0)
                    {
                        s_AddingTransition = false;
                        Repaint();
                    }
                }
                break;
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
                        EditorUtility.SetDirty(s_ScriptableFSM);
                        s_AddingTransition = false;
                    }
                }
                break;
        }
        GUIStyle newStyle = GUI.skin.GetStyle("Label");
        newStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(Vector2.zero, s_BoxSize), s_ScriptableFSM.dynamicFSM.states[a_WindowID], newStyle);
        GUI.DragWindow();
    }

    private static void AddState(object a_Obj)
    {
        s_ScriptableFSM.dynamicFSM.AddState();
        s_ScriptableFSM.windowPositions.Add(s_MousePosition);

        EditorUtility.SetDirty(s_ScriptableFSM);
    }
    private static void RemoveState(object a_Obj)
    {
        string state = s_ScriptableFSM.dynamicFSM.states[(int)a_Obj];

        s_ScriptableFSM.windowPositions.RemoveAt((int)a_Obj);
        s_ScriptableFSM.dynamicFSM.RemoveState(state);

        EditorUtility.SetDirty(s_ScriptableFSM);
    }

    private static void AddTransition(object a_Obj)
    {
        s_AddingTransition = true;
        s_TransitionAnchor = s_ScriptableFSM.dynamicFSM.states[(int)a_Obj];

        EditorUtility.SetDirty(s_ScriptableFSM);

    }
    private static void RemoveTransition(object a_Obj)
    {
        s_ScriptableFSM.dynamicFSM.RemoveTransition((string)a_Obj);

        EditorUtility.SetDirty(s_ScriptableFSM);
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
        if (Selection.activeGameObject == null ||
            Selection.activeGameObject.GetComponent<FiniteStateMachine>() == null ||
            Selection.activeGameObject.GetComponent<FiniteStateMachine>().scriptableFSM == null)
            return;

        s_ScriptableFSM = Selection.activeGameObject.GetComponent<FiniteStateMachine>().scriptableFSM;

        if (s_ScriptableFSM.windowPositions == null)
            s_ScriptableFSM.windowPositions = new List<Vector2>();

        s_GUISkin = EditorGUIUtility.Load("MyGUISkin.guiskin") as GUISkin;
    }

    private static Vector3 ScalePosition(EditorWindow a_Window, Vector2 a_Position)
    {
        float tabHeight = 22f;
        Vector3 scaledPosition =
            new Vector3(
                a_Position.x / a_Window.position.width,
                (a_Window.position.height - a_Position.y) / (a_Window.position.height + tabHeight));

        //Debug.Log(a_Window.position.height + ", " + a_Position.y);

        return scaledPosition;
    }
}
