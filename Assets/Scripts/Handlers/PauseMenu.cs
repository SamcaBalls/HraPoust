using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] PlayerStats playerStats;

    bool on = false;
    void ShowPauseMenu()
    {
        Cursor.lockState = on ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = on;
        canvas.SetActive(on);
        playerStats.MovementEnabled(!on);
    }

    public void HandlePause()
    {
        on = !on;
        ShowPauseMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            HandlePause();
        }
    }
}
