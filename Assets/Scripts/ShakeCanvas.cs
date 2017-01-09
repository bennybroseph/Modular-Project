using System;
using System.Collections;

using UnityEngine;

using Random = UnityEngine.Random;

public class ShakeCanvas : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve m_AnimationCurve;

    [SerializeField]
    private float m_AnimationTime = 3f;

    [SerializeField]
    private Vector2 m_MinDistance = new Vector2(2f, 2f);
    [SerializeField]
    private Vector2 m_MaxDistance = new Vector2(5f, 5f);

    // Use this for initialization
    private void Start()
    {
        Random.InitState((int)DateTime.Now.Ticks);
    }

    [ContextMenu("Shake")]
    public void Shake()
    {
        StartCoroutine(ShakeAnimation());
    }

    private IEnumerator ShakeAnimation()
    {
        var rectTransform = GetComponent<RectTransform>();
        var originalPosition = rectTransform.anchoredPosition;

        var deltatTime = 0f;
        while (deltatTime < m_AnimationTime)
        {
            var animationValue = m_AnimationCurve.Evaluate(deltatTime / m_AnimationTime);

            var positiveX = Random.Range(0, 2) == 0 ? 1 : -1;
            var randomX = positiveX * animationValue * Random.Range(m_MinDistance.x, m_MaxDistance.x);

            var positiveY = Random.Range(0, 2) == 0 ? 1 : -1;
            var randomY = positiveY * animationValue * Random.Range(m_MinDistance.y, m_MaxDistance.y);

            rectTransform.anchoredPosition =
                originalPosition + new Vector2(randomX, randomY);

            deltatTime += Time.deltaTime;

            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
    }
}
