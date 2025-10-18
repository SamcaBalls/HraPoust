using UnityEngine;

public class EnablerGameOver : MonoBehaviour
{
    [SerializeField] GameObject gameOverManager;
    void Start()
    {
        gameOverManager.SetActive(true);
    }
}
