using UnityEngine;
using UnityEngine.EventSystems;

public class SkyNavigationController : MonoBehaviour,
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
    public float panSpeed = 0.65f;

    private float currentZoom = 1f;

    public float CurrentZoom => currentZoom;


    private void Start()
    {
        currentZoom = minZoom;

        ApplyZoom();
        ResetPosition();
    }


    // -------------------------
    // ZOOM
    // -------------------------

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


    public void ResetView()
    {
        currentZoom = minZoom;

        ApplyZoom();
        ResetPosition();
    }


    private void ApplyZoom()
    {
        if (skyContent == null)
            return;

        skyContent.localScale =
            Vector3.one * currentZoom;
    }


    // -------------------------
    // PAN
    // -------------------------

    public void OnDrag(PointerEventData eventData)
    {
        if (skyContent == null ||
            skyViewArea == null)
        {
            return;
        }

        // Pan only after zooming in
        if (currentZoom <= 1.001f)
        {
            return;
        }

        Vector2 movement =
            eventData.delta *
            panSpeed /
            currentZoom;

        skyContent.anchoredPosition +=
            movement;

        ClampPan();
    }


    // -------------------------
    // BOUNDARIES
    // -------------------------

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


    private void ResetPosition()
    {
        if (skyContent != null)
        {
            skyContent.anchoredPosition =
                Vector2.zero;
        }
    }
}