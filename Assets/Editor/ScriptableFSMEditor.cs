using UnityEngine;
using UnityEditor;
using System.Collections;
using Library;

[CustomEditor(typeof(ScriptableFSM))]
public class ScriptableFSMEditor : Editor
{
    private ScriptableFSM m_ScriptableFSM;

    private string m_FocusedControl;
    private string m_CurrentText;

    void OnEnable()
    {
        ScriptableFSMWindow.repaintEvent += Repaint;

        m_ScriptableFSM = target as ScriptableFSM;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        {
            string oldName = m_ScriptableFSM.dynamicFSM.states[ScriptableFSMWindow.s_FocusedState];
            string newName = EditorGUILayout.DelayedTextField(oldName);

            if (newName != oldName)
                m_ScriptableFSM.dynamicFSM.RenameState(oldName, newName);

            EditorGUILayout.Space();

            string currentState = m_ScriptableFSM.dynamicFSM.states[ScriptableFSMWindow.s_FocusedState];
            DynamicFSM.TransitionType both = DynamicFSM.TransitionType.To | DynamicFSM.TransitionType.From;

            foreach (string[] states in m_ScriptableFSM.dynamicFSM.GetTransitionsOnState(currentState, both))
            {
                EditorGUILayout.LabelField(states[0] + " -> " + states[1]);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
