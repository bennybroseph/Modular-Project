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

            if (!scriptableFSM.dynamicFSM.transitions.ContainsKey(
                DynamicFSM.CreateKey("Test State 1", "Test State 2")))
                scriptableFSM.dynamicFSM.AddTransition("Test State 1", "Test State 2", TestTransitionCheck);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        scriptableFSM.OnDisable();
    }

    public bool TestTransitionCheck()
    {
        Debug.Log("Transition Successful");
        return true;
    }
}
