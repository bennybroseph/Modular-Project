using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Library
{
    using System.Collections;

    using UnityEngine;

    public class SceneLoader : MonoBehaviour
    {
        [SerializeField]
        private string m_SceneName;
        [SerializeField]
        private Image m_Image;

        [SerializeField, Range(0f, 1f)]
        private float m_LoadingProgress;

        public float loadingProgress { get { return m_LoadingProgress; } }

        // Use this for initialization
        private void Awake()
        {
            StartCoroutine(WaitForLoadScene());
        }

        // Update is called once per frame
        private IEnumerator WaitForLoadScene()
        {
            while (true)
            {
                while (!Input.GetKeyDown(KeyCode.Return))
                    yield return null;

                var asyncOperation = SceneManager.LoadSceneAsync(m_SceneName);

                while (!asyncOperation.isDone)
                {
                    m_LoadingProgress = asyncOperation.progress;

                    if (m_Image != null)
                        m_Image.fillAmount = m_LoadingProgress;

                    yield return null;
                }

                m_LoadingProgress = 1f;

                yield return null;
            }
        }
    }
}
