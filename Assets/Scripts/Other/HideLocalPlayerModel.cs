using UnityEngine;
using Mirror;

public class HideLocalPlayerModel : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerParts;

    public override void OnStartLocalPlayer()
    {
        GiveHead(false);
    }

    public void GiveHead(bool head)
    {
        if (playerParts.Length != 0)
        {
            foreach (var part in playerParts)
            {
                part.SetActive(head);
            }
        }
    }
}
