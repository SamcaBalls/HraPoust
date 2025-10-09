using System.Collections.Generic;
using UnityEngine;

public class CameraSwapper : MonoBehaviour 
{
    public List<CameraScript> cameras = new List<CameraScript>();

    public void SwapCamera(int camera)
    {
        foreach (CameraScript script in cameras) 
        {
            script.GetComponent<AudioListener>().enabled = false;
            script.cam.enabled = false;
            script.gameObject.SetActive(false);
        }
        cameras[camera].ActivateCamera();
    }
}
