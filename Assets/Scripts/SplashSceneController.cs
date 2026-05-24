using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SplashSceneController : MonoBehaviour
{
    public RawImage logoImage; 
    public float fadeDuration = 1.5f; 
    public float stayDuration = 1.5f;

    private void Start()
    {
        StartCoroutine(PlaySplashSequence());
    }

    IEnumerator PlaySplashSequence()
    {
        yield return StartCoroutine(FadeImage(0f, 1f, fadeDuration));

        yield return new WaitForSeconds(stayDuration);

        yield return StartCoroutine(FadeImage(1f, 0f, fadeDuration));
        yield return StartCoroutine(CrazyGamesIntegration.EnsureInitialized());

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    IEnumerator FadeImage(float fromAlpha, float toAlpha, float duration)
    {
        float timer = 0f;
        Color color = logoImage.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, timer / duration);
            color.a = alpha;
            logoImage.color = color;
            yield return null;
        }

        color.a = toAlpha;
        logoImage.color = color;
    }
}
