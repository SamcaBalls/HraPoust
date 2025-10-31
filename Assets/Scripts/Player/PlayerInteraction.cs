using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private InputActionReference interact;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private PlayerStats playerStats;

    private ItemPickup currentTarget;
    private ItemPickup heldItem;



    private void Start()
    {
        interact.action.Enable();
    }

    private void Update()
    {
        if (heldItem == null)
            HandleRaycast();
        else
            HandleHeldItem();
    }

    private void HandleRaycast()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            ItemPickup pickup = hit.collider.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                currentTarget = pickup;

                if (Input.GetKeyDown(KeyCode.E))
                    PickupItem(pickup);
            }
        }
        else
        {
            currentTarget = null;
        }
    }

    private void PickupItem(ItemPickup item)
    {
        if (!item.isPickedUp)
        {
         
            heldItem = item;
            holdPoint.transform.localPosition = heldItem.HoldPosition;
            holdPoint.transform.localRotation = heldItem.HoldRotation;
            heldItem.OnPickup(gameObject);
            playerStats.hasWater = true;

            Rigidbody rb = heldItem.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            heldItem.transform.SetParent(holdPoint);
            heldItem.transform.localPosition = Vector3.zero;
            heldItem.transform.localRotation = Quaternion.identity;
            if (heldItem.useHands)
            {
                //Logika pro ruce
            }
        }
        else
        {
            Debug.Log("Objekt u� je sebr�n");
            return;
        }


    }

    private void HandleHeldItem()
    {
        // Udr�uj item p�ed hr��em
        heldItem.transform.position = Vector3.Lerp(
            heldItem.transform.position,
            holdPoint.position,
            Time.deltaTime * 10f
        );
        heldItem.transform.rotation = Quaternion.Lerp(
            heldItem.transform.rotation,
            holdPoint.rotation,
            Time.deltaTime * 10f
        );

        // Drop
        if (Keyboard.current.qKey.wasPressedThisFrame)
            DropItem();
    }

    private void DropItem()
    {
        heldItem.transform.SetParent(null);
        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
        heldItem.isPickedUp = false;
        heldItem = null;
        playerStats.hasWater = false;
        
    }
}
