using Library;
using Library.GeneticAlgorithm;
using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(ParseCNF))]
public class ParseCNFEditor : Editor
{
    private ParseCNF m_Self;

    private void OnEnable()
    {
        m_Self = target as ParseCNF;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Parse"))
            m_Self.Parse();
    }
}
