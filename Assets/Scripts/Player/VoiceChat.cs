using UnityEngine;
using Mirror;
using Concentus.Structs;
using Concentus.Enums;
using System;
using System.Collections;

public class VoiceChat : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private AudioSource audioSource;

    [Header("Sample / Opus")]
    [Tooltip("Preferovaný sample rate (pokud mikrofon podporuje)")]
    public int desiredSampleRate = 48000;
    [Tooltip("Velikost frame v ms (20ms = dobrý kompromis)")]
    public int frameMs = 20;

    [Header("Noise gate")]
    [Tooltip("RMS prah; pod tímto se rámec neposílá")]
    public float sendRmsThreshold = 0.005f;

    [Header("Playback buffer")]
    [Tooltip("Kolik sekund playback buffer drží (zvýšit při výpadcích)")]
    public int playbackBufferSeconds = 2;

    [Header("Proximity")]
    public float audibleDistance = 70f;
    public float noReverbDistance = 10f;

    [SerializeField] private Settings playerSettings;


    private int sampleRate;
    private int frameSize; 

    private AudioClip micClip;
    private int micPosition = 0;

    private OpusEncoder encoder;
    private OpusDecoder decoder;

    // temporary buffer pro čtení z mikrofonu
    private float[] micTemp;

    // playback ring buffer (jednokanálové)
    private float[] playBuffer;
    private int playBufferLen;
    private int playWritePos = 0;
    private int playReadPos = 0;
    private readonly object bufferLock = new object();

    // statistikky a ladění
    private int sentPacketsThisSec = 0;
    private int recvPacketsThisSec = 0;
    private float statsTimer = 0f;
    private float lastSentRms = 0f;

    //Stav brány
    static int gateHoldCounter = 0;
    static bool gateOpen = false;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("[VoiceChat] AudioSource chybí - přidej komponentu nebo ji přiřaď.");
    }

    void Start()
    {
        sampleRate = desiredSampleRate;
        frameSize = Mathf.Max(1, sampleRate * frameMs / 1000);
        micTemp = new float[frameSize];


        SetupPlaybackClip(sampleRate);

        if (isLocalPlayer)
        {
            Debug.Log("[VoiceChat] Local player: starting microphone (async)...");
            StartCoroutine(StartMicCoroutine());
        }
        else
        {

            try
            {
                decoder = OpusDecoder.Create(sampleRate, 1);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[VoiceChat] Nelze inicializovat decoder hned: " + e.Message);
            }
        }
    }

    IEnumerator StartMicCoroutine()
    {
        string micDevice = null;
        if (playerSettings != null && playerSettings.micIndex >= 0 && playerSettings.micIndex < Microphone.devices.Length)
            micDevice = Microphone.devices[playerSettings.micIndex];

        Debug.Log("[VoiceChat] Using mic: " + (micDevice ?? "Default"));

        int minFreq = 0, maxFreq = 0;
        Microphone.GetDeviceCaps(micDevice, out minFreq, out maxFreq);

        int chosen = desiredSampleRate;
        if (minFreq != 0 || maxFreq != 0)
        {
            if (!(chosen >= minFreq && chosen <= maxFreq))
            {
                int[] fallbacks = new int[] { 48000, 44100, 16000, 8000 };
                int found = -1;
                foreach (var f in fallbacks)
                {
                    if (f >= minFreq && f <= maxFreq) { found = f; break; }
                }
                if (found != -1) chosen = found;
                else chosen = Mathf.Clamp(desiredSampleRate, minFreq, Math.Max(minFreq, maxFreq));
            }
        }
        sampleRate = chosen;

        // ✅ Zkontroluj, jestli je sample rate pro Opus platný
        int[] validRates = { 8000, 12000, 16000, 24000, 48000 };
        if (Array.IndexOf(validRates, sampleRate) < 0)
        {
            Debug.LogWarning($"[VoiceChat] Sample rate {sampleRate} is invalid for Opus. Using 48000 instead.");
            sampleRate = 48000;
        }

        // znovu přepočítej frameSize podle opraveného sampleRate
        frameSize = Mathf.Max(1, sampleRate * frameMs / 1000);
        micTemp = new float[frameSize];

        SetupPlaybackClip(sampleRate);

        try
        {
            encoder = OpusEncoder.Create(sampleRate, 1, OpusApplication.OPUS_APPLICATION_VOIP);
            Debug.Log($"[VoiceChat] Encoder created at {sampleRate} Hz");
        }
        catch (Exception e)
        {
            Debug.LogError($"[VoiceChat] Encoder error: {e.Message}");
        }


        try
        {
            decoder = OpusDecoder.Create(sampleRate, 1);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[VoiceChat] Decoder error: " + e.Message);
        }

        micClip = Microphone.Start(micDevice, true, 1, sampleRate);

        float t0 = Time.time;
        while (Microphone.GetPosition(micDevice) <= 0)
        {
            if (Time.time - t0 > 5f)
            {
                Debug.LogWarning("[VoiceChat] Timeout při startu mikrofonu.");
                break;
            }
            yield return null;
        }

        Debug.Log($"[VoiceChat] Microphone started ({micDevice ?? "Default"}) at {sampleRate}Hz, frameSize={frameSize}");
    }

    void SetupPlaybackClip(int newSampleRate)
    {
        if (audioSource == null) return;

        AudioClip clip = AudioClip.Create("VoiceOut_" + newSampleRate, newSampleRate, 1, newSampleRate, false);
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // 🎧 3D audio s lineárním útlumem
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Custom;
        audioSource.maxDistance = audibleDistance;

        audioSource.Play();

        playBufferLen = Mathf.Max(1024, newSampleRate * Mathf.Max(1, playbackBufferSeconds));
        playBuffer = new float[playBufferLen];
        playWritePos = playReadPos = 0;
    }


    void Update()
    {
        // statistiky
        statsTimer += Time.deltaTime;
        if (statsTimer >= 1f)
        {
            //Debug.Log($"[VoiceChat] Sent/sec={sentPacketsThisSec}, Recv/sec={recvPacketsThisSec}, LastRMS={lastSentRms:F5}, BufferUsed={(GetBufferedSamples())} samples");
            sentPacketsThisSec = recvPacketsThisSec = 0;
            statsTimer = 0f;
        }

        if (!isLocalPlayer) return;
        if (micClip == null) return;

        int position = Microphone.GetPosition(null);
        int sampleDiff = position - micPosition;
        if (sampleDiff < 0) sampleDiff += micClip.samples;

        if (sampleDiff < frameSize)
            return;

        // přečti přesně frameSize vzorků (řeší wrap-around)
        ReadMicSamples(micPosition, micTemp);
        micPosition = (micPosition + frameSize) % micClip.samples;

        // Noise gate s "hold" a "release"
        float sum = 0f;
        for (int i = 0; i < micTemp.Length; i++) sum += micTemp[i] * micTemp[i];
        float rms = Mathf.Sqrt(sum / micTemp.Length);
        lastSentRms = rms;

        // Parametry brány
        float attackThreshold = sendRmsThreshold;   // kdy otevřít
        float releaseThreshold = sendRmsThreshold * 0.7f; // kdy zavřít
        int holdFrames = 5;  // drž otevřeno X frameů po překročení prahu


        if (rms > attackThreshold)
        {
            gateOpen = true;
            gateHoldCounter = holdFrames;
        }
        else if (rms < releaseThreshold)
        {
            if (gateHoldCounter > 0)
                gateHoldCounter--;
            else
                gateOpen = false;
        }

        if (!gateOpen)
        {
            // ztiš signál (ne úplně mute – aby se Opus stabilizoval)
            for (int i = 0; i < micTemp.Length; i++)
                micTemp[i] *= 0.2f;

            // můžeme vrátit, pokud chceš ušetřit síť, ale takto je plynulejší
            // return;
        }


        // převod na short s clampem
        short[] pcmShorts = new short[frameSize];
        for (int i = 0; i < frameSize; i++)
        {
            float f = micTemp[i];
            float v = Mathf.Clamp(f, -1f, 1f) * short.MaxValue;
            pcmShorts[i] = (short)Mathf.RoundToInt(v);
        }

        // enkóduj
        if (encoder == null)
        {
            Debug.LogWarning("[VoiceChat] Encoder neni inicializovaný.");
            return;
        }

        byte[] encoded = new byte[4000]; // dost velký buffer
        int encodedLength = 0;
        try
        {
            encodedLength = encoder.Encode(pcmShorts, 0, frameSize, encoded, 0, encoded.Length);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[VoiceChat] Encode exception: " + e.Message);
            return;
        }

        if (encodedLength > 0)
        {
            byte[] dataToSend = new byte[encodedLength];
            Array.Copy(encoded, dataToSend, encodedLength);
            CmdSendVoice(dataToSend);
            sentPacketsThisSec++;
        }
    }

    // Čtení z micClip s ošetřením wrap-aroundu
    void ReadMicSamples(int start, float[] dest)
    {
        if (micClip == null || dest == null) return;
        int need = dest.Length;
        int toEnd = micClip.samples - start;
        if (toEnd >= need)
        {
            micClip.GetData(dest, start);
        }
        else
        {
            // číst ve dvou krocích
            float[] part1 = new float[toEnd];
            float[] part2 = new float[need - toEnd];
            micClip.GetData(part1, start);
            micClip.GetData(part2, 0);
            Array.Copy(part1, 0, dest, 0, part1.Length);
            Array.Copy(part2, 0, dest, part1.Length, part2.Length);
        }
    }

    [Command]
    void CmdSendVoice(byte[] data)
    {
        RpcReceiveVoice(data);
    }

    [ClientRpc]
    void RpcReceiveVoice(byte[] data)
    {
        if (isLocalPlayer)
            return;

        // proximity check
        float distance = 0f;
        if (NetworkClient.localPlayer != null)
        {
            distance = Vector3.Distance(transform.position, NetworkClient.localPlayer.transform.position);
            if (distance > audibleDistance)
                return;
        }

        if (!isLocalPlayer && distance > noReverbDistance)
        {
            AddReverb(audioSource, distance);
            Debug.Log($"[VoiceChat] Received voice from {gameObject.name} at distance {distance:F2} meters");
        }


        if (decoder == null)
        {
            try
            {
                decoder = OpusDecoder.Create(sampleRate, 1);
                Debug.Log("[VoiceChat] Decoder initialized dynamically");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[VoiceChat] Nelze vytvořit decoder: " + e.Message);
                return;
            }
        }

        // dekóduj a ulož do bufferu (bez změn)
        short[] decoded = new short[frameSize];
        int samples = 0;
        try
        {
            samples = decoder.Decode(data, 0, data.Length, decoded, 0, frameSize, false);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[VoiceChat] Decode exception: " + e.Message);
            return;
        }

        if (samples <= 0) return;

        float[] floatData = new float[samples];
        for (int i = 0; i < samples; i++)
            floatData[i] = decoded[i] / (float)short.MaxValue;

        lock (bufferLock)
        {
            int free = GetFreeSpace();
            if (free < samples) return;

            for (int i = 0; i < samples; i++)
            {
                playBuffer[playWritePos] = floatData[i];
                playWritePos++;
                if (playWritePos >= playBufferLen) playWritePos = 0;
            }
        }

        recvPacketsThisSec++;
    }


    // pomocné pro buffer
    int GetBufferedSamples()
    {
        lock (bufferLock)
        {
            int used = playWritePos - playReadPos;
            if (used < 0) used += playBufferLen;
            return used;
        }
    }
    int GetFreeSpace()
    {
        lock (bufferLock)
        {
            int used = playWritePos - playReadPos;
            if (used < 0) used += playBufferLen;
            return playBufferLen - 1 - used;
        }
    }

    // audio thread - Unity volá pravidelně
    void OnAudioFilterRead(float[] data, int channels)
    {
        // vyplníme data z playBuffer; pokud chybí, vyplníme 0 (silence)
        int outLen = data.Length;
        lock (bufferLock)
        {
            for (int i = 0; i < outLen; i += channels)
            {
                float sample = 0f;
                int used = playWritePos - playReadPos;
                if (used < 0) used += playBufferLen;
                if (used > 0)
                {
                    sample = playBuffer[playReadPos];
                    playReadPos++;
                    if (playReadPos >= playBufferLen) playReadPos = 0;
                }
                for (int ch = 0; ch < channels; ch++)
                {
                    data[i + ch] = sample;
                }
            }
        }
    }

    void OnDisable()
    {
        if (isLocalPlayer && Microphone.IsRecording(null))
        {
            Microphone.End(null);
            Debug.Log("[VoiceChat] Microphone stopped.");
        }
    }

    void AddReverb(AudioSource src, float distance)
    {
        if (src.GetComponent<AudioReverbFilter>() == null)
        {
            var reverb = src.gameObject.AddComponent<AudioReverbFilter>();
            reverb.reverbPreset = AudioReverbPreset.Arena; // můžeš zkusit různé
                                                          // intenzita podle vzdálenosti
            reverb.dryLevel = 0;
            reverb.room = Mathf.Clamp((distance / audibleDistance) * 1000f, 0, 1000);
        }
    }


    // jednoduchý overlay pro ladění
    /*void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 320, 110), "VoiceChat debug");
        GUI.Label(new Rect(20, 34, 300, 20), $"SampleRate: {sampleRate} Hz  frameSize: {frameSize}");
        GUI.Label(new Rect(20, 54, 300, 20), $"Sent/s: {sentPacketsThisSec}  Recv/s: {recvPacketsThisSec}");
        GUI.Label(new Rect(20, 74, 300, 20), $"RMS(last sent): {lastSentRms:F5}");
        GUI.Label(new Rect(20, 94, 300, 20), $"Buffer samples: {GetBufferedSamples()} / {playBufferLen}");
    }*/
}
