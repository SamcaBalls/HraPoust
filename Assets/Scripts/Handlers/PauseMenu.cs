using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] PlayerStats playerStats;
    public void ShowPauseMenu(bool show)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
        canvas.SetActive(show);
        playerStats.MovementEnabled(!show);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.P))
            ShowPauseMenu(true);
    }
}
