using UnityEngine;

public class BowlPickup : ItemPickup
{
    public override void OnInteract(GameObject player)
    {
        base.OnInteract(player);
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats.drinkObject.Capacity <= 0) return;

        StartCoroutine(playerStats.ChangeFatigueSmooth(-playerStats.drinkObject.Capacity * 2, 3));

        playerStats.drinkObject.Capacity = 0;
    }
}
