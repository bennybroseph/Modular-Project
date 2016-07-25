using UnityEngine;
using UnityEditor;
using Library;
using UnityEditorInternal;

[CustomEditor(typeof(ScriptableFSM))]
public class ScriptableFSMEditor : Editor
{
    private ScriptableFSM m_ScriptableFSM;

    private string m_FocusedControl;
    private string m_CurrentState;

    private ReorderableList m_ReorderableList;

    void OnEnable()
    {
        ScriptableFSMWindow.repaintEvent += Repaint;

        m_ScriptableFSM = target as ScriptableFSM;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //serializedObject.Update();
        //{
        //    //string oldName = m_ScriptableFSM.dynamicFSM.states[ScriptableFSMWindow.s_FocusedState];
        //    //string newName = EditorGUILayout.DelayedTextField(oldName);

        //    //if (newName != oldName)
        //    //    m_ScriptableFSM.dynamicFSM.RenameState(oldName, newName);

        //    //m_CurrentState = m_ScriptableFSM.dynamicFSM.states[ScriptableFSMWindow.s_FocusedState];

        //    m_ReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("dynamicFSM").FindPropertyRelative("m_Transitions"), true, true, false, false);
        //    m_ReorderableList.drawElementCallback = DrawTransitionElement;

        //    EditorGUILayout.Space();
        //    m_ReorderableList.DoLayoutList();
        //    EditorGUILayout.Space();

        //    if (GUILayout.Button("Invoke Transition Check"))
        //        m_ScriptableFSM.dynamicFSM.Transition(m_CurrentState);
        //}
        //serializedObject.ApplyModifiedProperties();
        if(GUILayout.Button("Initialize"))
            m_ScriptableFSM.Init();
    }

    //private void DrawTransitionElement(Rect a_Rect, int a_Index, bool a_IsActive, bool a_IsFocused)
    //{
    //    var element = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(a_Index);

    //    DynamicFSM.TransitionType both = DynamicFSM.TransitionType.To | DynamicFSM.TransitionType.From;

    //    foreach (var transition in m_ScriptableFSM.dynamicFSM.GetTransitionsOnState(m_CurrentState, both))
    //    {
    //        if (transition == element.stringValue)
    //            EditorGUI.LabelField(
    //                new Rect(a_Rect.x, a_Rect.y, a_Rect.width, EditorGUIUtility.singleLineHeight),
    //                new GUIContent(element.stringValue));
    //    }
    //}
}
