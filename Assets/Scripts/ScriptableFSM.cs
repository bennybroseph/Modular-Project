using Library;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFSM", menuName = "Scriptable FSM")]
public class ScriptableFSM : ScriptableObject
{
    public DynamicFSM dynamicFSM;
    public List<Vector2> windowPositions;

    private List<string> m_Keys;
    private List<DynamicFSM.IsValidateAction> m_Values;

    private void OnEnable()
    {
        for (int i = 0; i < m_Keys.Count; i++)
        {
            string[] states = DynamicFSM.ParseStates(m_Keys[i]);
            dynamicFSM.AddTransition(states[0], states[1]);
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Destroyed " + name);
    }
    private void OnDisable()
    {
        m_Keys = new List<string>();
        m_Values = new List<DynamicFSM.IsValidateAction>();

        foreach (var keyValuePair in dynamicFSM.transitions)
        {
            m_Keys.Add(keyValuePair.Key);
            m_Values.Add(keyValuePair.Value);
        }
    }
}
