using Library;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class MonoFSM : MonoBehaviour
{
    [SerializeField]
    public ScriptableFSM scriptableFSM;

    // Use this for initialization
    void Start()
    {
        if (!EditorApplication.isPlaying)
        {
            if (!scriptableFSM.dynamicFSM.states.Contains("Test State 1"))
                scriptableFSM.dynamicFSM.AddState("Test State 1");
            if (!scriptableFSM.dynamicFSM.states.Contains("Test State 2"))
                scriptableFSM.dynamicFSM.AddState("Test State 2");
            
            scriptableFSM.dynamicFSM.AddTransition("Test State 1", "Test State 2", MethodTransitionCheck);
            scriptableFSM.dynamicFSM.AddTransition("Test State 2", "Test State 1", StaticCheck);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool MethodTransitionCheck()
    {
        Debug.Log("Method Transition Check Successful");
        return true;
    }

    private static bool StaticCheck()
    {
        Debug.Log("Static Transition Check Successful");
        return true;
    }
}
