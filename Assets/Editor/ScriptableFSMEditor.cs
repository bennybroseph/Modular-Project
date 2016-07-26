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
        serializedObject.Update();
        {
			
        }
        serializedObject.ApplyModifiedProperties();

        if(GUILayout.Button("Initialize"))
            m_ScriptableFSM.Init();
    }
}
