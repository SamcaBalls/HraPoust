using SteamLobbyTutorial;
using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TweakingBehaviors : MonoBehaviour
{

    public static TweakingBehaviors instance;

    private void Awake()
    {
        instance = this;
    }

    public void ShakyVision(Volume vol)
    {
        if (vol.profile.TryGet(out LensDistortion lensDistortion) &&
            vol.profile.TryGet(out ColorAdjustments colorAdjustments) &&
            vol.profile.TryGet(out MotionBlur motionBlur)
            )
        {
            motionBlur.mode.value = MotionBlurMode.CameraAndObjects;
            motionBlur.intensity.value = 1;

            StartCoroutine(ShakyVisionRoutine(lensDistortion, colorAdjustments));
        }
    }

    private IEnumerator ShakyVisionRoutine(LensDistortion lensDistortion, ColorAdjustments colorAdjustments)
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


    public void PartialBlindness()
    {
        Debug.Log("Partial blindness effect started");
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

    public void ResetAll()
    {

    }
}
