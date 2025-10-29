using Mirror;
using UnityEngine;

public class PlayerUIManager : NetworkBehaviour
{
    [SerializeField] private Canvas playerCanvas;

    public override void OnStartLocalPlayer()
    {
        playerCanvas.enabled = true;
    }

    public override void OnStartClient()
    {
        if (!isLocalPlayer)
        {
            playerCanvas.enabled = false;

            // volitelné: úplnì vypnout raycaster
            var ray = playerCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (ray != null) ray.enabled = false;
        }
    }
}
