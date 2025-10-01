using Mirror;
using UnityEngine;

public abstract class CameraScript : NetworkBehaviour
{
    [SerializeField]
    public Camera cam;

    [SerializeField]
    protected Settings settings;

    public virtual void ActivateCamera()
    {
        if (!isLocalPlayer) return;
        cam.gameObject.SetActive(true);
        cam.GetComponent<AudioListener>().enabled = false;
        cam.enabled = true;
    }
}
