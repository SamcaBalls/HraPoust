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
        [SerializeField] MenuComponents menuComp;    
        bool privateLobby = false;

        private Callback<LobbyCreated_t> lobbyCreated;
        private Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
        private Callback<LobbyEnter_t> lobbyEntered;
        private Callback<LobbyChatUpdate_t> lobbyChatUpdate;

        private const string HostAddressKey = "HostAddress";
        private bool callbacksRegistered = false;
        private bool waitingForSteam = false;

        public event System.Action OnLobbyReady;

        bool startup = true;




    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Připoj callback pro každé načtení scény
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Odpoj event, aby nezůstal viset
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("Menu")) return;

        SceneManager.MoveGameObjectToScene(gameObject, scene);

        Debug.Log($"Nová scéna načtena: {scene.name}");

        // Tohle se ti spustí POKAŽDÉ po načtení scény
        menuComp = FindAnyObjectByType<MenuComponents>();
        LobbyStarter lobbyStarter = FindAnyObjectByType<LobbyStarter>();

        if (lobbyStarter != null)
            lobbyStarter.CoroutineStart();

        SetOnclicks();

    }

    void SetOnclicks()
    {
        if(menuComp != null)
        {
            menuComp.hostButton.onClick.AddListener(HostLobby);
            Debug.Log("Added listeners");
        }
    }

    void Start()
        {
            DontDestroyOnLoad(gameObject);
            networkManager = FindAnyObjectByType<NetworkManager>();
            
            SteamAPI.RunCallbacks();
        }


        void RegisterCallbacks()
        {
            if (callbacksRegistered)
                return;

            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);

            callbacksRegistered = true;
            Debug.Log("Steam callbacky registrované");
        }

        public void HostLobby()
        {
        if (string.IsNullOrWhiteSpace(menuComp.inputFieldHost.text) && privateLobby)
        {
            menuComp.warningText.SetActive(true);
            return;
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

        menuComp.panelSwapper.SwapPanel("LobbyPanel");

            RegisterCallbacks();

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
            RegisterCallbacks();

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

            // reset flagu pro jistotu
            privateLobby = false;
        menuComp.dropdown.value = 0;
        menuComp.inputFieldHost.text = "";
        menuComp.warningText.SetActive(false);
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
        if (lobbyID == 0)
            return;
        Debug.Log("Funguje");
        var lobby = new CSteamID(lobbyID);
        CSteamID owner = SteamMatchmaking.GetLobbyOwner(lobby);

        // Zkontroluj, jestli jsem host
        if (owner == SteamUser.GetSteamID())
        {
            // Uzavřu lobby (už nebude joinable)
            SteamMatchmaking.SetLobbyJoinable(lobby, false);
            SteamMatchmaking.SetLobbyData(lobby, "closed", "true");
            Debug.Log("Lobby uzavřena (SetLobbyJoinable false)");
        }

        // Odejdu normálně
        SteamMatchmaking.LeaveLobby(lobby);
        lobbyID = 0;

        // Ukončím síťovou část
        if (NetworkServer.active && owner == SteamUser.GetSteamID())
            NetworkManager.singleton.StopHost();
        else if (NetworkClient.isConnected)
            NetworkManager.singleton.StopClient();
        if(menuComp != null)
        {
            menuComp.panelSwapper.gameObject.SetActive(true);
            gameObject.SetActive(true);
            Debug.Log("Do hl. menu");
            menuComp.panelSwapper.SwapPanel("MainPanel");
        }
    }



    public void OnDropdownChange()
        {
            privateLobby = menuComp.dropdown.value == 1;
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
                    RegisterCallbacks();
                    SteamMatchmaking.JoinLobby(targetLobbyID);
                }

            }
            else
            {
                Debug.Log("Lobby nemá heslo → rovnou join");
                SteamMatchmaking.JoinLobby(targetLobbyID);
            }
        }

    }
