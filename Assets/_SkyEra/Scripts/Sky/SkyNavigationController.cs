using UnityEngine;
using UnityEngine.EventSystems;

public class SkyNavigationController : MonoBehaviour,
    IDragHandler
{
    [Header("References")]
    public RectTransform skyViewArea;
    public RectTransform skyContent;

    public SkyCoordinateConverter coordinateConverter;
    public StarRenderer starRenderer;


    [Header("Zoom Settings")]

    [Min(0.1f)]
    public float minZoom = 1f;

    [Min(0.1f)]
    public float maxZoom = 3f;

    [Min(0.05f)]
    public float zoomStep = 0.25f;


    [Header("Sky Rotation Settings")]

    [Tooltip("Horizontal swipe sensitivity.")]
    public float horizontalRotationSpeed = 0.015f;

    [Tooltip("Vertical swipe sensitivity.")]
    public float verticalRotationSpeed = 0.08f;


    private float currentZoom = 1f;


    public float CurrentZoom =>
        currentZoom;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        currentZoom = minZoom;

        ApplyZoom();
    }


    // =========================================================
    // ZOOM IN
    // =========================================================

    public void ZoomIn()
    {
        currentZoom =
            Mathf.Clamp(
                currentZoom + zoomStep,
                minZoom,
                maxZoom
            );


        ApplyZoom();
    }


    // =========================================================
    // ZOOM OUT
    // =========================================================

    public void ZoomOut()
    {
        currentZoom =
            Mathf.Clamp(
                currentZoom - zoomStep,
                minZoom,
                maxZoom
            );


        ApplyZoom();
    }


    // =========================================================
    // RESET VIEW
    // =========================================================

    public void ResetView()
    {
        currentZoom =
            minZoom;


        ApplyZoom();


        if (coordinateConverter != null)
        {
            coordinateConverter
                .SetCenterRightAscension(6f);


            coordinateConverter
                .SetCenterDeclination(10f);
        }


        if (starRenderer != null)
        {
            starRenderer
                .RefreshCurrentView();
        }
    }


    // =========================================================
    // APPLY ZOOM
    // =========================================================

    private void ApplyZoom()
    {
        if (skyContent == null)
        {
            return;
        }


        skyContent.localScale =
            Vector3.one *
            currentZoom;


        // Keep content centered.
        skyContent.anchoredPosition =
            Vector2.zero;
    }


    // =========================================================
    // DRAG / SWIPE = ROTATE CELESTIAL SKY
    // =========================================================

    public void OnDrag(
        PointerEventData eventData
    )
    {
        if (coordinateConverter == null ||
            starRenderer == null)
        {
            return;
        }


        Vector2 delta =
            eventData.delta;


        // -----------------------------------------------------
        // HORIZONTAL
        //
        // Drag left/right changes Right Ascension.
        // RA range = 0 - 24 hours.
        // -----------------------------------------------------

        float newRA =
            coordinateConverter.centerRightAscension
            +
            delta.x *
            horizontalRotationSpeed;


        newRA =
            Mathf.Repeat(
                newRA,
                24f
            );


        coordinateConverter
            .SetCenterRightAscension(
                newRA
            );


        // -----------------------------------------------------
        // VERTICAL
        //
        // Drag up/down changes Declination.
        // -----------------------------------------------------

        float newDec =
            coordinateConverter.centerDeclination
            +
            delta.y *
            verticalRotationSpeed;


        newDec =
            Mathf.Clamp(
                newDec,
                -80f,
                80f
            );


        coordinateConverter
            .SetCenterDeclination(
                newDec
            );


        // -----------------------------------------------------
        // REBUILD CURRENT CELESTIAL VIEW
        // -----------------------------------------------------

        starRenderer
            .RefreshCurrentView();
    }
}