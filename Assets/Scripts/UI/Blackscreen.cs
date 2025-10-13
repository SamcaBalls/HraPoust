using System.Collections;
using UnityEngine;

public class Blackscreen : MonoBehaviour
{
    [SerializeField] CanvasGroup fadeGroup;
    [SerializeField] float fadeDuration = 1f;
    [SerializeField] GameObject image;


    public IEnumerator FadeRoutine(bool fadeIn)
    {
        image.SetActive(true);

        float start = fadeGroup.alpha;
        float end = fadeIn ? 1f : 0f;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(start, end, time / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = end;

        if (!fadeIn)
        {
            image.SetActive(false);
        }
        Debug.Log("Faded");
    }
}
