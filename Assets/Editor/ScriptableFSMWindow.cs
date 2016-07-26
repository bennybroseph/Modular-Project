using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Library;


public class ScriptableFSMWindow : EditorWindow
{
    public delegate void RepaintEvent();
    public static event RepaintEvent repaintEvent;

    [SerializeField]
    private Vector2 m_StateButtonSize = new Vector2(200, 40);
    [SerializeField]
    private Vector2 m_SpecialButtonSize = new Vector2(150, 30);

    [SerializeField]
    private Color m_NormalButtonColor = Color.gray;
    [SerializeField]
	private Color m_EntryButtonColor = new Color32(76, 175, 80, 255);
    [SerializeField]
	private Color m_AnyStateButtonColor = new Color32(3, 169, 244, 255);

    private ScriptableFSM m_ScriptableFSM;

    private GenericMenu m_ContextMenu;

    private FSMState m_TransitionAnchor;
    private bool m_AddingTransition;

    private Vector2 m_MousePosition;

    private GUISkin m_GUISkin;

    [MenuItem("Window/Finite State Machine")]
    private static void ShowEditor()
    {
        var editor = GetWindow<ScriptableFSMWindow>();
        editor.titleContent = new GUIContent("FSM");
        editor.SetReferencedDynamicFSM();
    }

    private ScriptableFSMWindow()
    {
        SetReferencedDynamicFSM();
    }

    private void OnFocus()
    {
        SetReferencedDynamicFSM();
    }
    private void OnEnable()
    {
        SetReferencedDynamicFSM();
    }

    private void OnGUI()
    {
        DrawGrid();

        if (m_ScriptableFSM == null)
            return;

		Dictionary<FSMTransition, Rect> transitionLineRects = new Dictionary<FSMTransition, Rect>();

        Handles.BeginGUI();
        {
			if (Selection.activeObject.GetType() == typeof(FSMState)) 
			{
				var activeState = Selection.activeObject as FSMState;

				float width = 5f;
				Handles.color = new Color(1, 1, 1, 0.2f);

				Vector2 buttonSize = activeState.attribute == FSMState.Attribute.None ? 
					m_StateButtonSize :
					m_SpecialButtonSize;
				
				Handles.DrawAAConvexPolygon(
					new Vector3(
						activeState.position.x - width,
						activeState.position.y - width),
					new Vector3(
						activeState.position.x - width,
						activeState.position.y + buttonSize.y + width),
					new Vector3(
						activeState.position.x + buttonSize.x + width,
						activeState.position.y + buttonSize.y + width),
					new Vector3(
						activeState.position.x + buttonSize.x + width,
						activeState.position.y - width));			
			}

            foreach (var state in m_ScriptableFSM.m_States)
            {
                foreach (var transition in state.transitions)
                {
                    float radius = 5f;
                    float angle =
                        Mathf.PI / 2f + Mathf.Atan2(
                            transition.state.toState.position.y - transition.state.fromState.position.y,
                            transition.state.toState.position.x - transition.state.fromState.position.x);
                    List<Vector2> linePositions = new List<Vector2>
                    {
                        new Vector2(
                            transition.state.fromState.position.x + m_StateButtonSize.x / 2f
                            + radius * Mathf.Cos(angle),
                            transition.state.fromState.position.y + m_StateButtonSize.y / 2f
                            + radius * Mathf.Sin(angle)),
                        new Vector2(
                            transition.state.toState.position.x + m_StateButtonSize.x / 2f
                            + radius * Mathf.Cos(angle),
                            transition.state.toState.position.y + m_StateButtonSize.y / 2f
                            + radius * Mathf.Sin(angle))
                    };
					Handles.color = Selection.activeObject == transition ? (Color)new Color32(107, 178, 255, 255) : Color.white;
                    Handles.DrawAAPolyLine(2.5f, linePositions[0], linePositions[1]);

                    Vector2 between =
                        new Vector2(
                            linePositions[1].x - linePositions[0].x,
                            linePositions[1].y - linePositions[0].y);
                    between /= 2f;
                    between += linePositions[0];

					radius = 7f;

					transitionLineRects.Add (
						transition,
						new Rect (
							new Vector2 (between.x - radius / 2, between.y - radius / 2),
							new Vector2 (radius, radius)));	

                    Handles.DrawAAConvexPolygon(
                        new Vector3(
                            between.x + (radius - 2f) * Mathf.Cos(angle + 3 * Mathf.PI / 2),
                            between.y + (radius - 2f) * Mathf.Sin(angle + 3 * Mathf.PI / 2)),
                        new Vector3(
                            between.x + radius * Mathf.Cos(angle + 3 * Mathf.PI / 4),
                            between.y + radius * Mathf.Sin(angle + 3 * Mathf.PI / 4)),
                        new Vector3(
                            between.x + radius * Mathf.Cos(angle + Mathf.PI / 4),
                            between.y + radius * Mathf.Sin(angle + Mathf.PI / 4)));
                }
            }
        }
        Handles.EndGUI();

        BeginWindows();
        {
            for (int i = 0; i < m_ScriptableFSM.m_States.Count; ++i)
            {
                Vector2 buttonSize = new Vector2(50f, 50f);
                switch (m_ScriptableFSM.m_States[i].attribute)
                {
                    case FSMState.Attribute.None:
                        GUI.color = m_NormalButtonColor;
                        buttonSize = m_StateButtonSize;
                        break;
                    case FSMState.Attribute.Entry:
                        GUI.color = m_EntryButtonColor;
                        buttonSize = m_SpecialButtonSize;
                        break;
                    case FSMState.Attribute.ToAny:
                        GUI.color = m_AnyStateButtonColor;
                        buttonSize = m_SpecialButtonSize;
                        break;
                }

                Rect windowRect =
                    new Rect(
                        m_ScriptableFSM.m_States[i].position.x,
                        m_ScriptableFSM.m_States[i].position.y,
                        buttonSize.x,
                        buttonSize.y);
				
                windowRect =
                    GUI.Window(
                        i,
                        windowRect,
                        DrawNodeWindow,
                        "",
                        m_GUISkin.button);

                m_ScriptableFSM.m_States[i].position = new Vector2(windowRect.x, windowRect.y);
            }
        }
        EndWindows();

        Handles.BeginGUI();
        {
            if (m_AddingTransition)
            {
                Vector2 linePosition =
                    new Vector2(
                        m_TransitionAnchor.position.x + m_StateButtonSize.x / 2f,
                        m_TransitionAnchor.position.y + m_StateButtonSize.y / 2f);

                Handles.color = Color.white;
                Handles.DrawAAPolyLine(2.5f, linePosition, Event.current.mousePosition);
            }
        }
        Handles.EndGUI();

        switch (Event.current.type)
        {
            case EventType.ContextClick:
                {
                    CreateGeneralContextMenu();
                    m_ContextMenu.ShowAsContext();
                }
                break;
            case EventType.MouseDown:
                {
                    if (Event.current.button == 0)
                    {
						bool clickedTransition = false;
						foreach (var transitionRect in transitionLineRects) 
						{
							if (transitionRect.Value.Contains (Event.current.mousePosition)) 
							{
								Selection.activeObject = transitionRect.Key;
								clickedTransition = true;								
							}
						}
						
						if (clickedTransition)
							break;
						
                        Selection.activeObject = m_ScriptableFSM;
                        m_AddingTransition = false;
                    }
                }
                break;
        }

        m_MousePosition = Event.current.mousePosition;
    }

    private void DrawNodeWindow(int a_WindowID)
    {
        switch (Event.current.type)
        {
            case EventType.MouseDown:
                {
                    if ((Event.current.button == 1 || Event.current.button == 0) && !m_AddingTransition)
                    {
                        Selection.activeObject = m_ScriptableFSM.m_States[a_WindowID];

                        if (repaintEvent != null)
                            repaintEvent();
                    }
                    if (Event.current.button == 1)
                    {
                        if (!m_AddingTransition)
                        {
                            CreateWindowContextMenu(m_ScriptableFSM.m_States[a_WindowID]);
                            m_ContextMenu.ShowAsContext();
                        }
                        else
                            m_AddingTransition = false;
                    }
                    if (Event.current.button == 0 && m_AddingTransition)
                    {
                        FSMTransition newTransition = m_TransitionAnchor.AddFromTransition(
                            m_ScriptableFSM.m_States[a_WindowID]);

                        m_AddingTransition = false;
                    }
                }
                break;
        }
        var newStyle = new GUIStyle(GUI.skin.GetStyle("Label"))
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
        };
        GUI.color = Color.white;

		Vector2 buttonSize = m_ScriptableFSM.m_States [a_WindowID].attribute == FSMState.Attribute.None ? 
			m_StateButtonSize :
			m_SpecialButtonSize;
        GUI.Label(
			new Rect(Vector2.zero, buttonSize),
            m_ScriptableFSM.m_States[a_WindowID].displayName,
            newStyle);

        GUI.DragWindow();
    }

    private void Update()
    {
        if (m_AddingTransition)
            Repaint();
    }

    private void OnSelectionChange()
    {
        SetReferencedDynamicFSM();
        Repaint();
    }

    private void OnInspectorUpdate()
    {
        if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(ScriptableFSM))
            Repaint();
    }

    private void DrawGrid()
    {
        Vector2 lineSpacing = new Vector2(12f, 12f);
        Handles.BeginGUI();
        {
			Handles.color = new Color(93f / 255f, 93f / 255f, 93f / 255f);
            Handles.DrawAAConvexPolygon(
                new Vector3(0, 0),
                new Vector3(0, position.height),
                new Vector3(position.width, position.height),
                new Vector3(position.width, 0));

            Handles.color = new Color(0f, 0f, 0f, 0.5f);
            for (int i = 0; i < position.width / lineSpacing.x; ++i)
            {
                Handles.DrawAAPolyLine(
                    1f,
                    new Vector3(i * lineSpacing.x, 0),
                    new Vector3(i * lineSpacing.x, position.height));
            }
            for (int i = 0; i < position.width / lineSpacing.y; ++i)
            {
                Handles.DrawAAPolyLine(
                    1f,
                    new Vector3(0, i * lineSpacing.y),
                    new Vector3(position.width, i * lineSpacing.y));
            }

            Handles.color = new Color(0f, 0f, 0f, 0.6f);
            for (int i = 0; i < position.width / (lineSpacing.x * 10f); ++i)
            {
                Handles.DrawAAPolyLine(
                    1f,
                    new Vector3(i * lineSpacing.x * 10f, 0),
                    new Vector3(i * lineSpacing.x * 10f, position.height));
            }
            for (int i = 0; i < position.width / (lineSpacing.y * 10f); ++i)
            {
                Handles.DrawAAPolyLine(
                    1f,
                    new Vector3(0, i * lineSpacing.y * 10f),
                    new Vector3(position.width, i * lineSpacing.y * 10f));
            }
        }
        Handles.EndGUI();
    }

    private void AddState()
    {
        m_ScriptableFSM.AddState(a_Position: m_MousePosition);

        EditorUtility.SetDirty(m_ScriptableFSM);
        AssetDatabase.Refresh();
    }
    private void RemoveState(object a_Obj)
    {
        m_ScriptableFSM.RemoveState((FSMState)a_Obj);

        EditorUtility.SetDirty(m_ScriptableFSM);
        AssetDatabase.Refresh();
    }

    private void AddTransition(object a_Obj)
    {
        m_AddingTransition = true;
        m_TransitionAnchor = a_Obj as FSMState;

        EditorUtility.SetDirty(m_ScriptableFSM);
        AssetDatabase.Refresh();
    }
    private void RemoveTransition(object a_Obj)
    {
        FSMTransition transition = a_Obj as FSMTransition;

        transition.state.fromState.RemoveTransition(transition);

        EditorUtility.SetDirty(m_ScriptableFSM);
        AssetDatabase.Refresh();
    }

    private void CreateGeneralContextMenu()
    {
        m_ContextMenu = new GenericMenu();
        m_ContextMenu.AddItem(new GUIContent("Create/New State"), false, AddState);
    }

    private void CreateWindowContextMenu(FSMState a_State)
    {
        m_ContextMenu = new GenericMenu();

        m_ContextMenu.AddItem(new GUIContent("Add/Transition"), false, AddTransition, a_State);
        m_ContextMenu.AddItem(
            new GUIContent("Delete/'" + a_State.displayName + "'"),
            false,
            RemoveState,
            a_State);

        if (a_State.transitions.Count == 0)
            return;

        foreach (var transition in a_State.transitions)
        {
            m_ContextMenu.AddItem(
                new GUIContent("Delete/Transition/" + "'" + transition.displayName + "'"),
                false,
                RemoveTransition,
                transition);
        }
    }

    private void SetReferencedDynamicFSM()
    {
        if (Selection.activeGameObject == null ||
            Selection.activeGameObject.GetComponent<MonoFSM>() == null ||
            Selection.activeGameObject.GetComponent<MonoFSM>().scriptableFSM == null)
            return;

        m_ScriptableFSM = Selection.activeGameObject.GetComponent<MonoFSM>().scriptableFSM;
        m_GUISkin = EditorGUIUtility.Load("MyGUISkin.guiskin") as GUISkin;
    }

    private static Vector3 ScalePosition(EditorWindow a_Window, Vector2 a_Position)
    {
        float tabHeight = 22f;
        Vector3 scaledPosition =
            new Vector3(
                a_Position.x / a_Window.position.width,
                (a_Window.position.height - a_Position.y) / (a_Window.position.height + tabHeight));

        return scaledPosition;
    }
}
