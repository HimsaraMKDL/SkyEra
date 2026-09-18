using UnityEngine;
using UnityEngine.EventSystems;

public class SkyNavigationController : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler
{
    [Header("References")]
    public RectTransform skyViewArea;
    public RectTransform skyContent;

    [Header("Zoom Settings")]
    [Min(0.1f)]
    public float minZoom = 1f;

    [Min(0.1f)]
    public float maxZoom = 3f;

    [Min(0.05f)]
    public float zoomStep = 0.25f;

    [Header("Pan Settings")]
    [Range(0.1f, 2f)]
    public float panSpeed = 1f;

    private float currentZoom = 1f;

    public float CurrentZoom => currentZoom;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        currentZoom = minZoom;

        ApplyZoom();
        ResetPosition();
    }


    // =========================================================
    // ZOOM IN
    // =========================================================

    public void ZoomIn()
    {
        currentZoom = Mathf.Clamp(
            currentZoom + zoomStep,
            minZoom,
            maxZoom
        );

        ApplyZoom();
        ClampPan();
    }


    // =========================================================
    // ZOOM OUT
    // =========================================================

    public void ZoomOut()
    {
        currentZoom = Mathf.Clamp(
            currentZoom - zoomStep,
            minZoom,
            maxZoom
        );

        ApplyZoom();
        ClampPan();

        if (Mathf.Approximately(currentZoom, minZoom))
        {
            ResetPosition();
        }
    }


    // =========================================================
    // RESET
    // =========================================================

    public void ResetView()
    {
        currentZoom = minZoom;

        ApplyZoom();
        ResetPosition();
    }


    // =========================================================
    // APPLY ZOOM
    // =========================================================

    private void ApplyZoom()
    {
        if (skyContent == null)
            return;

        skyContent.localScale =
            Vector3.one * currentZoom;
    }


    // =========================================================
    // BEGIN DRAG
    // =========================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Required so Unity starts the drag event sequence.
    }


    // =========================================================
    // DRAG / PAN
    // =========================================================

    public void OnDrag(PointerEventData eventData)
    {
        if (skyContent == null ||
            skyViewArea == null)
        {
            return;
        }


        // Do not pan when fully zoomed out
        if (currentZoom <= minZoom + 0.001f)
        {
            return;
        }


        // Natural mouse movement
        Vector2 movement =
            eventData.delta * panSpeed;


        skyContent.anchoredPosition +=
            movement;


        ClampPan();
    }


    // =========================================================
    // CLAMP PAN
    // =========================================================

    private void ClampPan()
    {
        if (skyContent == null ||
            skyViewArea == null)
        {
            return;
        }


        float maxX =
            skyViewArea.rect.width *
            (currentZoom - 1f) *
            0.5f;


        float maxY =
            skyViewArea.rect.height *
            (currentZoom - 1f) *
            0.5f;


        Vector2 position =
            skyContent.anchoredPosition;


        position.x =
            Mathf.Clamp(
                position.x,
                -maxX,
                maxX
            );


        position.y =
            Mathf.Clamp(
                position.y,
                -maxY,
                maxY
            );


        skyContent.anchoredPosition =
            position;
    }


    // =========================================================
    // RESET POSITION
    // =========================================================

    private void ResetPosition()
    {
        if (skyContent != null)
        {
            skyContent.anchoredPosition =
                Vector2.zero;
        }
    }
}