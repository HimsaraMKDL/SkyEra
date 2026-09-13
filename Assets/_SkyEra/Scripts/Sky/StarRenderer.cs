using System;
using System.Collections.Generic;
using UnityEngine;

public class StarRenderer : MonoBehaviour
{
    [Header("References")]
    public StarDatabase starDatabase;
    public GameObject starPrefab;

    // Used for sky dimensions / coordinate conversion
    public RectTransform skyViewArea;

    // Stars are now spawned here
    public RectTransform starLayer;

    public SkyCoordinateConverter coordinateConverter;


    public Dictionary<string, Transform> spawnedStars =
        new Dictionary<string, Transform>(
            StringComparer.OrdinalIgnoreCase
        );


    public event Action<int> StarsRendered;


    private int selectedEra = 0;

    public int CurrentEraIndex => selectedEra;


    public void ChangeEra(int eraIndex)
    {
        if (starDatabase == null ||
            starDatabase.datasets == null ||
            starDatabase.datasets.Length == 0)
        {
            Debug.LogError(
                "StarRenderer: Star Database is missing or empty."
            );

            return;
        }


        selectedEra = Mathf.Clamp(
            eraIndex,
            0,
            starDatabase.datasets.Length - 1
        );


        RenderStars();
    }


    public void RenderStars()
    {
        ClearStars();


        if (starDatabase == null)
        {
            Debug.LogError(
                "StarRenderer: Star Database Missing."
            );

            return;
        }


        if (starPrefab == null)
        {
            Debug.LogError(
                "StarRenderer: Star Prefab Missing."
            );

            return;
        }


        if (skyViewArea == null)
        {
            Debug.LogError(
                "StarRenderer: Sky View Area Missing."
            );

            return;
        }


        if (starLayer == null)
        {
            Debug.LogError(
                "StarRenderer: Star Layer Missing."
            );

            return;
        }


        if (coordinateConverter == null)
        {
            Debug.LogError(
                "StarRenderer: Coordinate Converter Missing."
            );

            return;
        }


        if (selectedEra < 0 ||
            selectedEra >= starDatabase.datasets.Length)
        {
            Debug.LogError(
                "StarRenderer: Invalid era index: " +
                selectedEra
            );

            return;
        }


        StarDataset currentDataset =
            starDatabase.datasets[selectedEra];


        if (currentDataset == null ||
            currentDataset.stars == null)
        {
            Debug.LogWarning(
                "StarRenderer: No stars found for era " +
                selectedEra
            );

            StarsRendered?.Invoke(selectedEra);

            return;
        }


        foreach (StarData star in currentDataset.stars)
        {
            CreateStar(star);
        }


        Debug.Log(
            "StarRenderer: Rendered " +
            spawnedStars.Count +
            " stars for era " +
            selectedEra
        );


        StarsRendered?.Invoke(selectedEra);
    }


    private void CreateStar(StarData data)
    {
        if (data == null)
            return;


        string cleanStarName =
            NormalizeStarName(data.starName);


        if (string.IsNullOrEmpty(cleanStarName))
        {
            Debug.LogWarning(
                "StarRenderer: Found star with empty name."
            );

            return;
        }


        GameObject starObject =
            Instantiate(
                starPrefab,
                starLayer
            );


        starObject.name =
            "Star_" + cleanStarName;


        RectTransform rect =
            starObject.GetComponent<RectTransform>();


        if (rect == null)
        {
            Debug.LogError(
                "StarRenderer: PF_Star requires RectTransform."
            );

            Destroy(starObject);

            return;
        }


        Vector2 position =
            coordinateConverter.ConvertToScreenPosition(
                data.altitude,
                data.azimuth,
                skyViewArea
            );


        rect.anchoredPosition =
            position;


        StarView view =
            starObject.GetComponent<StarView>();


        if (view != null)
        {
            view.Setup(data);
        }


        if (!spawnedStars.ContainsKey(cleanStarName))
        {
            spawnedStars.Add(
                cleanStarName,
                starObject.transform
            );
        }
        else
        {
            Debug.LogWarning(
                "StarRenderer: Duplicate star name: [" +
                cleanStarName +
                "]"
            );
        }
    }


    public static string NormalizeStarName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim();
    }


    private void ClearStars()
    {
        foreach (
            KeyValuePair<string, Transform> entry
            in spawnedStars
        )
        {
            if (entry.Value != null)
            {
                Destroy(
                    entry.Value.gameObject
                );
            }
        }


        spawnedStars.Clear();
    }
}