using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine.SceneManagement;

    public class SteamLobby : NetworkBehaviour
    {
        private static SteamLobby instance;
        public static SteamLobby Instance {
            get { if (instance == null)
                    instance = FindAnyObjectByType<SteamLobby>(FindObjectsInactive.Include); 
                return instance;
            } 
        }
        public ulong lobbyID;
        public NetworkManager networkManager;
        public MenuComponents menuComp;    
        bool privateLobby = false;

        private Callback<LobbyCreated_t> lobbyCreated;
        private Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        private Callback<LobbyEnter_t> lobbyEntered;
        private Callback<LobbyChatUpdate_t> lobbyChatUpdate;

        private const string HostAddressKey = "HostAddress";
        private bool callbacksRegistered = false;
        private bool waitingForSteam = false;

        public event System.Action OnLobbyReady;

        public bool startup = true;

        bool sceneLoadedSubscribed = false;




    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (!callbacksRegistered)
            RegisterCallbacks();

        if (!sceneLoadedSubscribed)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            sceneLoadedSubscribed = true;
        }
    }

    void OnDestroy()
    {
        Debug.Log("[SteamLobby] Destroying...");

        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (SteamManager.Initialized)
        {
            try
            {
                SteamMatchmaking.LeaveLobby((CSteamID)lobbyID);
                Debug.Log("[SteamLobby] Left lobby " + lobbyID);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[SteamLobby] LeaveLobby failed: " + e.Message);
            }
        }

        Debug.Log("[SteamLobby] Destroy done.");
    }



    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Menu")) return;

        SceneManager.MoveGameObjectToScene(gameObject, scene);

        Debug.Log($"Nová scéna načtena: {scene.name}");

        // Tohle se ti spustí POKAŽDÉ po načtení scény
        menuComp = FindAnyObjectByType<MenuComponents>();


        SetOnclicks();

    }

    void SetOnclicks()
    {
        if (menuComp != null)
        {
            menuComp.hostButton.onClick.RemoveAllListeners();
            menuComp.returnLobbyButton.onClick.RemoveAllListeners();
            menuComp.dropdown.onValueChanged.RemoveAllListeners();

            menuComp.hostButton.onClick.AddListener(HostLobby);
            menuComp.returnLobbyButton.onClick.AddListener(LeaveLobby);
            menuComp.dropdown.onValueChanged.AddListener(OnDropdownChange);

            var browser = FindAnyObjectByType<LobbyBrowser>();
            if (browser != null)
                browser.SetOnclicks();

            Debug.Log("Added listeners");
        }
    }


    void Start()
    {
        DontDestroyOnLoad(gameObject);
        networkManager = FindAnyObjectByType<NetworkManager>();

        // Manuální inicializace, protože sceneLoaded se nevolá při prvním načtení
        InitializeMenu();

        SteamAPI.RunCallbacks();
    }

    private void InitializeMenu()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            menuComp = FindAnyObjectByType<MenuComponents>();
            SetOnclicks();
        }
    }


    void RegisterCallbacks()
    {
        if (callbacksRegistered)
            return; // už registrováno

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);

        callbacksRegistered = true;
        Debug.Log("Steam callbacky registrované");
    }


    public void HostLobby()
        {
        if(menuComp != null)
        {
            if (string.IsNullOrWhiteSpace(menuComp.inputFieldHost.text) && privateLobby)
            {
                menuComp.warningText.SetActive(true);
                return;
            }
        }



        if (!SteamManager.Initialized)
            {
                if (!waitingForSteam)
                {
                    StartCoroutine(WaitForSteamAndHost());
                    waitingForSteam = true;
                }
                return;
            }

        if (menuComp != null) menuComp.panelSwapper.SwapPanel("LobbyPanel");


            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 8);

            Debug.Log(privateLobby
                ? "Požadavek na vytvoření PUBLIC lobby s heslem odeslán"
                : "Požadavek na vytvoření PUBLIC lobby bez hesla odeslán");
        }

        public IEnumerator WaitForSteamAndHost()
        {
            while (!SteamManager.Initialized)
                yield return null;

            Debug.Log("Steam inicializován, registruji callbacky a vytvářím lobby");

            HostLobby();
        }

        void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError("Nepodařilo se vytvořit lobby: " + callback.m_eResult);
                return;
            }

            if (startup)
            {
                OnLobbyReady?.Invoke(); // tady se spustí event
                startup = false;
            }

        lobbyID = callback.m_ulSteamIDLobby;
            var lobby = new CSteamID(lobbyID);

            Debug.Log("Lobby vytvořeno. ID: " + lobbyID);

            SteamMatchmaking.SetLobbyData(lobby, HostAddressKey, SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(lobby, "name", SteamFriends.GetPersonaName() + "'s Lobby");
            SteamMatchmaking.SetLobbyData(lobby, "game_id", "xXBallerXx");

            // uložíme jestli má lobby heslo
            SteamMatchmaking.SetLobbyData(lobby, "private", privateLobby ? "true" : "false");
            SteamMatchmaking.SetLobbyData(lobby, "password", privateLobby ? menuComp.inputFieldHost.text : "");

            networkManager.StartHost();
        if (menuComp != null)
        {
            // reset flagu pro jistotu
            privateLobby = false;
            menuComp.dropdown.value = 0;
            menuComp.inputFieldHost.text = "";
            menuComp.warningText.SetActive(false);
        }
    }


        void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            Debug.Log("Join request received for lobby: " + callback.m_steamIDLobby);

            if (NetworkServer.active)
                NetworkManager.singleton.StopHost();
            if (NetworkClient.isConnected || NetworkClient.active)
            {
                NetworkManager.singleton.StopClient();
                NetworkClient.Shutdown();
            }

            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        void OnLobbyEntered(LobbyEnter_t callback)
        {
            if (NetworkServer.active)
            {
                Debug.Log("Jsem host, ignoruji join request");
                return;
            }

            lobbyID = callback.m_ulSteamIDLobby;
            var lobby = new CSteamID(lobbyID);

            string hostAddress = SteamMatchmaking.GetLobbyData(lobby, HostAddressKey);
            if (string.IsNullOrEmpty(hostAddress))
            {
                Debug.LogError("Nebyla nalezena HostAddress!");
                return;
            }

            networkManager.networkAddress = hostAddress;
            Debug.Log("Vstoupil jsem do lobby: " + lobbyID);

            networkManager.StartClient();
        menuComp.panelSwapper.SwapPanel("LobbyPanel");
        }

        void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            if (callback.m_ulSteamIDLobby != lobbyID) return;

            EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;
            Debug.Log($"LobbyChatUpdate: {stateChange}");

            bool shouldUpdate = stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeLeft) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeDisconnected) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeKicked) ||
                                stateChange.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeBanned);

            if (shouldUpdate)
            {
                StartCoroutine(DelayedNameUpdate(0.5f));
                LobbyUIManager.Instance?.CheckAllPlayersReady();
            }
        }

        private IEnumerator DelayedNameUpdate(float delay)
        {
            if (LobbyUIManager.Instance == null)
            {
                Debug.LogWarning("Lobby UI Manager.Instance je null, přeskočeno");
                yield break;
            }
            yield return new WaitForSeconds(delay);
            LobbyUIManager.Instance?.UpdatePlayerLobbyUI();
        }

        public void LeaveLobby()
        {
            CSteamID currentOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyID));
            CSteamID me = SteamUser.GetSteamID();
            var lobby = new CSteamID(lobbyID);

            if (lobbyID != 0)
            {
                SteamMatchmaking.LeaveLobby(lobby);
                lobbyID = 0;
            }

        if (NetworkClient.isConnected)
        {
            networkManager.StopClient();
            Debug.Log("Stopped Client");
        }

        if (NetworkServer.active)
        {
            networkManager.StopHost();
            Debug.Log("Stopped Host");
        }

        if (NetworkServer.active && currentOwner == me)
                NetworkManager.singleton.StopHost();
            else if (NetworkClient.isConnected)
                NetworkManager.singleton.StopClient();

            menuComp.panelSwapper.gameObject.SetActive(true);
            this.gameObject.SetActive(true);
            menuComp.panelSwapper.SwapPanel("MainPanel");
        }

    public void CloseLobby()
    {
        if (lobbyID != 0)
        {
            var lobby = new CSteamID(lobbyID);
            CSteamID owner = SteamMatchmaking.GetLobbyOwner(lobby);

            // Pokud jsem host, uzavři lobby pro join
            if (owner == SteamUser.GetSteamID())
            {
                SteamMatchmaking.SetLobbyJoinable(lobby, false);
                SteamMatchmaking.SetLobbyData(lobby, "closed", "true");
            }

            // Stop network
            if (NetworkServer.active && owner == SteamUser.GetSteamID())
                networkManager.StopHost();
            else if (NetworkClient.isConnected)
                networkManager.StopClient();

            // Odejdeme z lobby
            SteamMatchmaking.LeaveLobby(lobby);
            lobbyID = 0;



            // Vyčistit UI hráčů
            LobbyUIManager.Instance?.ClearLobbyPlayers();

            // Hard reset callbacků
            HardReset();
            Debug.Log("Zajebal jsem to");
            if (menuComp != null)
            {
                menuComp.panelSwapper.gameObject.SetActive(true);
                gameObject.SetActive(true);
                menuComp.panelSwapper.SwapPanel("MainPanel");
            }
        }
    }



    public void OnDropdownChange(int value)
        {
            privateLobby = value == 1;
            Debug.Log("PrivateLobby: " + privateLobby);
            menuComp.inputFieldHost.interactable = privateLobby;
        }

        public void JoinLobby(CSteamID targetLobbyID)
        {
            string password = SteamMatchmaking.GetLobbyData(targetLobbyID, "password");

            if (!string.IsNullOrEmpty(password))
            {
                if(menuComp.inputFieldClient.text != password)
                {
                    Debug.Log("Lobby má heslo → otevírám PasswordPanel");
                menuComp.passwordUI.SetLobbyInfo(targetLobbyID);
                menuComp.panelSwapper.SwapPanel("PasswordEnterPanel");
                }
                else
                {
                    SteamMatchmaking.JoinLobby(targetLobbyID);
                }

            }
            else
            {
                Debug.Log("Lobby nemá heslo → rovnou join");
                SteamMatchmaking.JoinLobby(targetLobbyID);
            }
        }

    public void HardReset()
    {
        // Odregistrovat všechny callbacky
        if (lobbyCreated != null) { lobbyCreated.Dispose(); lobbyCreated = null; }
        if (gameLobbyJoinRequested != null) { gameLobbyJoinRequested.Dispose(); gameLobbyJoinRequested = null; }
        if (lobbyEntered != null) { lobbyEntered.Dispose(); lobbyEntered = null; }
        if (lobbyChatUpdate != null) { lobbyChatUpdate.Dispose(); lobbyChatUpdate = null; }

        callbacksRegistered = false;
        lobbyID = 0;
        startup = true;
        waitingForSteam = false;

        // Znovu registrovat callbacky pro nové lobby
        RegisterCallbacks();

        Debug.Log("[SteamLobby] Hard reset hotov");
    }


}
