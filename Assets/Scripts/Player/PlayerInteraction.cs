using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private InputActionReference interact;
    [SerializeField] public Transform holdPoint;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] Animator animator;
    [SerializeField] Vector3 handPosition;
    [SerializeField] GameObject player;

    [SyncVar] private NetworkIdentity heldItemNet;

    private ItemPickup heldItem;

    public override void OnStartLocalPlayer()
    {
        interact.action.Enable();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

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
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                var item = hit.collider.GetComponent<ItemPickup>();
                if (item != null && !item.isPickedUp)
                    CmdPickupItem(item.netIdentity);
            }
        }
    }

    [Command]
    void CmdPickupItem(NetworkIdentity itemNet)
    {
        if (heldItemNet != null) return; // už nìco drží

        var item = itemNet.GetComponent<ItemPickup>();
        if (item == null || item.isPickedUp) return;

        heldItemNet = itemNet;
        heldItem = item;
        item.OnPickup(player.GetComponent<NetworkIdentity>());

        playerStats.drinkObject = item.drinkableObject;

        if (item.useHands)
            RpcSetCarryingAnim(true);
    }



    [ClientRpc]
    void RpcSetCarryingAnim(bool state)
    {
        if (animator != null)
            animator.SetBool("Carrying", state);
    }

    private void HandleHeldItem()
    {

        if (!isLocalPlayer) return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
            CmdDropItem();

        if (Input.GetMouseButtonDown(1))
            CmdUseItem();

        if (Keyboard.current.rKey.wasPressedThisFrame)
            CmdHandItem(true);

        if (Keyboard.current.rKey.wasReleasedThisFrame)
            CmdHandItem(false);
    }

    [Command]
    public void CmdDropItem()
    {
        if (heldItemNet == null) return;

        Debug.Log("Dropuju");

        var item = heldItemNet.GetComponent<ItemPickup>();
        if (item == null) return;

        Vector3 dropPos = holdPoint.position + transform.forward * 0.5f;
        item.OnDrop(dropPos);

        heldItemNet = null;
        heldItem = null;
        playerStats.drinkObject = null;
        RpcSetCarryingAnim(false);
    }

    [Command]
    void CmdUseItem()
    {
        if (heldItemNet == null) return;
        var item = heldItemNet.GetComponent<ItemPickup>();
        item.OnInteract(player);
    }

    [Command]
    void CmdHandItem(bool active)
    {
        RpcHandItem(active);
    }

    [ClientRpc]
    void RpcHandItem(bool active)
    {
        animator.SetBool("Hand", active);
        if (active)
            holdPoint.localPosition = handPosition;
        else if (heldItem != null)
            holdPoint.localPosition = heldItem.HoldPosition;
    }
}
