using UnityEngine;
using UnityEngine.UI;

public class SkyBackgroundController : MonoBehaviour
{
    [Header("References")]
    public RawImage skyBackground;
    public SkyCoordinateConverter coordinateConverter;


    [Header("Panorama Settings")]

    [Tooltip("Flip this between -1 and 1 if horizontal movement feels reversed.")]
    public float horizontalDirection = -1f;

    [Tooltip("How much of the panorama is vertically visible.")]
    [Range(0.2f, 1f)]
    public float verticalViewSize = 0.55f;


    private float lastRA = -999f;
    private float lastDec = -999f;
    private float lastFOV = -999f;


    private void Start()
    {
        UpdateBackground(true);
    }


    private void LateUpdate()
    {
        if (coordinateConverter == null ||
            skyBackground == null)
        {
            return;
        }


        if (!Mathf.Approximately(
                lastRA,
                coordinateConverter.centerRightAscension) ||
            !Mathf.Approximately(
                lastDec,
                coordinateConverter.centerDeclination) ||
            !Mathf.Approximately(
                lastFOV,
                coordinateConverter.horizontalFieldOfView))
        {
            UpdateBackground(false);
        }
    }


    private void UpdateBackground(bool force)
    {
        if (skyBackground == null ||
            coordinateConverter == null)
        {
            return;
        }


        float centerRA =
            coordinateConverter.centerRightAscension;


        float centerDec =
            coordinateConverter.centerDeclination;


        float horizontalFOV =
            coordinateConverter.horizontalFieldOfView;


        // =====================================================
        // HORIZONTAL
        //
        // Full panorama = 360 degrees.
        // RA full circle = 24 hours.
        // =====================================================

        float horizontalViewSize =
            horizontalFOV / 360f;


        float centerU =
            horizontalDirection *
            (centerRA / 24f);


        float uvX =
            centerU -
            horizontalViewSize * 0.5f;


        // =====================================================
        // VERTICAL
        //
        // Declination:
        // -90 = bottom
        //   0 = middle
        // +90 = top
        // =====================================================

        float normalizedDec =
            Mathf.InverseLerp(
                -90f,
                90f,
                centerDec
            );


        float uvY =
            normalizedDec -
            verticalViewSize * 0.5f;


        uvY =
            Mathf.Clamp(
                uvY,
                0f,
                1f - verticalViewSize
            );


        // =====================================================
        // APPLY
        // =====================================================

        skyBackground.uvRect =
            new Rect(
                uvX,
                uvY,
                horizontalViewSize,
                verticalViewSize
            );


        lastRA =
            coordinateConverter.centerRightAscension;

        lastDec =
            coordinateConverter.centerDeclination;

        lastFOV =
            coordinateConverter.horizontalFieldOfView;
    }
}