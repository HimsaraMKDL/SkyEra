using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstellationRenderer : MonoBehaviour
{
    [Header("References")]
    public ConstellationData constellationData;
    public StarRenderer starRenderer;

    // Constellation lines will be created here
    public RectTransform constellationLayer;


    private readonly List<GameObject> lineObjects =
        new List<GameObject>();


    private void OnEnable()
    {
        if (starRenderer != null)
        {
            starRenderer.StarsRendered +=
                HandleStarsRendered;
        }
    }


    private void Start()
    {
        if (starRenderer == null)
        {
            Debug.LogError(
                "ConstellationRenderer: " +
                "StarRenderer reference is missing."
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


    private void OnDisable()
    {
        if (starRenderer != null)
        {
            starRenderer.StarsRendered -=
                HandleStarsRendered;
        }
    }


    private void HandleStarsRendered(int eraIndex)
    {
        ClearLines();


        if (constellationData == null)
            return;


        if (constellationData.eraIndex != eraIndex)
            return;


        RenderConstellation();
    }


    public void RenderConstellation()
    {
        ClearLines();


        if (constellationData == null)
        {
            Debug.LogWarning(
                "ConstellationRenderer: " +
                "No constellation data assigned."
            );

            return;
        }


        if (starRenderer == null)
        {
            Debug.LogWarning(
                "ConstellationRenderer: " +
                "StarRenderer reference missing."
            );

            return;
        }


        if (constellationLayer == null)
        {
            Debug.LogWarning(
                "ConstellationRenderer: " +
                "Constellation Layer missing."
            );

            return;
        }


        if (constellationData.starNames == null ||
            constellationData.connectionStart == null ||
            constellationData.connectionEnd == null)
        {
            Debug.LogWarning(
                "ConstellationRenderer: " +
                "Constellation data incomplete."
            );

            return;
        }


        int connectionCount =
            Mathf.Min(
                constellationData.connectionStart.Length,
                constellationData.connectionEnd.Length
            );


        for (int i = 0; i < connectionCount; i++)
        {
            int startIndex =
                constellationData.connectionStart[i];

            int endIndex =
                constellationData.connectionEnd[i];


            if (startIndex < 0 ||
                startIndex >= constellationData.starNames.Length ||
                endIndex < 0 ||
                endIndex >= constellationData.starNames.Length)
            {
                Debug.LogWarning(
                    "ConstellationRenderer: " +
                    "Invalid connection index."
                );

                continue;
            }


            string startName =
                StarRenderer.NormalizeStarName(
                    constellationData.starNames[startIndex]
                );


            string endName =
                StarRenderer.NormalizeStarName(
                    constellationData.starNames[endIndex]
                );


            CreateUILine(
                startName,
                endName
            );
        }
    }


    private void CreateUILine(
        string startStarName,
        string endStarName
    )
    {
        if (!starRenderer.spawnedStars.TryGetValue(
                startStarName,
                out Transform startTransform))
        {
            Debug.LogWarning(
                "ConstellationRenderer: Missing star: [" +
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
                "ConstellationRenderer: Missing star: [" +
                endStarName +
                "]"
            );

            return;
        }


        RectTransform startRect =
            startTransform as RectTransform;

        RectTransform endRect =
            endTransform as RectTransform;


        if (startRect == null ||
            endRect == null)
        {
            Debug.LogWarning(
                "ConstellationRenderer: " +
                "Stars require RectTransforms."
            );

            return;
        }


        Vector2 startPosition =
            startRect.anchoredPosition;

        Vector2 endPosition =
            endRect.anchoredPosition;


        Vector2 direction =
            endPosition - startPosition;


        float distance =
            direction.magnitude;


        if (distance <= 0.01f)
            return;


        GameObject lineObject =
            new GameObject(
                "Line_" +
                startStarName +
                "_to_" +
                endStarName,
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
            new Vector2(0.5f, 0.5f);

        lineRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        lineRect.pivot =
            new Vector2(0f, 0.5f);


        lineRect.anchoredPosition =
            startPosition;


        lineRect.sizeDelta =
            new Vector2(
                distance,
                constellationData.lineWidth
            );


        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;


        lineRect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );


        Image lineImage =
            lineObject.GetComponent<Image>();


        lineImage.color =
            constellationData.lineColor;


        lineImage.raycastTarget =
            false;


        lineObjects.Add(
            lineObject
        );
    }


    public void ClearLines()
    {
        foreach (GameObject line in lineObjects)
        {
            if (line != null)
            {
                Destroy(line);
            }
        }


        lineObjects.Clear();
    }
}