using Mirror;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BurnoutCamera : NetworkBehaviour
{
    [Header("Follow Cam Settings")]
    public float distance = 5f;
    public float height = 2f;
    public float rotationSpeed = 5f;

    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;

    private float orbitY = 0f; // horizont�ln� �hel
    private float orbitX = 20f; // vertik�ln� �hel

    public void ActivateBurnoutMode()
    {
        if (!isLocalPlayer) return;

        cam.gameObject.SetActive(true);
        FollowCurrentPlayer();
    }

    private void Update()
    {
        HandleMouseInput();
        FollowCurrentPlayer();
    }

    private void FollowCurrentPlayer()
    {

        Transform target = player;

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

    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        orbitY += mouseX;
        orbitX -= mouseY;
        orbitX = Mathf.Clamp(orbitX, 10f, 80f); // omezen� vertik�ln�ho �hlu
    }
}
