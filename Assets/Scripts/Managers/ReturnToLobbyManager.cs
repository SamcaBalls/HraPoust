using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class ReturnToLobbyManager : NetworkBehaviour
{

    public void SendToLobby()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(GoToLobby());
    }

    private IEnumerator GoToLobby()
    {


        // Počkej chvíli, aby se dokončily případné network operace
        yield return new WaitForSeconds(0.5f);

        // Zavři aktuální lobby (nebo nic neudělá, pokud žádné není)
        var steamLobby = FindAnyObjectByType<SteamLobby>();
        if (steamLobby != null && steamLobby.lobbyID != 0)
        {
            steamLobby.CloseLobby();
        }

        var netwokM = FindAnyObjectByType<CustomNetworkManager>();
        if (netwokM != null)
        {
            if (isClient)
            {
                netwokM.StopClient();
            }
            if (isServer)
            {
                netwokM.StopHost();
            }            
        }


        // Načti Menu scénu
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

}
