using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] DrinkableObject drink;
    [SerializeField] GameObject water;
    [SerializeField] float minHeight; // v��ka, kdy� je pr�zdn�
    [SerializeField] float maxHeight; // v��ka, kdy� je pln�

    public void ChangeWaterLevel()
    {
        if (drink == null || water == null) return;

        // Pom�r napln�n� 0�1
        float fillRatio = Mathf.Clamp01((float)drink.Capacity / drink.maxCapacity);

        // Vypo��tan� Y pozice
        float yPosition = Mathf.Lerp(minHeight, maxHeight, fillRatio);

        // Nastaven� pozice
        Vector3 pos = water.transform.position;
        pos.y = drink.transform.position.y + yPosition;
        water.transform.localPosition = new Vector3(0, yPosition, 0);

        // Viditelnost
        water.SetActive(fillRatio > 0);
    }
}

