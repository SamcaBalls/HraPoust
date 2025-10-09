using System.Collections;
using UnityEngine;

public class OnGameStart : MonoBehaviour
{
    [SerializeField] SteamLobby steamLobby;
    [SerializeField] CanvasGroup fadeGroup;
    [SerializeField] float fadeDuration = 1f;
    [SerializeField] GameObject image;

    private void Start()
    {
        steamLobby.OnLobbyReady += HandleLobbyReady;
        steamLobby.HostLobby();
    }

    private void HandleLobbyReady()
    {
        Debug.Log("Lobby ready – provádím fade a zavírám lobby...");
        StartCoroutine(CloseAndFade());
    }

    private IEnumerator CloseAndFade()
    {
        // Počkej chvilku, aby Steam stihl vše dokončit
        yield return new WaitForSeconds(0.5f);

        steamLobby.CloseLobby(); // bezpečně uzavře lobby
        yield return FadeRoutine(false); // fade efekt
    }

    private IEnumerator FadeRoutine(bool fadeIn)
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
    }
}
