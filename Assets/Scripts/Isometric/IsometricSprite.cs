using Library;

using UnityEngine;

namespace Isometric
{
    [ExecuteInEditMode]
    public class IsometricSprite : ExecuteInEditor
    {
        [SerializeField]
        private Camera m_Camera;

        protected override void OnEditorStart()
        {
            OnGameStart();
        }
        protected override void OnEditorUpdateSelected()
        {
            //throw new System.NotImplementedException();
        }
        protected override void OnEditorUpdate()
        {
            //throw new System.NotImplementedException();
        }
        protected override void OnGameStart()
        {
            if (m_Camera)
                transform.eulerAngles = m_Camera.transform.eulerAngles;


        }
        protected override void OnGameUpdate()
        {
            //throw new System.NotImplementedException();
        }
    }
}
