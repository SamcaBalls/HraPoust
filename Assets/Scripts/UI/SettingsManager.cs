using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Slidery")]
    [SerializeField] private Slider sliderMaster; // NOVÉ
    [SerializeField] private Slider sliderSFX;
    [SerializeField] private Slider sliderMusic;
    [SerializeField] private Slider sliderVoicechat;
    [SerializeField] private Slider sliderFOV;
    [SerializeField] private Slider sliderSensitivity;

    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown micDropdown;

    [Header("Reference")]
    [SerializeField] private Settings playerSettings;

    [SerializeField] Camera cam;

    private void Start()
    {
        PopulateMicDropdown();
        LoadSettingsToUI();
        AddListeners();
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    private void AddListeners()
    {
        sliderMaster.onValueChanged.AddListener(OnMasterChanged); // NOVÉ
        sliderSFX.onValueChanged.AddListener(OnSFXChanged);
        sliderMusic.onValueChanged.AddListener(OnMusicChanged);
        sliderVoicechat.onValueChanged.AddListener(OnVoiceChatChanged);
        sliderFOV.onValueChanged.AddListener(OnFOVChanged);
        sliderSensitivity.onValueChanged.AddListener(OnSensitivityChanged);
        micDropdown.onValueChanged.AddListener(OnMicChanged);
    }

    private void RemoveListeners()
    {
        sliderMaster.onValueChanged.RemoveListener(OnMasterChanged);
        sliderSFX.onValueChanged.RemoveListener(OnSFXChanged);
        sliderMusic.onValueChanged.RemoveListener(OnMusicChanged);
        sliderVoicechat.onValueChanged.RemoveListener(OnVoiceChatChanged);
        sliderFOV.onValueChanged.RemoveListener(OnFOVChanged);
        sliderSensitivity.onValueChanged.RemoveListener(OnSensitivityChanged);
        micDropdown.onValueChanged.RemoveListener(OnMicChanged);
    }

    private void PopulateMicDropdown()
    {
        micDropdown.ClearOptions();
        List<string> options = new List<string>(Microphone.devices);
        micDropdown.AddOptions(options);

        if (playerSettings != null && playerSettings.micIndex >= 0 && playerSettings.micIndex < options.Count)
        {
            micDropdown.value = playerSettings.micIndex;
            micDropdown.RefreshShownValue();
        }
    }

    private void LoadSettingsToUI()
    {
        if (playerSettings == null) return;

        sliderMaster.value = playerSettings.masterVolume; // NOVÉ
        sliderSFX.value = playerSettings.sfxVolume;
        sliderMusic.value = playerSettings.musicVolume;
        sliderVoicechat.value = playerSettings.voiceChatVolume;
        sliderFOV.value = playerSettings.FOV;
        sliderSensitivity.value = playerSettings.sensitivity;
    }

    #region Slider Callbacks
    private void OnMasterChanged(float value) => playerSettings.masterVolume = value; // NOVÉ
    private void OnSFXChanged(float value) => playerSettings.sfxVolume = value;
    private void OnMusicChanged(float value) => playerSettings.musicVolume = value;
    private void OnVoiceChatChanged(float value) => playerSettings.voiceChatVolume = value;
    private void OnFOVChanged(float value)
    {
        playerSettings.FOV = value;

        cam.fieldOfView = value;
    }
    private void OnSensitivityChanged(float value) => playerSettings.sensitivity = value;
    #endregion

    private void OnMicChanged(int index)
    {
        if (index < 0 || index >= Microphone.devices.Length) return;

        playerSettings.micIndex = index;
        playerSettings.micName = Microphone.devices[index];
        Debug.Log("[SettingsManager] Selected Mic: " + playerSettings.micName);
    }

    #region Save / Load PlayerPrefs
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", playerSettings.masterVolume); // NOVÉ
        PlayerPrefs.SetFloat("SFXVolume", playerSettings.sfxVolume);
        PlayerPrefs.SetFloat("MusicVolume", playerSettings.musicVolume);
        PlayerPrefs.SetFloat("VoiceChatVolume", playerSettings.voiceChatVolume);
        PlayerPrefs.SetFloat("FOV", playerSettings.FOV);
        PlayerPrefs.SetFloat("Sensitivity", playerSettings.sensitivity);
        PlayerPrefs.SetInt("MicIndex", playerSettings.micIndex);
        PlayerPrefs.Save();
        Debug.Log("[SettingsManager] Settings saved.");
    }

    public void LoadFromPrefs()
    {
        playerSettings.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f); // NOVÉ
        playerSettings.sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        playerSettings.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        playerSettings.voiceChatVolume = PlayerPrefs.GetFloat("VoiceChatVolume", 1f);
        playerSettings.FOV = PlayerPrefs.GetFloat("FOV", 90f);
        playerSettings.sensitivity = PlayerPrefs.GetFloat("Sensitivity", 0.5f);
        playerSettings.micIndex = PlayerPrefs.GetInt("MicIndex", 0);

        PopulateMicDropdown();
        LoadSettingsToUI();
        Debug.Log("[SettingsManager] Settings loaded from PlayerPrefs.");
    }
    #endregion
}
