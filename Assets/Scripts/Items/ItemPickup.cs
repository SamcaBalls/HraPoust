using System.Collections;
using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class ItemPickup : NetworkBehaviour
{
    [SyncVar] public string itemName = "Item";
    [SyncVar(hook = nameof(OnPickupStateChanged))] public bool isPickedUp = false;

    public Vector3 HoldPosition;
    public Quaternion HoldRotation;
    public bool useHands;

    [SerializeField] float deathTime = 60f;
    [SerializeField] public DrinkableObject drinkableObject;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // 🟢 Server zvednutí — přijímá NetworkIdentity místo GameObjectu
    [Server]
    public void OnPickup(NetworkIdentity playerIdentity)
    {
        if (playerIdentity == null) return;
        if (isPickedUp) return;

        isPickedUp = true;
        StopAllCoroutines();

        // Poslat klientům netId, ne GameObject
        RpcAttachToPlayer(playerIdentity.netId);
    }

    // 🟢 Server položení
    [Server]
    public void OnDrop(Vector3 pos)
    {
        if (!isPickedUp) return;

        isPickedUp = false;
        RpcDetachFromPlayer(pos);
        StartCoroutine(DeathTimer());
    }

    void OnPickupStateChanged(bool oldState, bool newState)
    {
        if (rb != null) rb.isKinematic = newState;
    }

    [ClientRpc]
    void RpcAttachToPlayer(uint playerNetId)
    {
        if (!NetworkClient.spawned.TryGetValue(playerNetId, out var obj))
            return;

        var player = obj.GetComponent<PlayerInteraction>();
        if (player == null) return;

        transform.SetParent(player.holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        player.holdPoint.localPosition = HoldPosition;
        player.holdPoint.localRotation = HoldRotation;
        if (rb) rb.isKinematic = true;

        Debug.Log("Přiděláno k HoldPoint"); 
    }

    [ClientRpc]
    void RpcDetachFromPlayer(Vector3 pos)
    {
        transform.SetParent(null);
        transform.position = pos;
        if (rb) rb.isKinematic = false;
    }

    public virtual void OnInteract(GameObject player)
    {
        Debug.Log(name + ": Interacting...");
    }

    [Server]
    public IEnumerator DeathTimer()
    {
        float elapsed = 0f;
        while (elapsed < deathTime)
        {
            if (isPickedUp)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        NetworkServer.Destroy(gameObject);
    }
}
