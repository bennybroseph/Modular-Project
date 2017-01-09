using UnityEngine;

public class DelayedCanvas : MonoBehaviour
{
    [SerializeField]
    private Camera m_AnchoredCamera;

    [SerializeField]
    private bool m_LockRotation;

    [SerializeField]
    private Vector2 m_MovementScale = new Vector2(50f, 25f);

    [SerializeField]
    private Vector2 m_RotationMovementScale = new Vector2(1f, 1f);

    private bool m_UseMainCamera;

    private Vector3 m_CurrentRotation;

    private Vector3 m_PreviousRotation;
    private Vector3 m_PreviousPosition;

    private void Start()
    {
        if (m_AnchoredCamera == null)
            m_UseMainCamera = true;

        if (m_UseMainCamera)
            m_AnchoredCamera = Camera.main;

        m_PreviousRotation = m_AnchoredCamera.transform.eulerAngles;
        m_PreviousPosition = m_AnchoredCamera.transform.position;
    }

    private void LateUpdate()
    {
        if (m_UseMainCamera)
            m_AnchoredCamera = Camera.main;

        var rectTransform = GetComponent<RectTransform>();

        if (m_AnchoredCamera.transform.position != m_PreviousPosition)
        {
            var deltaPosition =
                Vector3.ProjectOnPlane(
                    m_PreviousPosition - m_AnchoredCamera.transform.position,
                    m_AnchoredCamera.transform.forward);

            rectTransform.anchoredPosition +=
                new Vector2(
                    deltaPosition.x * m_MovementScale.x,
                    deltaPosition.y * m_MovementScale.y);
        }
        else if (rectTransform.anchoredPosition.magnitude <= 0.5f)
            rectTransform.anchoredPosition = Vector2.zero;

        if (m_CurrentRotation != m_PreviousRotation)
        {
            var deltaRotation = new Vector3(
                Mathf.DeltaAngle(m_AnchoredCamera.transform.eulerAngles.x, m_PreviousRotation.x),
                Mathf.DeltaAngle(m_AnchoredCamera.transform.eulerAngles.y, m_PreviousRotation.y),
                Mathf.DeltaAngle(m_AnchoredCamera.transform.eulerAngles.z, m_PreviousRotation.z));

            m_CurrentRotation += deltaRotation;

            rectTransform.anchoredPosition +=
                new Vector2(
                    deltaRotation.y * m_RotationMovementScale.x,
                    deltaRotation.x * m_RotationMovementScale.y);
        }
        else if (m_CurrentRotation.magnitude <= 0.5f)
            m_CurrentRotation = Vector2.zero;

        rectTransform.anchoredPosition =
            Vector3.Lerp(
                rectTransform.anchoredPosition,
                Vector3.zero, 0.1f);

        m_CurrentRotation =
            new Vector3(
                Mathf.LerpAngle(m_CurrentRotation.x, 0f, 0.1f),
                Mathf.LerpAngle(m_CurrentRotation.y, 0f, 0.1f),
                Mathf.LerpAngle(m_CurrentRotation.z, 0f, 0.1f));

        if (!m_LockRotation)
            transform.eulerAngles = m_CurrentRotation;

        m_PreviousRotation = m_AnchoredCamera.transform.eulerAngles;
        m_PreviousPosition = m_AnchoredCamera.transform.position;
    }
}
