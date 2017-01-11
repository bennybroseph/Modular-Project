using UnityEngine;

public class OldSchoolRotation : MonoBehaviour
{
    [SerializeField]
    private Transform m_ReferenceTransform;

    private GameObject m_AnchorObject;

    // Use this for initialization
    private void Awake()
    {
        m_AnchorObject = new GameObject("New Anchor");

        m_AnchorObject.transform.forward = m_ReferenceTransform.forward;
        transform.SetParent(m_AnchorObject.transform, false);
    }

    // Update is called once per frame
    private void Update()
    {
        m_AnchorObject.transform.forward = m_ReferenceTransform.forward;
        //transform.forward = m_ReferenceTransform.forward;

        var newRotationY = m_ReferenceTransform.eulerAngles.y;
        newRotationY += (int)((newRotationY + 22.5f)/ 45f) * -45f;

        m_AnchorObject.transform.eulerAngles = new Vector3(0, newRotationY, 0);

        //transform.forward = new Vector3();
    }
}
