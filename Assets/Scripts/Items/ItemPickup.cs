using UnityEngine;

[DisallowMultipleComponent]
public class ItemPickup : MonoBehaviour
{
    public string itemName = "Item";

    // Volá se z player skriptu pøi interakci
    public virtual void OnPickup(GameObject player)
    {
        Debug.Log($"Player picked up {itemName}");
        Destroy(gameObject);
    }
}
