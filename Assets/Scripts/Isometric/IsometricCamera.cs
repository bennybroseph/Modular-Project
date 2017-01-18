using System;
using System.Collections;
using UnityEngine;

namespace Isometric
{
    public class IsometricCamera : MonoBehaviour
    {
        [SerializeField]
        private Camera m_Camera;

        [Header("Zoom"), Space, SerializeField]
        private float m_ZoomContinueTime = 0.15f;
        [SerializeField]
        private float m_ZoomAmount = 2.5f;
        [Space, SerializeField]
        private float m_ZoomStopTime = 0.25f;
        [SerializeField]
        private AnimationCurve m_ZoomStopCurve;

        [Space, SerializeField]
        private float m_MinZoom = 3f;
        [SerializeField]
        private float m_MaxZoom = 10f;

        [Space, Header("Translation"), Space, SerializeField]
        private float m_TranslationContinueTime = 0.15f;
        [SerializeField]
        private Vector3 m_TranslationAmount = new Vector3(2.5f, 2.5f, 2.5f);
        [Space, SerializeField]
        private float m_TranslationStopTime = 0.25f;
        [SerializeField]
        private AnimationCurve m_TranslationStopCurve;
        [Space, SerializeField]
        private AnimationCurve m_MoveToTargetCurve;

        [Space, Header("Rotation"), Space, SerializeField]
        private float m_RotationTime = 1f;
        [SerializeField]
        private AnimationCurve m_RotationCurve;
        [SerializeField]
        private float m_RotationAmount = 45f;

        private float m_ZoomTimer;

        private Vector3 m_TranslationDirection;

        private Coroutine m_ZoomCoroutine;
        private Coroutine m_ZoomStopCoroutine;

        private Coroutine m_TranslationCoroutine;
        private Coroutine m_TranslationStopCoroutine;

        private Coroutine m_RotationCoroutine;

        public void MoveToTarget(Vector3 newPosition, float duration)
        {
            if (m_TranslationCoroutine == null)
                m_TranslationCoroutine = StartCoroutine(OnMoveToTarget(newPosition, duration));
        }

        private void Awake()
        {
            if (m_Camera == null)
                m_Camera = GetComponentInChildren<Camera>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (m_RotationCoroutine == null)
            {
                if (Input.GetKey(KeyCode.Q))
                    m_RotationCoroutine = StartCoroutine(Rotate());
                else if (Input.GetKey(KeyCode.E))
                    m_RotationCoroutine = StartCoroutine(Rotate(-1f));
            }

            if (Input.mouseScrollDelta.y != 0f)
            {
                m_ZoomTimer = m_ZoomContinueTime;
                if (m_ZoomCoroutine == null)
                {
                    if (m_ZoomStopCoroutine != null)
                        StopCoroutine(m_ZoomStopCoroutine);

                    m_ZoomCoroutine =
                        StartCoroutine(
                            Zoom(Input.mouseScrollDelta.y < 0f ? 1 : -1));
                }
            }

            if (Input.GetKey(KeyCode.W) ||
                Input.GetKey(KeyCode.S) ||
                Input.GetKey(KeyCode.A) ||
                Input.GetKey(KeyCode.D))
            {
                if (Input.GetKey(KeyCode.W))
                    m_TranslationDirection =
                        new Vector3(m_TranslationDirection.x, m_TranslationDirection.y, 1f);
                else if (Input.GetKey(KeyCode.S))
                    m_TranslationDirection =
                        new Vector3(m_TranslationDirection.x, m_TranslationDirection.y, -1f);
                else
                    m_TranslationDirection =
                        new Vector3(m_TranslationDirection.x, m_TranslationDirection.y, 0f);

                if (Input.GetKey(KeyCode.A))
                    m_TranslationDirection =
                        new Vector3(-1f, m_TranslationDirection.y, m_TranslationDirection.z);
                else if (Input.GetKey(KeyCode.D))
                    m_TranslationDirection =
                        new Vector3(1f, m_TranslationDirection.y, m_TranslationDirection.z);
                else
                    m_TranslationDirection =
                        new Vector3(0, m_TranslationDirection.y, m_TranslationDirection.z);

                if (transform.eulerAngles.y > 90f && transform.eulerAngles.y < 270f)
                    m_TranslationDirection = -m_TranslationDirection;

                if (m_TranslationCoroutine == null)
                {
                    if (m_TranslationStopCoroutine != null)
                        StopCoroutine(m_TranslationStopCoroutine);

                    m_TranslationCoroutine = StartCoroutine(Translate());
                }
            }
            else
                m_TranslationDirection = Vector3.zero;

            if (Input.GetKeyDown(KeyCode.T))
                MoveToTest();
        }

        private IEnumerator Zoom(float coefficient = 1f)
        {
            var totalZoom = coefficient * m_ZoomAmount;

            while (m_ZoomTimer > 0f)
            {
                var newSize = m_Camera.orthographicSize + totalZoom * Time.deltaTime;

                m_Camera.orthographicSize = Mathf.Clamp(newSize, m_MinZoom, m_MaxZoom);
                if (Math.Abs(m_Camera.orthographicSize - newSize) > float.Epsilon)
                    break;

                m_ZoomTimer -= Time.deltaTime;

                yield return null;
            }

            m_ZoomCoroutine = null;

            m_ZoomStopCoroutine = StartCoroutine(ZoomStop(coefficient));
        }

        private IEnumerator ZoomStop(float coefficient = 1f)
        {
            var totalZoom = m_ZoomAmount * coefficient;

            var deltaTime = 0f;
            while (deltaTime < m_ZoomStopTime)
            {
                var newSize =
                    m_Camera.orthographicSize +
                    m_ZoomStopCurve.Evaluate(deltaTime / m_ZoomStopTime) * totalZoom * Time.deltaTime;

                m_Camera.orthographicSize = Mathf.Clamp(newSize, m_MinZoom, m_MaxZoom);
                if (Math.Abs(m_Camera.orthographicSize - newSize) > float.Epsilon)
                    break;

                deltaTime += Time.deltaTime;

                yield return null;
            }

            m_ZoomStopCoroutine = null;
        }

        private IEnumerator Translate()
        {
            var previousDirection = m_TranslationDirection;

            while (m_TranslationDirection != Vector3.zero)
            {
                var totalTranslation =
                    new Vector3(
                        m_TranslationDirection.x * m_TranslationAmount.x,
                        m_TranslationDirection.y * m_TranslationAmount.y,
                        m_TranslationDirection.z * m_TranslationAmount.z);

                transform.position += totalTranslation * Time.deltaTime;

                previousDirection = m_TranslationDirection;

                yield return null;
            }

            m_TranslationCoroutine = null;

            m_TranslationStopCoroutine = StartCoroutine(TranslationStop(previousDirection));
        }

        private IEnumerator TranslationStop(Vector3 oldDirection)
        {
            var totalTranslation =
                new Vector3(
                    oldDirection.x * m_TranslationAmount.x,
                    oldDirection.y * m_TranslationAmount.y,
                    oldDirection.z * m_TranslationAmount.z);

            var deltaTime = 0f;
            while (deltaTime < m_TranslationStopTime)
            {
                transform.position +=
                    m_TranslationStopCurve.Evaluate(deltaTime / m_TranslationStopTime) * totalTranslation * Time.deltaTime;

                deltaTime += Time.deltaTime;

                yield return null;
            }

            m_TranslationStopCoroutine = null;
        }

        private IEnumerator Rotate(float coefficient = 1f)
        {
            var totalRotation = coefficient * m_RotationAmount;
            var originalRotationY = transform.eulerAngles.y;

            var deltaTime = 0f;
            while (deltaTime < m_RotationTime)
            {
                var currentRotation = transform.eulerAngles;

                transform.eulerAngles =
                    new Vector3(
                        currentRotation.x,
                        originalRotationY + m_RotationCurve.Evaluate(deltaTime / m_RotationTime) * totalRotation,
                        currentRotation.z);

                deltaTime += Time.deltaTime;

                yield return null;
            }

            transform.eulerAngles =
                new Vector3(
                    transform.eulerAngles.x,
                    originalRotationY + totalRotation,
                    transform.eulerAngles.z);

            m_RotationCoroutine = null;
        }

        private IEnumerator OnMoveToTarget(Vector3 newPosition, float duration)
        {
            var originalPosition = transform.position;
            var deltaPosition = newPosition - originalPosition;

            var deltaTime = 0f;
            while (deltaTime < duration)
            {
                transform.position =
                    originalPosition + m_MoveToTargetCurve.Evaluate(deltaTime / duration) * deltaPosition;

                deltaTime += Time.deltaTime;

                yield return null;
            }

            transform.position = newPosition;

            m_TranslationCoroutine = null;
        }

        private void MoveToTest()
        {
            MoveToTarget(Vector3.zero, 1f);
        }

        private float ClampAngle(float angle)
        {
            while (angle >= 360f)
                angle -= 360f;

            while (angle < 0f)
                angle += 360f;

            Debug.Log(angle);
            return angle;
        }
    }
}
