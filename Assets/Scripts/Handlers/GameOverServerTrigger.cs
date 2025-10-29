using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class GameOverServerTrigger : NetworkBehaviour
{
    public override void OnStartServer()
    {
        PlayerStats.OnAnyPlayerDied += CheckGameOver;
    }

    public override void OnStopServer()
    {
        PlayerStats.OnAnyPlayerDied -= CheckGameOver;
    }

    void CheckGameOver()
    {
        var allPlayers = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
        bool allDead = true;

        foreach (var ps in allPlayers)
        {
            if (ps.health > 0)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            Debug.Log("?? Všichni mrtví — posílám GameOver scénu všem!");
            RpcLoadGameOver();
        }
    }

    [ClientRpc]
    void RpcLoadGameOver()
    {
        FindAnyObjectByType<GameManager>().ClearList();
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
    }
}
