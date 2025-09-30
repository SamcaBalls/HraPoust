using UnityEngine;
using Mirror;

public class HideLocalPlayerModel : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerParts;

    public override void OnStartLocalPlayer()
    {
        // skryj model jen pro sebe
        if (playerParts.Length != 0)
        {
            foreach (var part in playerParts)
            {
                part.SetActive(false);
            }
        }
    }
}
