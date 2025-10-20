using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyStarter : MonoBehaviour
{
    [SerializeField] private Blackscreen blackscreen;
    private SteamLobby steamLobby;


    private void Start()
    {
        CoroutineStart();
    }

    public void CoroutineStart()
    {
        if(steamLobby != null && steamLobby.lobbyID != 0)
        {
            steamLobby.CloseLobby();
        }
        StartCoroutine(StartLobbyRoutine());
        Debug.Log("[LobbyStarter] Started coroutine");
    }

    private IEnumerator StartLobbyRoutine()
    {
        while (steamLobby == null)
        {
            steamLobby = FindAnyObjectByType<SteamLobby>(FindObjectsInactive.Include);
            yield return null;
        }
        steamLobby.gameObject.SetActive(true);
        steamLobby.OnLobbyReady += HandleLobbyReady;
        Debug.Log("[LobbyStarter] Připojil jsem se na OnLobbyReady");



        while (!SteamManager.Initialized)
            yield return null;

        while (steamLobby.menuComp == null)
            yield return null;

        Debug.Log("[LobbyStarter] menuComp připraveno, můžeme spustit HostLobby");

        if (steamLobby.lobbyID != 0)
            steamLobby.LeaveLobby();

        
        steamLobby.startup = true;
        steamLobby.HostLobby();
    }

    private void HandleLobbyReady()
    {
        steamLobby.OnLobbyReady -= HandleLobbyReady; 
        Debug.Log("[OnGameStart] Lobby ready!");
        StartCoroutine(CloseLobbyAndFade());
    }


    private IEnumerator CloseLobbyAndFade()
    {
        Debug.Log("Došlo do CloseLobbyAndRape");
        yield return new WaitForSeconds(0.5f);
        steamLobby.CloseLobby();
        yield return StartCoroutine(blackscreen.FadeRoutine(false));
        Debug.Log("Došlo na konec CloseLobbyAndRape");
        Destroy(gameObject);
    }
}
