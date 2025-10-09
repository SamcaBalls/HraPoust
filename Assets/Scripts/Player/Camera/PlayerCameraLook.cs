using Mirror;
using Mirror.Examples.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerCameraLook : CameraScript
{
    [Header("References")]
    public Transform playerBody; // objekt hr��e, kolem kter�ho se kamera ot��� (v�t�inou parent)

    [Header("Settings")]
    public float clampAngle = 85f;

    private Vector2 lookInput;
    private float xRotation = 0f;

    [SerializeField] private InputActionReference look;
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (gameObject != null)
        {
            cam.gameObject.SetActive(true);
            cam.tag = "MainCamera"; // d�le�it�, aby fungovalo Camera.main
            cam.GetComponent<AudioListener>().enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Vypne kamery u jin�ch hr���
        if (!isLocalPlayer && gameObject != null)
        {
            cam.gameObject.SetActive(false);
            cam.GetComponent<AudioListener>().enabled = false;
        }
    }

    // Input System callback
    void Update()
    {
        lookInput = look.action.ReadValue<Vector2>();

        // my�/joystick input
        float mouseX = lookInput.x * settings.sensitivity;
        float mouseY = lookInput.y * settings.sensitivity;


        // vertik�ln� rotace (kamera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // horizont�ln� rotace (t�lo hr��e)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
