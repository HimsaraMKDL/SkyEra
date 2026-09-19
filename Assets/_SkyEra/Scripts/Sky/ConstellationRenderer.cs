using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConstellationRenderer : MonoBehaviour
{
    [Header("References")]
    public StarRenderer starRenderer;

    [Header("Constellations")]
    public ConstellationData[] constellations;

    [Header("Layers")]
    public RectTransform constellationLayer;
    public RectTransform labelLayer;


    [Header("Label Settings")]
    public float labelFontSize = 22f;

    public Color labelColor =
        new Color(
            0.8f,
            0.9f,
            1f,
            0.9f
        );


    private readonly List<GameObject> lineObjects =
        new List<GameObject>();


    private readonly List<GameObject> labelObjects =
        new List<GameObject>();


    // =========================================================
    // ENABLE
    // =========================================================

    private void OnEnable()
    {
        if (starRenderer != null)
        {
            starRenderer.StarsRendered +=
                HandleStarsRendered;
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (starRenderer == null)
        {
            Debug.LogError(
                "ConstellationRenderer: StarRenderer missing."
            );

            return;
        }


        if (starRenderer.spawnedStars.Count > 0)
        {
            HandleStarsRendered(
                starRenderer.CurrentEraIndex
            );
        }
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        if (starRenderer != null)
        {
            starRenderer.StarsRendered -=
                HandleStarsRendered;
        }
    }


    // =========================================================
    // WHEN STARS ARE RENDERED
    // =========================================================

    private void HandleStarsRendered(
        int eraIndex
    )
    {
        ClearLines();
        ClearLabels();


        if (constellations == null)
        {
            return;
        }


        foreach (
            ConstellationData data
            in constellations
        )
        {
            if (data == null)
            {
                continue;
            }


            // IMPORTANT:
            // We intentionally do NOT check eraIndex here.
            // The same constellation definitions are used
            // for Ancient, Historical and Modern skies.


            RenderConstellation(
                data
            );


            CreateConstellationLabel(
                data
            );
        }
    }


    // =========================================================
    // RENDER ONE CONSTELLATION
    // =========================================================

    private void RenderConstellation(
        ConstellationData data
    )
    {
        if (data.starNames == null ||
            data.connectionStart == null ||
            data.connectionEnd == null)
        {
            return;
        }


        int connectionCount =
            Mathf.Min(
                data.connectionStart.Length,
                data.connectionEnd.Length
            );


        for (
            int i = 0;
            i < connectionCount;
            i++
        )
        {
            int startIndex =
                data.connectionStart[i];

            int endIndex =
                data.connectionEnd[i];


            if (startIndex < 0 ||
                startIndex >= data.starNames.Length ||
                endIndex < 0 ||
                endIndex >= data.starNames.Length)
            {
                continue;
            }


            string startName =
                StarRenderer.NormalizeStarName(
                    data.starNames[startIndex]
                );


            string endName =
                StarRenderer.NormalizeStarName(
                    data.starNames[endIndex]
                );


            CreateUILine(
                startName,
                endName,
                data
            );
        }
    }


    // =========================================================
    // CREATE LINE
    // =========================================================

    private void CreateUILine(
        string startStarName,
        string endStarName,
        ConstellationData data
    )
    {
        if (!starRenderer.spawnedStars.TryGetValue(
                startStarName,
                out Transform startTransform))
        {
            return;
        }


        if (!starRenderer.spawnedStars.TryGetValue(
                endStarName,
                out Transform endTransform))
        {
            return;
        }


        // Only connect stars currently visible
        if (!startTransform.gameObject.activeSelf ||
            !endTransform.gameObject.activeSelf)
        {
            return;
        }


        RectTransform startRect =
            startTransform as RectTransform;

        RectTransform endRect =
            endTransform as RectTransform;


        if (startRect == null ||
            endRect == null ||
            constellationLayer == null)
        {
            return;
        }


        Vector2 startPosition =
            constellationLayer.InverseTransformPoint(
                startRect.position
            );


        Vector2 endPosition =
            constellationLayer.InverseTransformPoint(
                endRect.position
            );


        CreateSingleLine(
            startPosition,
            endPosition,
            data,
            "Line_" +
            data.constellationName +
            "_" +
            startStarName +
            "_to_" +
            endStarName
        );
    }


    // =========================================================
    // CREATE SINGLE UI LINE
    // =========================================================

    private void CreateSingleLine(
        Vector2 startPosition,
        Vector2 endPosition,
        ConstellationData data,
        string objectName
    )
    {
        Vector2 direction =
            endPosition -
            startPosition;


        float distance =
            direction.magnitude;


        if (distance <= 0.01f)
        {
            return;
        }


        GameObject lineObject =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );


        RectTransform lineRect =
            lineObject.GetComponent<RectTransform>();


        lineRect.SetParent(
            constellationLayer,
            false
        );


        lineRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        lineRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        lineRect.pivot =
            new Vector2(
                0f,
                0.5f
            );


        lineRect.anchoredPosition =
            startPosition;


        lineRect.sizeDelta =
            new Vector2(
                distance,
                data.lineWidth
            );


        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) *
            Mathf.Rad2Deg;


        lineRect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );


        Image lineImage =
            lineObject.GetComponent<Image>();


        lineImage.color =
            data.lineColor;


        lineImage.raycastTarget =
            false;


        lineObjects.Add(
            lineObject
        );
    }


    // =========================================================
    // CREATE CONSTELLATION LABEL
    // =========================================================

    private void CreateConstellationLabel(
        ConstellationData data
    )
    {
        if (labelLayer == null ||
            data.starNames == null)
        {
            return;
        }


        Vector2 totalPosition =
            Vector2.zero;


        int visibleStarCount =
            0;


        foreach (
            string starName
            in data.starNames
        )
        {
            string cleanName =
                StarRenderer.NormalizeStarName(
                    starName
                );


            if (!starRenderer.spawnedStars.TryGetValue(
                    cleanName,
                    out Transform starTransform))
            {
                continue;
            }


            if (!starTransform.gameObject.activeSelf)
            {
                continue;
            }


            RectTransform starRect =
                starTransform as RectTransform;


            if (starRect == null)
            {
                continue;
            }


            Vector2 localPosition =
                labelLayer.InverseTransformPoint(
                    starRect.position
                );


            totalPosition +=
                localPosition;


            visibleStarCount++;
        }


        // Don't show a label if almost the entire
        // constellation is outside the current view.
        if (visibleStarCount < 2)
        {
            return;
        }


        Vector2 centerPosition =
            totalPosition /
            visibleStarCount;


        // Slightly above the pattern
        centerPosition.y +=
            35f;


        // -----------------------------------------------------
        // CREATE LABEL OBJECT
        // -----------------------------------------------------

        GameObject labelObject =
            new GameObject(
                "Label_" +
                data.constellationName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );


        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();


        labelRect.SetParent(
            labelLayer,
            false
        );


        labelRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        labelRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        labelRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        labelRect.anchoredPosition =
            centerPosition;


        labelRect.sizeDelta =
            new Vector2(
                220f,
                45f
            );


        TextMeshProUGUI text =
            labelObject.GetComponent<TextMeshProUGUI>();


        text.text =
            data.constellationName;


        text.fontSize =
            labelFontSize;


        text.color =
            labelColor;


        text.alignment =
            TextAlignmentOptions.Center;


        text.fontStyle =
            FontStyles.Bold;


        text.raycastTarget =
            false;


        labelObjects.Add(
            labelObject
        );
    }


    // =========================================================
    // CLEAR LINES
    // =========================================================

    public void ClearLines()
    {
        foreach (
            GameObject line
            in lineObjects
        )
        {
            if (line != null)
            {
                Destroy(
                    line
                );
            }
        }


        lineObjects.Clear();
    }


    // =========================================================
    // CLEAR LABELS
    // =========================================================

    public void ClearLabels()
    {
        foreach (
            GameObject label
            in labelObjects
        )
        {
            if (label != null)
            {
                Destroy(
                    label
                );
            }
        }


        labelObjects.Clear();
    }
}