using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstellationRenderer : MonoBehaviour
{
    [Header("References")]
    public StarRenderer starRenderer;

    [Header("Constellations")]
    public ConstellationData[] constellations;

    [Header("Layer")]
    public RectTransform constellationLayer;


    private readonly List<GameObject> lineObjects =
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
    // STARS RENDERED
    // =========================================================

    private void HandleStarsRendered(
        int eraIndex
    )
    {
        ClearLines();


        if (constellations == null)
            return;


        foreach (
            ConstellationData data
            in constellations
        )
        {
            if (data == null)
                continue;


            


            RenderConstellation(
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


        for (int i = 0;
             i < connectionCount;
             i++)
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
    // CREATE CONSTELLATION LINE
    // =========================================================

    private void CreateUILine(
        string startStarName,
        string endStarName,
        ConstellationData data
    )
    {
        // -----------------------------------------------------
        // Star must exist in database / renderer
        // -----------------------------------------------------

        if (!starRenderer.spawnedStars.TryGetValue(
                startStarName,
                out Transform startTransform))
        {
            Debug.LogWarning(
                "ConstellationRenderer: Star not found in database [" +
                startStarName +
                "]"
            );

            return;
        }


        if (!starRenderer.spawnedStars.TryGetValue(
                endStarName,
                out Transform endTransform))
        {
            Debug.LogWarning(
                "ConstellationRenderer: Star not found in database [" +
                endStarName +
                "]"
            );

            return;
        }


        // -----------------------------------------------------
        // IMPORTANT:
        // If either star is outside current celestial view,
        // don't draw this line yet.
        // -----------------------------------------------------

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
            endRect == null)
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
            return;


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
    // CLEAR
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
                Destroy(line);
            }
        }


        lineObjects.Clear();
    }
}