namespace TestScripts
{
    using UnityEngine;

    public class ExpandToScreen : MonoBehaviour
    {
        private RectTransform m_ParentRectTransform;
        private Vector2 m_PreviousScreenSize;

        private void LateUpdate()
        {
            m_ParentRectTransform = GetComponentInParent<RectTransform>();

            var currentScreenSize = new Vector2(Screen.width, Screen.height);
            if (m_PreviousScreenSize == currentScreenSize)
                return;

            var index = 0;
            foreach (RectTransform childTransform in transform)
            {
                if (Screen.width > Screen.height)
                {
                    childTransform.SetInsetAndSizeFromParentEdge(
                        index == 0 ? RectTransform.Edge.Left : RectTransform.Edge.Right,
                        0,
                        m_ParentRectTransform.rect.width / 2f);

                    childTransform.SetInsetAndSizeFromParentEdge(
                        RectTransform.Edge.Top,
                        0,
                        m_ParentRectTransform.rect.height);
                }
                else
                {
                    childTransform.SetInsetAndSizeFromParentEdge(
                        RectTransform.Edge.Left,
                        0,
                        m_ParentRectTransform.rect.width);

                    childTransform.SetInsetAndSizeFromParentEdge(
                        index == 0 ? RectTransform.Edge.Top : RectTransform.Edge.Bottom,
                        0,
                        m_ParentRectTransform.rect.height / 2f);
                }

                ++index;
            }

            m_PreviousScreenSize = currentScreenSize;
        }
    }
}
