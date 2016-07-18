using Library;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFSM", menuName = "Scriptable FSM")]
public class ScriptableFSM : ScriptableObject
{
    public DynamicFSM dynamicFSM;
    public List<Rect> windowPositions;
}
