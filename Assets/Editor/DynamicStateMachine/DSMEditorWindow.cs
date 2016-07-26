﻿using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Library;

namespace DynamicStateMachine
{
	public class DSMEditorWindow : EditorWindow
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

		private DSMObject m_DSMObject;

	    private GenericMenu m_ContextMenu;

	    private DSMState m_TransitionAnchor;
	    private bool m_AddingTransition;

	    private Vector2 m_MousePosition;

	    private GUISkin m_GUISkin;

	    [MenuItem("Window/Dynamic State Machine")]
	    private static void ShowEditor()
	    {
	        var editor = GetWindow<DSMEditorWindow>();
	        editor.titleContent = new GUIContent("DSM");
	        editor.SetReferencedDSM();
	    }

	    private DSMEditorWindow()
	    {
	        SetReferencedDSM();
	    }

	    private void OnFocus()
	    {
	        SetReferencedDSM();
	    }
	    private void OnEnable()
	    {
	        SetReferencedDSM();
	    }

	    private void OnGUI()
	    {
	        DrawGrid();

	        if (m_DSMObject == null)
	            return;

			Dictionary<DSMTransition, Rect> transitionLineRects = new Dictionary<DSMTransition, Rect>();

	        Handles.BeginGUI();
	        {
				if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(DSMState)) 
				{
					var activeState = Selection.activeObject as DSMState;

					float width = 5f;
					Handles.color = new Color(1, 1, 1, 0.2f);

					Vector2 buttonSize = activeState.attribute == DSMState.Attribute.None ? 
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

	            foreach (var state in m_DSMObject.m_States)
	            {
	                foreach (var transition in state.transitions)
	                {
						Vector2 fromButtonSize = transition.state.fromState.attribute == DSMState.Attribute.None ? 
							m_StateButtonSize :
							m_SpecialButtonSize;
						Vector2 toButtonSize = transition.state.toState.attribute == DSMState.Attribute.None ? 
							m_StateButtonSize :
							m_SpecialButtonSize;	
						
	                    float radius = 5f;
	                    float angle =
	                        Mathf.PI / 2f + Mathf.Atan2(
								(transition.state.toState.position.y + toButtonSize.y / 2f) - 
								(transition.state.fromState.position.y + fromButtonSize.y / 2f),
								(transition.state.toState.position.x + toButtonSize.x / 2f) - 
								(transition.state.fromState.position.x + fromButtonSize.x / 2f));									
						
	                    List<Vector2> linePositions = new List<Vector2>
	                    {
	                        new Vector2(
								transition.state.fromState.position.x + fromButtonSize.x / 2f
	                            + radius * Mathf.Cos(angle),
								transition.state.fromState.position.y + fromButtonSize.y / 2f
	                            + radius * Mathf.Sin(angle)),
	                        new Vector2(
								transition.state.toState.position.x + toButtonSize.x / 2f
	                            + radius * Mathf.Cos(angle),
								transition.state.toState.position.y + toButtonSize.y / 2f
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

						float selectionSize = radius + 5f;
						transitionLineRects.Add (
							transition,
							new Rect (
								new Vector2 (between.x - selectionSize / 2, between.y - selectionSize / 2),
								new Vector2 (selectionSize, selectionSize)));

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
	            for (int i = 0; i < m_DSMObject.m_States.Count; ++i)
	            {
	                Vector2 buttonSize = new Vector2(50f, 50f);
	                switch (m_DSMObject.m_States[i].attribute)
	                {
	                    case DSMState.Attribute.None:
	                        GUI.color = m_NormalButtonColor;
	                        buttonSize = m_StateButtonSize;
	                        break;
	                    case DSMState.Attribute.Entry:
	                        GUI.color = m_EntryButtonColor;
	                        buttonSize = m_SpecialButtonSize;
	                        break;
	                    case DSMState.Attribute.FromAny:
	                        GUI.color = m_AnyStateButtonColor;
	                        buttonSize = m_SpecialButtonSize;
	                        break;
	                }

	                Rect windowRect =
	                    new Rect(
	                        m_DSMObject.m_States[i].position.x,
	                        m_DSMObject.m_States[i].position.y,
	                        buttonSize.x,
	                        buttonSize.y);

					if (m_DSMObject.m_EntryPoint == m_DSMObject.m_States[i])
						GUI.color = new Color32 (255, 152, 0, 255);
					
	                windowRect =
	                    GUI.Window(
	                        i,
	                        windowRect,
	                        DrawNodeWindow,
	                        "",
	                        m_GUISkin.button);

	                m_DSMObject.m_States[i].position = new Vector2(windowRect.x, windowRect.y);
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
							
	                        Selection.activeObject = m_DSMObject;
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
	                        Selection.activeObject = m_DSMObject.m_States[a_WindowID];

	                        if (repaintEvent != null)
	                            repaintEvent();
	                    }
	                    if (Event.current.button == 1)
	                    {
	                        if (!m_AddingTransition)
	                        {
	                            CreateWindowContextMenu(m_DSMObject.m_States[a_WindowID]);
	                            m_ContextMenu.ShowAsContext();
	                        }
	                        else
	                            m_AddingTransition = false;
	                    }
	                    if (Event.current.button == 0 && m_AddingTransition)
	                    {
	                        DSMTransition newTransition = m_TransitionAnchor.AddFromTransition(
	                            m_DSMObject.m_States[a_WindowID]);

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

			Vector2 buttonSize = m_DSMObject.m_States [a_WindowID].attribute == DSMState.Attribute.None ? 
				m_StateButtonSize :
				m_SpecialButtonSize;
	        GUI.Label(
				new Rect(Vector2.zero, buttonSize),
	            m_DSMObject.m_States[a_WindowID].displayName,
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
	        SetReferencedDSM();
	        Repaint();
	    }

	    private void OnInspectorUpdate()
	    {
	        if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(DSMObject))
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
	        m_DSMObject.AddState(a_Position: m_MousePosition);

	        EditorUtility.SetDirty(m_DSMObject);
	        AssetDatabase.Refresh();
	    }
	    private void RemoveState(object a_Obj)
	    {
	        m_DSMObject.RemoveState((DSMState)a_Obj);

	        EditorUtility.SetDirty(m_DSMObject);
	        AssetDatabase.Refresh();
	    }

	    private void AddTransition(object a_Obj)
	    {
	        m_AddingTransition = true;
	        m_TransitionAnchor = a_Obj as DSMState;

	        EditorUtility.SetDirty(m_DSMObject);
	        AssetDatabase.Refresh();
	    }
	    private void RemoveTransition(object a_Obj)
	    {
	        DSMTransition transition = a_Obj as DSMTransition;

	        transition.state.fromState.RemoveTransition(transition);

	        EditorUtility.SetDirty(m_DSMObject);
	        AssetDatabase.Refresh();
	    }

	    private void CreateGeneralContextMenu()
	    {
	        m_ContextMenu = new GenericMenu();
	        m_ContextMenu.AddItem(new GUIContent("Create/New State"), false, AddState);
	    }

	    private void CreateWindowContextMenu(DSMState a_State)
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

	    private void SetReferencedDSM()
	    {
	        if (Selection.activeGameObject == null ||
	            Selection.activeGameObject.GetComponent<MonoDSM>() == null ||
	            Selection.activeGameObject.GetComponent<MonoDSM>().dsmObject == null)
	            return;

			m_DSMObject = Selection.activeGameObject.GetComponent<MonoDSM>().dsmObject;
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
}