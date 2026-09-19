using UnityEngine;
using UnityEngine.UI;

public class StarView : MonoBehaviour
{
    [Header("Star Images")]
    public Image coreImage;
    public Image glowImage;


    [Header("Size Settings")]

    [Tooltip("Smallest visual scale for faint stars.")]
    public float minimumScale = 0.45f;

    [Tooltip("Largest visual scale for very bright stars.")]
    public float maximumScale = 1.45f;


    [Header("Brightness Settings")]

    [Range(0.05f, 1f)]
    public float minimumCoreAlpha = 0.40f;

    [Range(0.05f, 1f)]
    public float maximumCoreAlpha = 1f;

    [Range(0f, 1f)]
    public float minimumGlowAlpha = 0.05f;

    [Range(0f, 1f)]
    public float maximumGlowAlpha = 0.65f;


    // =========================================================
    // SETUP STAR VISUAL
    // =========================================================

    public void Setup(StarData data)
    {
        if (data == null)
        {
            return;
        }


        // -----------------------------------------------------
        // MAGNITUDE RANGE
        //
        // Bright stars:
        // Sirius ? -1.46
        //
        // Faint stars in our selected patterns:
        // roughly +4
        // -----------------------------------------------------

        float brightnessFactor =
            Mathf.InverseLerp(
                4.0f,
                -1.5f,
                data.magnitude
            );


        brightnessFactor =
            Mathf.Clamp01(
                brightnessFactor
            );


        // -----------------------------------------------------
        // STAR SIZE
        // -----------------------------------------------------

        float starScale =
            Mathf.Lerp(
                minimumScale,
                maximumScale,
                brightnessFactor
            );


        // Optional manual size multiplier from StarData
        float manualSize =
            Mathf.Max(
                0.1f,
                data.size
            );


        transform.localScale =
            Vector3.one *
            starScale *
            manualSize;


        // -----------------------------------------------------
        // CORE BRIGHTNESS
        // -----------------------------------------------------

        if (coreImage != null)
        {
            float coreAlpha =
                Mathf.Lerp(
                    minimumCoreAlpha,
                    maximumCoreAlpha,
                    brightnessFactor
                );


            Color coreColor =
                coreImage.color;


            coreColor.a =
                coreAlpha;


            coreImage.color =
                coreColor;
        }


        // -----------------------------------------------------
        // GLOW BRIGHTNESS
        // -----------------------------------------------------

        if (glowImage != null)
        {
            float glowAlpha =
                Mathf.Lerp(
                    minimumGlowAlpha,
                    maximumGlowAlpha,
                    brightnessFactor
                );


            Color glowColor =
                glowImage.color;


            glowColor.a =
                glowAlpha;


            glowImage.color =
                glowColor;


            // Brighter stars also get slightly larger glow
            float glowScale =
                Mathf.Lerp(
                    0.75f,
                    1.35f,
                    brightnessFactor
                );


            glowImage.rectTransform.localScale =
                Vector3.one *
                glowScale;
        }
    }
}