using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerSettings",
    menuName = "Settings/Player Settings",
    order = 0)]
public class Settings : ScriptableObject
{
    [Header("Audio")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float voiceChatVolume = 1f;

    [Header("Gameplay")]
    [Range(60f, 120f)] public float FOV = 90f; // reálná hodnota FOV
    [Range(0f, 1f)] public float sensitivity = 0.5f;

    [Header("Audio Input")]
    public int micIndex = 0;
    public string micName = "";
}
