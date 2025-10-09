using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

    public class LobbyPreviewUI : MonoBehaviour
    {
    public TMP_Text lobbyNameText;
    private CSteamID lobbyID;

    [SerializeField]
    private GameObject privateImage;


    public void SetLobbyInfo(CSteamID id, string lobbyName, string hostAddress)
    {
        lobbyID = id;
        if (lobbyNameText != null)
            lobbyNameText.text = lobbyName;
        if(SteamMatchmaking.GetLobbyData(lobbyID, "private") == "false")
        {
            privateImage.SetActive(false);
        }
    }

    public void OnJoinClicked()
    {
        FindAnyObjectByType<SteamLobby>().JoinLobby(lobbyID);
    }
}
