using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class ReturnToLobbyManager : NetworkBehaviour
{
    [SerializeField] GameObject prefab;
    public void SendToLobby()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(GoToLobby());
    }

    private IEnumerator GoToLobby()
    {
        yield return new WaitForSeconds(0.5f);


        Debug.Log("Jdu na vypnutí");
        var steamLobby = FindAnyObjectByType<SteamLobby>();
        if (steamLobby != null && steamLobby.lobbyID != 0)
            steamLobby.CloseLobby();

        var networkM = FindAnyObjectByType<CustomNetworkManager>();
        if (networkM != null)
        {
            if (NetworkClient.isConnected)
            {
                Debug.Log("Stopping Client...");
                networkM.StopClient();
                Debug.Log("Stopped Client");
            }

            if (NetworkServer.active)
            {
                Debug.Log("Stopping Host...");
                networkM.StopHost();
                Debug.Log("Stopped Host");
            }

            // 🕓 Počkej dokud se síť úplně nevypne
            yield return new WaitUntil(() =>
                !NetworkClient.isConnected && !NetworkServer.active);
        }

        networkM.onlineScene = null;
        networkM.playerPrefab = prefab;
        // Až potom načti Menu
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }


}
