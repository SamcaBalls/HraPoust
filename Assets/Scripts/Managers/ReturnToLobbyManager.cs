using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToLobbyManager : MonoBehaviour
{

    public void SendToLobby()
    {
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


        // Načti Menu scénu
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}
