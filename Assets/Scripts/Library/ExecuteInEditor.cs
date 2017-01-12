using UnityEngine;  // Required for all 'MonoBehavior's

#if UNITY_EDITOR
using UnityEditor;  // Required for 'EditorApplication'
#endif

namespace Library    // namespace denoting that this is part of the tile mapping functionality inside the editor
{
    // Executes 'Start()', and 'Update()' while in the editor
    [ExecuteInEditMode]
    public abstract class ExecuteInEditor : MonoBehaviour
    {
        private bool m_EditorCompiling;   // Used to determine whether Unity is currently compiling

        /// <summary>
        /// Run automatically by Unity on 3 known conditions
        /// 1. Unity starts up
        /// 2. Unity enters play mode
        /// 3. Unity exits play mode
        /// </summary>
        private void Start()
        {
            m_EditorCompiling = false;

#if UNITY_EDITOR
            // Unity is in play mode
            if (EditorApplication.isPlaying)
#endif
                OnGameStart();
#if UNITY_EDITOR
            // Unity started up || Game stopped running
            else
                OnEditorStart();
#endif
        }
        /// <summary>
        /// Run automatically by Unity on 2 known conditions
        /// 1. Unity is in play mode. Happens every frame
        /// 2. Unity is not in play mode. Happens when something changes in the scene
        /// </summary>
        private void Update()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
#endif
                OnGameUpdate();
#if UNITY_EDITOR
            // Unity is not in play mode and it is compiling without us knowing
            else if (EditorApplication.isCompiling && !m_EditorCompiling)
                m_EditorCompiling = true;
            // Unity is not compiling, but it WAS compiling last we knew
            else if (m_EditorCompiling)
            {
                m_EditorCompiling = false;
                OnEditorStart();    // Run initialization code again
            }
            // Unity was not compiling last we knew
            // A game object is currently selected and that 'gameObject' is this one or this 'gameObject's parent
            else if (Selection.activeGameObject != null
                    && (Selection.activeGameObject == gameObject || Selection.activeGameObject.transform.parent == transform))
                OnEditorUpdateSelected();
            // Nothing currently selected or what IS selected isn't this 'gameObject' or this 'gameObject's parent
            else
                OnEditorUpdate();
#endif
        }

        // Virtual Functions to be implemented by inheriting class
        protected abstract void OnEditorStart();
        protected abstract void OnEditorUpdateSelected();
        protected abstract void OnEditorUpdate();

        protected abstract void OnGameStart();
        protected abstract void OnGameUpdate();
    }
}