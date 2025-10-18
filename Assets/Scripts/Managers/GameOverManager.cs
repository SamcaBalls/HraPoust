using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] Blackscreen blackscreen;
    [SerializeField] ReturnToLobbyManager returnToLobbyManager;

    void Start()
    {
        StartCoroutine(FadeSequence());
    }

    IEnumerator FadeSequence()
    {
        yield return blackscreen.FadeRoutine(false);
        yield return new WaitForSeconds(5);
        yield return blackscreen.FadeRoutine(true);
        returnToLobbyManager.SendToLobby();
    }

}
