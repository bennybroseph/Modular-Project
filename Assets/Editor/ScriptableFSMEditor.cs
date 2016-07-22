using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ScriptableFSM))]
public class ScriptableFSMEditor : Editor
{
    private SerializedProperty m_DynamicFSM;

    void OnEnable()
    {
        ScriptableFSMWindow.repaintEvent += Repaint;

        m_DynamicFSM = serializedObject.FindProperty("dynamicFSM");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        {
            SerializedProperty states = m_DynamicFSM.FindPropertyRelative("m_States");

            GUILayout.TextField(states.GetArrayElementAtIndex(ScriptableFSMWindow.s_FocusedState).stringValue);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
