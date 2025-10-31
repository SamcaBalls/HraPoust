using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField] DrinkableObject drink;
    [SerializeField] GameObject water;
    [SerializeField] float minHeight; // výška, když je prázdné
    [SerializeField] float maxHeight; // výška, když je plné

    public void ChangeWaterLevel()
    {
        if (drink == null || water == null) return;

        // Pomìr naplnìní 0–1
        float fillRatio = Mathf.Clamp01((float)drink.Capacity / drink.maxCapacity);

        // Vypoèítaná Y pozice
        float yPosition = Mathf.Lerp(minHeight, maxHeight, fillRatio);

        // Nastavení pozice
        Vector3 pos = water.transform.position;
        pos.y = drink.transform.position.y + yPosition;
        water.transform.localPosition = new Vector3(0, yPosition, 0);

        // Viditelnost
        water.SetActive(fillRatio > 0);
    }
}

