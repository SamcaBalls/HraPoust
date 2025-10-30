using UnityEngine;

[DisallowMultipleComponent]
public class ItemPickup : MonoBehaviour
{
    public string itemName = "Item";

    // Vol� se z player skriptu p�i interakci
    public virtual void OnPickup(GameObject player)
    {
        Debug.Log($"Player picked up {itemName}");
        Destroy(gameObject);
    }
}
