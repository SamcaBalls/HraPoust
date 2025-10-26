using SteamLobbyTutorial;
using System.Collections;
using UnityEngine;

public class TweakingBehaviors : MonoBehaviour
{
    [SerializeField] private PlayerMovementHandler movement;

    public void ShakyVision()
    {
        Debug.Log("Shaky vision effect started");
    }

    public void PartialBlindness()
    {
        Debug.Log("Partial blindness effect started");
    }

    public void InvertedInputs()
    {
        Debug.Log("Inverted inputs effect started");
    }

    public void Hallucination()
    {
        Debug.Log("Hallucination effect started");
    }

    public void FakeStructure()
    {
        Debug.Log("Fake structure effect started");
    }

    public void StrangeSounds()
    {
        Debug.Log("Strange sounds effect started");
    }

    public void Deafness()
    {
        Debug.Log("Deafness effect started");
    }

    IEnumerator PlayStrangeSounds()
    {
        yield return null;
    }

    public void ResetAll()
    {
        movement.moveSpeed = movement.normalSpeed; 
        // reset dalších efektù
    }
}
