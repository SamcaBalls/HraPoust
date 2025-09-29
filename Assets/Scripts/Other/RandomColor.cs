using Mirror;
using UnityEngine;

public class RandomColor : NetworkBehaviour
{
    [SerializeField] Renderer[] bodyParts;
    [SerializeField] Renderer[] hair;
    [SerializeField] Renderer[] accessories;
    [SerializeField] GameObject beard;
    [SerializeField] GameObject moustache;

    ColorLists colorLists;

    [SyncVar(hook = nameof(OnSkinChanged))] int skinIndex;
    [SyncVar(hook = nameof(OnHairChanged))] int hairIndex;
    [SyncVar(hook = nameof(OnAccChanged))] int accIndex;
    [SyncVar(hook = nameof(OnBeardChanged))] bool hasBeard;
    [SyncVar(hook = nameof(OnMoustacheChanged))] bool hasMoustache;

    public override void OnStartServer()
    {
        // najdi ColorLists jen na serveru
        colorLists = FindAnyObjectByType<ColorLists>();

        // server vygeneruje random jen jednou
        skinIndex = Random.Range(0, colorLists.skinColors.Length);
        hairIndex = Random.Range(0, colorLists.hairColors.Length);
        accIndex = Random.Range(0, colorLists.accesoriesColors.Length);

        hasBeard = Random.Range(0, 2) == 0;
        hasMoustache = Random.Range(0, 2) == 0;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // klient si také musí najít ColorLists
        colorLists = FindAnyObjectByType<ColorLists>();

        // hned aplikuj aktuální SyncVar hodnoty
        OnSkinChanged(0, skinIndex);
        OnHairChanged(0, hairIndex);
        OnAccChanged(0, accIndex);
        OnBeardChanged(false, hasBeard);
        OnMoustacheChanged(false, hasMoustache);
    }

    // hooky – spustí se na všech klientech (i hostu)
    void OnSkinChanged(int _, int newIndex) => Apply(bodyParts, colorLists?.skinColors[newIndex]);
    void OnHairChanged(int _, int newIndex) => Apply(hair, colorLists?.hairColors[newIndex]);
    void OnAccChanged(int _, int newIndex) => Apply(accessories, colorLists?.accesoriesColors[newIndex]);
    void OnBeardChanged(bool _, bool newValue) => beard?.SetActive(newValue);
    void OnMoustacheChanged(bool _, bool newValue) => moustache?.SetActive(newValue);

    void Apply(Renderer[] parts, Material mat)
    {
        if (mat == null) return;
        foreach (var r in parts)
            r.material = mat;
    }
}
