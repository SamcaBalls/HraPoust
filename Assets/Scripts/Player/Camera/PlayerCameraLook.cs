using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraLook : CameraScript
{
    [Header("References")]
    public Transform playerBody;
    public LayerMask interactLayer; // napø. "Item"
    public float interactRange = 3f;

    [Header("Settings")]
    public float clampAngle = 85f;

    private Vector2 lookInput;
    private float xRotation = 0f;

    [SerializeField] private InputActionReference look;
    [SerializeField] private InputActionReference interact; // nová akce pro "E" nebo "Use"

    private Camera cam;
    private ItemPickup currentTarget;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        cam = GetComponentInChildren<Camera>(true);

        if (cam != null)
        {
            cam.gameObject.SetActive(true);
            cam.tag = "MainCamera";
            cam.GetComponent<AudioListener>().enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Aktivace input akcí
        look.action.Enable();
        interact.action.Enable();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer && cam != null)
        {
            cam.gameObject.SetActive(false);
            cam.GetComponent<AudioListener>().enabled = false;
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        HandleLook();
        HandleInteractionRaycast();
    }

    private void HandleLook()
    {
        lookInput = look.action.ReadValue<Vector2>();

        float mouseX = lookInput.x * settings.sensitivity;
        float mouseY = lookInput.y * settings.sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleInteractionRaycast()
    {
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            ItemPickup pickup = hit.collider.gameObject.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                // Míøíme na interaktivní objekt
                currentTarget = pickup;

                // Po stisku E
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("Bum");
                    pickup.OnPickup(gameObject);
                }
            }
        }
        else
        {
            currentTarget = null;
        }
    }
}
