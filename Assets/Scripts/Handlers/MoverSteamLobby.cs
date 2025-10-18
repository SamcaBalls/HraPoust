using UnityEngine;
using UnityEngine.SceneManagement;

public class MoverSteamLobby : MonoBehaviour
{
    GameObject steamLobby;

    public void Move()
    {
        steamLobby = FindAnyObjectByType<SteamLobby>().gameObject;
        if(steamLobby != null)
        {
        SceneManager.MoveGameObjectToScene(steamLobby, SceneManager.GetSceneByName("Menu"));

        }
    }
}
