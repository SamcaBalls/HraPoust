using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyStarter : MonoBehaviour
{
    [SerializeField] private Blackscreen blackscreen;
    private SteamLobby steamLobby;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // Zůstane mezi scénami
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu")
        {
            Debug.Log("[LobbyStarter] Menu načteno, startuji lobby...");
            StartCoroutine(StartLobbyRoutine());
        }
    }

    private IEnumerator StartLobbyRoutine()
    {
        while (steamLobby == null)
        {
            steamLobby = FindAnyObjectByType<SteamLobby>();
            yield return null;
        }

        while (!SteamManager.Initialized)
            yield return null;

        if (steamLobby.lobbyID != 0)
            steamLobby.LeaveLobby();

        steamLobby.OnLobbyReady += HandleLobbyReady;
        steamLobby.HostLobby();
    }

    private void HandleLobbyReady()
    {
        Debug.Log("[OnGameStart] Lobby ready!");
        StartCoroutine(CloseLobbyAndFade());
    }

    private IEnumerator CloseLobbyAndFade()
    {
        yield return new WaitForSeconds(0.5f);
        steamLobby.CloseLobby();
        yield return StartCoroutine(blackscreen.FadeRoutine(false));
    }
}
