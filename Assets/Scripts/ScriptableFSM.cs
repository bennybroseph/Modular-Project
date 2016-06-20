using Library;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFSM", menuName = "Scriptable FSM")]
public class ScriptableFSM : ScriptableObject
{
    public DynamicFSM dynamicFSM;
}
