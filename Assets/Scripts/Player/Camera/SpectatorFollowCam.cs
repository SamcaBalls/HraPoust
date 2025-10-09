using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class SpectatorFollowCam : CameraScript
{
    [Header("Follow Cam Settings")]
    public float distance = 5f;
    public float height = 2f;

    private List<PlayerStats> livePlayers = new List<PlayerStats>();
    private int currentIndex = 0;

    private float orbitY = 0f; // horizont�ln� �hel
    private float orbitX = 20f; // vertik�ln� �hel


    public override void ActivateCamera()
    {
        base.ActivateCamera();
        UpdateLivePlayers();
        HandleMouseInput();
    }

    private void Update()
    {
        if (!cam.enabled || livePlayers.Count == 0) return;

        HandleMouseInput();
        FollowCurrentPlayer();

        // p�ep�n�n� hr���
        if (Input.GetKeyDown(KeyCode.Mouse0)) NextPlayer();
        if (Input.GetKeyDown(KeyCode.Mouse1)) PreviousPlayer();
    }

    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * settings.sensitivity * 3;
        float mouseY = Input.GetAxis("Mouse Y") * settings.sensitivity * 3;

        orbitY += mouseX;
        orbitX -= mouseY;
        orbitX = Mathf.Clamp(orbitX, 10f, 80f); // omezen� vertik�ln�ho �hlu
    }

    private void UpdateLivePlayers()
    {
        livePlayers.Clear();
        foreach (var ps in FindObjectsByType<PlayerStats>(FindObjectsSortMode.InstanceID))
            if (ps.health > 0) livePlayers.Add(ps);

        if (currentIndex >= livePlayers.Count) currentIndex = 0;
    }

    private void FollowCurrentPlayer()
    {
        if (livePlayers.Count == 0) return;

        Transform target = livePlayers[currentIndex].transform;

        // vypo��t�me orbit�ln� pozici kamery
        Quaternion rotation = Quaternion.Euler(orbitX, orbitY, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 targetPos = target.position + Vector3.up * height + offset;

        // kolize s ter�nem / objekty
        RaycastHit hit;
        if (Physics.Linecast(target.position + Vector3.up * 1.5f, targetPos, out hit))
        {
            targetPos = hit.point - (hit.point - (target.position + Vector3.up * 1.5f)).normalized * 0.2f;
        }

        cam.transform.position = targetPos;
        cam.transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    private void NextPlayer()
    {
        if (livePlayers.Count == 0) return;
        currentIndex = (currentIndex + 1) % livePlayers.Count;
    }

    private void PreviousPlayer()
    {
        if (livePlayers.Count == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = livePlayers.Count - 1;
    }
}
