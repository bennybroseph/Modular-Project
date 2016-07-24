using System;
using Library;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "NewFSM", menuName = "Scriptable FSM")]
public class ScriptableFSM : ScriptableObject
{
    public DynamicFSM dynamicFSM;
    public List<Vector2> windowPositions;
}
