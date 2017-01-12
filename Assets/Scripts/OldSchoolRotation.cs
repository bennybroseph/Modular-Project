using System.Collections;
using UnityEngine;

public class OldSchoolRotation : MonoBehaviour
{
    [SerializeField]
    private Transform m_CameraTransform;

    [SerializeField]
    private float m_RotationAngle = 45f;
    [SerializeField]
    private float m_ClampOffset;
    [SerializeField]
    private float m_RotationOffset;

    private Vector3 m_CurrentRotation;

    // Use this for initialization
    private void Awake()
    {
        m_CurrentRotation = transform.eulerAngles;
    }

    private void LateUpdate()
    {
        m_CurrentRotation = transform.eulerAngles;

        var newRotationY = ClampAngle(m_CurrentRotation.y);

        var newReferenceRotationY = m_CameraTransform.eulerAngles.y;
        newReferenceRotationY += ClampAngle(newReferenceRotationY);

        transform.eulerAngles =
            new Vector3(
                m_CurrentRotation.x,
                newRotationY + newReferenceRotationY + m_RotationOffset,
                m_CurrentRotation.z);

        StartCoroutine(ResetRotation());
    }

    private float ClampAngle(float angle)
    {
        return (int)((angle + m_ClampOffset) / m_RotationAngle) * -m_RotationAngle;
    }

    private IEnumerator ResetRotation()
    {
        yield return new WaitForEndOfFrame();

        transform.eulerAngles = m_CurrentRotation;
    }
}
