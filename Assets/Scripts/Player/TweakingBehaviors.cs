using SteamLobbyTutorial;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TweakingBehaviors : MonoBehaviour
{
    [SerializeField] Volume vol;

    public static TweakingBehaviors instance;

    // Pro uložení běžících coroutin
    private List<Coroutine> activeCoroutines = new List<Coroutine>();

    private void Awake()
    {
        instance = this;
        if (vol != null) SaveOriginalProfile();
    }

    // Uložení default hodnot jednotlivých parametrů
    private struct EffectDefaults
    {
        public float lensDistortionIntensity;
        public float lensDistortionX;
        public float lensDistortionY;
        public float colorContrast;
        public Color colorFilter;
        public float motionBlurIntensity;
        public float depthFocalLength;
        public float depthFocusDistance;
        public DepthOfFieldMode depthMode;
        public MotionBlurMode motionBlurMode;
    }
    private EffectDefaults defaults;

    // Reference na komponenty (získáme jednou)
    private LensDistortion lensDistortion;
    private ColorAdjustments colorAdjustments;
    private MotionBlur motionBlur;
    private DepthOfField depthOfField;
    private Vignette vignette;

    // Uloží originální hodnoty a získá reference na komponenty
    private void SaveOriginalProfile()
    {
        if (vol == null || vol.profile == null) return;

        // Získání referencí na komponenty
        vol.profile.TryGet(out lensDistortion);
        vol.profile.TryGet(out colorAdjustments);
        vol.profile.TryGet(out motionBlur);
        vol.profile.TryGet(out depthOfField);

        // Uložení default hodnot
        defaults = new EffectDefaults
        {
            lensDistortionIntensity = lensDistortion?.intensity.value ?? 0f,
            lensDistortionX = lensDistortion?.xMultiplier.value ?? 0f,
            lensDistortionY = lensDistortion?.yMultiplier.value ?? 0f,
            colorContrast = colorAdjustments?.contrast.value ?? 0f,
            colorFilter = colorAdjustments?.colorFilter.value ?? Color.white,
            motionBlurIntensity = motionBlur?.intensity.value ?? 0f,
            motionBlurMode = motionBlur?.mode.value ?? MotionBlurMode.CameraOnly,
            depthFocalLength = depthOfField?.focalLength.value ?? 50f,
            depthFocusDistance = depthOfField?.focusDistance.value ?? 10f,
            depthMode = depthOfField?.mode.value ?? DepthOfFieldMode.Off
        };

        Debug.Log("✅ Původní hodnoty volume uloženy");
    }


    //SHAKY VISION

    public void ShakyVision()
    {
        if (lensDistortion == null || colorAdjustments == null || motionBlur == null)
        {
            Debug.LogWarning("⚠️ Komponenty pro ShakyVision nejsou dostupné!");
            return;
        }

        motionBlur.mode.value = MotionBlurMode.CameraAndObjects;
        motionBlur.intensity.value = 1;

        var coroutine = StartCoroutine(ShakyVisionRoutine());
        activeCoroutines.Add(coroutine);
    }

    private IEnumerator ShakyVisionRoutine()
    {
        // Výchozí hodnoty
        float transitionSpeed = 1f;

        while (true)
        {
            // 🎯 Cílové hodnoty (náhodné, ale v rozumném rozsahu)
            float targetX = Random.Range(-1f, 1f);
            float targetY = Random.Range(-1f, 1f);
            float targetIntensity = Random.Range(0.6f, 1.0f);
            float targetContrast = Random.Range(60f, 100f);
            Color targetColor = Color.Lerp(
                new Color(1.0f, 0.6f, 0.3f),   // oranžová
                new Color(0.8f, 0.4f, 0.2f),   // tmavší červenooranžová
                Random.value
            );

            float changeInterval = Random.Range(0.8f, 1.5f);

            // Startovní hodnoty pro hladký přechod
            float startX = lensDistortion.xMultiplier.value;
            float startY = lensDistortion.yMultiplier.value;
            float startIntensity = lensDistortion.intensity.value;
            float startContrast = colorAdjustments.contrast.value;
            Color startColor = colorAdjustments.colorFilter.value;

            float elapsed = 0f;

            // 🌊 Hladký přechod všech hodnot
            while (elapsed < changeInterval)
            {
                elapsed += Time.deltaTime * transitionSpeed;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / changeInterval);

                lensDistortion.xMultiplier.value = Mathf.Lerp(startX, targetX, t);
                lensDistortion.yMultiplier.value = Mathf.Lerp(startY, targetY, t);
                lensDistortion.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, t);
                colorAdjustments.contrast.value = Mathf.Lerp(startContrast, targetContrast, t);
                colorAdjustments.colorFilter.value = Color.Lerp(startColor, targetColor, t);

                yield return null;
            }

        }
    }

    //PARTIAL BLINDNESS

    public void PartialBlindness()
    {
        if (depthOfField == null)
        {
            Debug.LogWarning("⚠️ Depth of Field komponenta není dostupná!");
            return;
        }

        depthOfField.mode.value = DepthOfFieldMode.Bokeh;
        depthOfField.focusDistance.value = 1f;

        var coroutine = StartCoroutine(PartialBlindnessCoroutine());
        activeCoroutines.Add(coroutine);
    }

    IEnumerator PartialBlindnessCoroutine()
    {
        while (true)
        {

            float transitionSpeed = Random.Range(0.3f, 1.5f);
            float changeInterval = Random.Range(0.8f, 1.5f);

            float targetFocalLength = Random.Range(50, 300);

            float startFocalLength = depthOfField.focalLength.value;


            float elapsed = 0f;

            // 🌊 Hladký přechod všech hodnot
            while (elapsed < changeInterval)
            {
                elapsed += Time.deltaTime * transitionSpeed;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / changeInterval);

                depthOfField.focalLength.value = Mathf.Lerp(startFocalLength, targetFocalLength, t);

                yield return null;
            }
        }

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

    IEnumerator Blink(bool close)
    {
        if (colorAdjustments == null)
        {
            if (!vol.profile.TryGet(out colorAdjustments))
            {
                Debug.LogWarning("⚠️ ColorAdjustments nejsou dostupné!");
                yield break;
            }
        }

        float duration = 0.4f; // rychlost přechodu
        float elapsed = 0f;

        // Výchozí hodnoty
        float startContrast = colorAdjustments.contrast.value;
        Color startColor = colorAdjustments.colorFilter.value;

        // Cílové hodnoty (zavřené oči = černá obrazovka)
        float targetContrast = close ? -100f : defaults.colorContrast;
        Color targetColor = close ? Color.black : defaults.colorFilter;

        // Hladký přechod
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            colorAdjustments.contrast.value = Mathf.Lerp(startContrast, targetContrast, t);
            colorAdjustments.colorFilter.value = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }
    }



    // === Reset jednoho volume ===
    public void ResetVolume()
    {
        // 1. Zastavit všechny běžící coroutines
        StopAllActiveCoroutines();

        // 2. Resetovat hodnoty na default
        StartCoroutine(ResetToDefaults());

        Debug.Log($"✅ Volume {vol.name} resetován zpět na původní hodnoty.");
    }

    // Resetuje všechny hodnoty na původní default
    IEnumerator ResetToDefaults()
    {
        yield return StartCoroutine(Blink(true));
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = defaults.lensDistortionIntensity;
            lensDistortion.xMultiplier.value = defaults.lensDistortionX;
            lensDistortion.yMultiplier.value = defaults.lensDistortionY;
        }

        if (motionBlur != null)
        {
            motionBlur.intensity.value = defaults.motionBlurIntensity;
            motionBlur.mode.value = defaults.motionBlurMode;
        }

        if (depthOfField != null)
        {
            depthOfField.focalLength.value = defaults.depthFocalLength;
            depthOfField.focusDistance.value = defaults.depthFocusDistance;
            depthOfField.mode.value = defaults.depthMode;
        }

        yield return StartCoroutine(Blink(false));

        if (colorAdjustments != null)
        {
            colorAdjustments.contrast.value = defaults.colorContrast;
            colorAdjustments.colorFilter.value = defaults.colorFilter;
        }
    }

    // Zastaví všechny aktivní coroutines
    private void StopAllActiveCoroutines()
    {
        foreach (var coroutine in activeCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeCoroutines.Clear();
    }


    // Cleanup při destroy
    private void OnDestroy()
    {
        StopAllActiveCoroutines();
    }
}