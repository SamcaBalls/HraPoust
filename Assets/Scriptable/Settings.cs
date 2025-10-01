using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerSettings",       // n�zev nov�ho souboru
    menuName = "Settings/Player Settings", // kde se to objev� v menu
    order = 0)]
public class Settings : ScriptableObject
{
    [Header("Audio")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Header("Gameplay")]
    [Range(0f, 1f)] public float sensitivity = 1f;
}

