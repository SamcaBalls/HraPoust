using Mirror;
using SteamLobbyTutorial;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class PlayerStats : NetworkBehaviour
{
    public enum TweakingState
    {
        None,
        ShakyVision,
        InvertedInputs,
        Hallucination
    }

    [SyncVar] private TweakingState currentTweaking = TweakingState.None;

    private Dictionary<TweakingState, Action> tweakingActions;

    [Header("Stats")]
    [SyncVar] public int health = 100;
    [SyncVar] public float fatigue = 0f;

    [Header("Config")]
    public int maxHealth = 100;
    public float maxFatigue = 100f;

    public float baseFatigueRate = 1f;
    public float runFatigueRate = 5f;
    public float fatigueRecoveryRate = 2f;

    public float tweakingRange;
    bool isSafe = false;
    bool ragdoll = false;
    public bool lockinIn = false;

    public static event Action OnAnyPlayerDied;

    [SerializeField] PlayerInteraction playerInteraction;
    [SerializeField] RagdollHandler ragdollHandler;
    [SerializeField] CameraSwapper cameraSwapper;
    [SerializeField] HideLocalPlayerModel headShower;
    Volume vol;

    public DrinkableObject drinkObject;   

    void Awake()
    {
        vol = FindAnyObjectByType<Volume>();

        
    }

    void Start()
    {
        if (isLocalPlayer)
        {
            cameraSwapper.SwapCamera(0);
            tweakingRange = Random.Range(75, 85);

            tweakingActions = new Dictionary<TweakingState, Action>
        {
            { TweakingState.ShakyVision, () => TweakingBehaviors.instance.ShakyVision(vol) },
            { TweakingState.InvertedInputs, () => TweakingBehaviors.instance.InvertedInputs() },
            { TweakingState.Hallucination, () => TweakingBehaviors.instance.Hallucination() }
        };
        }


    }

    void Update()
    {
        if (!isLocalPlayer) return;

        HandleFatigue();

        if (Input.GetKeyDown(KeyCode.K))
            CmdTakeDamage(10);

        if (Input.GetKeyDown(KeyCode.L))
            StartCoroutine(LockIn());
    }

    void HandleFatigue()
    {
        if (lockinIn) return;

        float delta = baseFatigueRate * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftShift))
            delta += runFatigueRate * Time.deltaTime;

        CmdChangeFatigue(fatigue + delta);

        if (fatigue >= maxFatigue && !isSafe)
        {
            RpcOnBurnout();
        }
        if (fatigue > tweakingRange && currentTweaking == TweakingState.None)
        {
            StartTweaking();
        }
    }

    [Command]
    public void CmdTakeDamage(int amount)
    {
        health = Mathf.Max(health - amount, 0);
        if (health <= 0)
            RpcOnDeath();
    }

    [Command]
    public void CmdChangeFatigue(float value)
    {
        fatigue = Mathf.Clamp(value, 0, maxFatigue);
    }

    public IEnumerator ChangeFatigueSmooth(float changeValue, float time)
    {
        float startFatigue = fatigue;
        float targetFatigue = Mathf.Clamp(startFatigue + changeValue, 0, maxFatigue);

        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;
            float newFatigue = Mathf.Lerp(startFatigue, targetFatigue, t);
            CmdChangeFatigue(newFatigue);
            yield return null;
        }

        // zajistí přesnou cílovou hodnotu na konci
        CmdChangeFatigue(targetFatigue);
    }


    [ClientRpc]
    void RpcOnDeath()
    {
        Debug.Log($"{netIdentity.netId} zemřel!");

        SetRagdollAll(true, -transform.forward);

        playerInteraction.CmdDropItem();

        if (isServer)
            OnAnyPlayerDied?.Invoke(); // jen server to řeší

        if (isLocalPlayer)
        {
            cameraSwapper.SwapCamera(1);
            CmdNotifyDeath();
        }
    }

    [Command]
    void CmdNotifyDeath()
    {
        ulong steamID = SteamUser.GetSteamID().m_SteamID;

        if (GameManager.Instance != null)
            GameManager.Instance.SetPlayerDead(steamID);
    }



    [ClientRpc]
    void RpcOnBurnout()
    {
        if (!ragdoll)
        {
            SetRagdollAll(true, -transform.forward * 10f);
            ragdoll = true;
            cameraSwapper.SwapCamera(2);
        }
    }

    // 🧩 NOVÝ systém synchronizace ragdollu
    [Command]
    public void CmdSetRagdoll(bool on)
    {
        Vector3 force = on ? -transform.forward * 10f : Vector3.zero;
        currentTweaking = TweakingState.None;
        RpcSetRagdoll(on, force);
    }

    [ClientRpc]
    void RpcSetRagdoll(bool on, Vector3 force)
    {
        SetRagdollAll(on, force);
    }

    void SetRagdollAll(bool on, Vector3 force)
    {
        if (isLocalPlayer)
            MovementEnabled(!on);

        var charCon = GetComponent<CharacterController>();
        if (charCon != null) charCon.enabled = !on;

        ragdollHandler.SetRagdoll(on, force);
        headShower.GiveHead(on);
    }

    public void MovementEnabled(bool on)
    {
        var move = GetComponent<PlayerMovementHandler>();
        if (move != null) move.enabled = on;

        var camMove = GetComponentInChildren<PlayerCameraLook>();
        if (camMove != null) camMove.enabled = on;

        var character = GetComponent<CharacterController>();
        if (character != null) character.enabled = on;

        var headbob = GetComponent<Headbob>();
        if (headbob != null) headbob.enabled = on;
    }

    public IEnumerator LockIn()
    {
        lockinIn = true;

        yield return ChangeFatigueSmooth(-fatigue, 5);

        // vypne ragdoll přes server, všem se obnoví postava
        CmdSetRagdoll(false);


        ragdoll = false;
        MovementEnabled(true);
        cameraSwapper.SwapCamera(0);
        headShower.GiveHead(false);
        lockinIn = false;
        CmdNotifyRevive();
    }

    [Command]
    void CmdNotifyRevive()
    {
        ulong steamID = SteamUser.GetSteamID().m_SteamID;
        if (GameManager.Instance != null)
            GameManager.Instance.SetPlayerAlive(steamID);
    }

    void StartTweaking()
    {
        Array values = Enum.GetValues(typeof(TweakingState));
        int randomIndex = 1;//UnityEngine.Random.Range(0, values.Length);
        TweakingState chosen = (TweakingState)values.GetValue(randomIndex);

        currentTweaking = chosen;

        if (tweakingActions.ContainsKey(chosen))
        {
            tweakingActions[chosen].Invoke();
            Debug.Log($"Tweaking start na objektu {gameObject.name}, isLocalPlayer = {isLocalPlayer}, instance = {TweakingBehaviors.instance != null}");
        }
    }
}
