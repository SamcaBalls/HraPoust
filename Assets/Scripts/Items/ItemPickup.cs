using UnityEngine;

[DisallowMultipleComponent]
public class ItemPickup : MonoBehaviour
{
    public string itemName = "Item";
    public bool isPickedUp = false;
    public Vector3 HoldPosition;
    public Quaternion HoldRotation;
    public bool useHands;

    // Volá se z player skriptu pøi interakci
    public virtual void OnPickup(GameObject player)
    {
        Debug.Log($"Player picked up {itemName}");
        Destroy(gameObject);
    }
}
