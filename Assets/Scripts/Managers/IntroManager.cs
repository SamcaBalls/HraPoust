using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [SerializeField] Blackscreen blackscreen;

    void Start()
    {
        StartCoroutine(FadeSequence());
    }

    IEnumerator FadeSequence()
    {
        yield return blackscreen.FadeRoutine(false);
        yield return new WaitForSeconds(1);
        yield return blackscreen.FadeRoutine(true);
        SceneManager.LoadScene("Menu");
    }

}
