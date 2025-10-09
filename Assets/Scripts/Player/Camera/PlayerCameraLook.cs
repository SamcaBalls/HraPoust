using Mirror;
using Mirror.Examples.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerCameraLook : CameraScript
{
    [Header("References")]
    public Transform playerBody; // objekt hráèe, kolem kterého se kamera otáèí (vìtšinou parent)

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
            cam.tag = "MainCamera"; // dùležité, aby fungovalo Camera.main
            cam.GetComponent<AudioListener>().enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Vypne kamery u jiných hráèù
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

        // myš/joystick input
        float mouseX = lookInput.x * settings.sensitivity;
        float mouseY = lookInput.y * settings.sensitivity;


        // vertikální rotace (kamera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // horizontální rotace (tìlo hráèe)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
