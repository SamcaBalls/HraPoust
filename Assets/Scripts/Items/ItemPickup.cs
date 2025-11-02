using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemPickup : MonoBehaviour
{
    public string itemName = "Item";
    public bool isPickedUp = false;
    public Vector3 HoldPosition;
    public Quaternion HoldRotation;
    public bool useHands;
    [SerializeField] float deathTime = 60;

    // Volá se z player skriptu pøi interakci
    public virtual void OnPickup(GameObject player)
    {
        isPickedUp = true;
        Debug.Log($"Player picked up {itemName}");
    }

    public IEnumerator DeathTimer()
    {
        float elapsed = 0f;
        while (elapsed < deathTime)
        {
            // pokud ho nìkdo zvedne, timer se zruší
            if (isPickedUp)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"{itemName} destroyed after being left on ground");
        Destroy(gameObject);
    }

}
