using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarRenderer : MonoBehaviour
{
    [Header("References")]
    public StarDatabase starDatabase;
    public GameObject starPrefab;

    public RectTransform skyViewArea;
    public RectTransform starLayer;

    public SkyCoordinateConverter coordinateConverter;

    // Kept for compatibility / later use
    public AstronomyCalculator astronomyCalculator;


    [Header("Era / Epoch Settings")]

    [Tooltip("Modern Sri Lanka dataset containing the master J2000 star list.")]
    public int referenceDatasetIndex = 2;

    [Tooltip("250 BCE in astronomical year numbering = -249.")]
    public float ancientYear = -249f;

    public float historicalYear = 1700f;

    public float modernYear = 2026f;


    public Dictionary<string, Transform> spawnedStars =
        new Dictionary<string, Transform>(
            StringComparer.OrdinalIgnoreCase
        );


    public event Action<int> StarsRendered;


    private int selectedEra = 0;


    public int CurrentEraIndex =>
        selectedEra;


    // =========================================================
    // CHANGE ERA
    // =========================================================

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


        selectedEra =
            Mathf.Clamp(
                eraIndex,
                0,
                2
            );


        if (skyViewArea != null &&
            skyViewArea.gameObject.activeInHierarchy)
        {
            RenderStars();
        }
    }


    // =========================================================
    // RENDER
    // =========================================================

    public void RenderStars()
    {
        ClearStars();


        if (starDatabase == null ||
            starPrefab == null ||
            skyViewArea == null ||
            starLayer == null ||
            coordinateConverter == null)
        {
            Debug.LogError(
                "StarRenderer: One or more references are missing."
            );

            return;
        }


        Canvas.ForceUpdateCanvases();


        RectTransform parentRect =
            skyViewArea.parent as RectTransform;


        if (parentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                parentRect
            );
        }


        LayoutRebuilder.ForceRebuildLayoutImmediate(
            skyViewArea
        );


        Canvas.ForceUpdateCanvases();


        float width =
            skyViewArea.rect.width;

        float height =
            skyViewArea.rect.height;


        if (width <= 1f ||
            height <= 1f)
        {
            Debug.LogError(
                "StarRenderer: INVALID SkyViewArea size. Width = " +
                width +
                ", Height = " +
                height
            );

            return;
        }


        // -----------------------------------------------------
        // IMPORTANT:
        // Always use Modern dataset as master star catalogue.
        // Era differences are calculated mathematically.
        // -----------------------------------------------------

        int safeReferenceIndex =
            Mathf.Clamp(
                referenceDatasetIndex,
                0,
                starDatabase.datasets.Length - 1
            );


        StarDataset referenceDataset =
            starDatabase.datasets[
                safeReferenceIndex
            ];


        if (referenceDataset == null ||
            referenceDataset.stars == null)
        {
            Debug.LogError(
                "StarRenderer: Reference star dataset is missing."
            );

            return;
        }


        float targetYear =
            GetTargetYear();


        int visibleCount = 0;


        foreach (StarData star in referenceDataset.stars)
        {
            if (CreateStar(
                    star,
                    targetYear
                ))
            {
                visibleCount++;
            }
        }


        Debug.Log(
            "StarRenderer: Era " +
            selectedEra +
            " | Year = " +
            targetYear +
            " | Registered = " +
            spawnedStars.Count +
            " | Visible = " +
            visibleCount
        );


        StarsRendered?.Invoke(
            selectedEra
        );
    }


    // =========================================================
    // REFRESH
    // =========================================================

    public void RefreshCurrentView()
    {
        if (skyViewArea == null ||
            !skyViewArea.gameObject.activeInHierarchy)
        {
            return;
        }


        RenderStars();
    }


    // =========================================================
    // CREATE STAR
    // =========================================================

    private bool CreateStar(
        StarData data,
        float targetYear
    )
    {
        if (data == null)
        {
            return false;
        }


        string cleanStarName =
            NormalizeStarName(
                data.starName
            );


        if (string.IsNullOrEmpty(cleanStarName))
        {
            return false;
        }


        // -----------------------------------------------------
        // PRECESS J2000 RA / DEC TO SELECTED ERA
        // -----------------------------------------------------

        Vector2 eraRaDec =
            PrecessJ2000ToYear(
                data.rightAscension,
                data.declination,
                targetYear
            );


        float eraRA =
            eraRaDec.x;

        float eraDec =
            eraRaDec.y;


        // -----------------------------------------------------
        // CELESTIAL POSITION
        // -----------------------------------------------------

        Vector2 screenPosition =
            coordinateConverter
                .ConvertRaDecToScreenPosition(
                    eraRA,
                    eraDec,
                    skyViewArea
                );


        // -----------------------------------------------------
        // CREATE STAR
        // -----------------------------------------------------

        GameObject starObject =
            Instantiate(
                starPrefab,
                starLayer
            );


        starObject.name =
            "Star_" +
            cleanStarName;


        RectTransform rect =
            starObject.GetComponent<RectTransform>();


        if (rect == null)
        {
            Destroy(
                starObject
            );

            return false;
        }


        rect.anchoredPosition =
            screenPosition;


        StarView view =
            starObject.GetComponent<StarView>();


        if (view != null)
        {
            view.Setup(
                data
            );
        }


        // -----------------------------------------------------
        // REGISTER
        // -----------------------------------------------------

        if (!spawnedStars.ContainsKey(cleanStarName))
        {
            spawnedStars.Add(
                cleanStarName,
                starObject.transform
            );
        }


        // -----------------------------------------------------
        // VISIBILITY
        // -----------------------------------------------------

        bool visible =
            coordinateConverter.IsInsideCurrentView(
                eraRA,
                eraDec,
                skyViewArea
            );


        starObject.SetActive(
            visible
        );


        return visible;
    }


    // =========================================================
    // ERA YEAR
    // =========================================================

    private float GetTargetYear()
    {
        switch (selectedEra)
        {
            case 0:
                return ancientYear;

            case 1:
                return historicalYear;

            default:
                return modernYear;
        }
    }


    // =========================================================
    // PRECESS J2000 RA / DEC TO TARGET YEAR
    //
    // Input:
    // RA  = hours
    // Dec = degrees
    //
    // Output:
    // X = RA hours
    // Y = Dec degrees
    // =========================================================

    private Vector2 PrecessJ2000ToYear(
        float rightAscensionHours,
        float declinationDegrees,
        float targetYear
    )
    {
        // Julian centuries from J2000
        double t =
            (targetYear - 2000.0)
            / 100.0;


        // Precession angles in arcseconds
        double zetaArcsec =
            2306.2181 * t
            +
            0.30188 * t * t
            +
            0.017998 * t * t * t;


        double zArcsec =
            2306.2181 * t
            +
            1.09468 * t * t
            +
            0.018203 * t * t * t;


        double thetaArcsec =
            2004.3109 * t
            -
            0.42665 * t * t
            -
            0.041833 * t * t * t;


        double zeta =
            zetaArcsec /
            3600.0 *
            Mathf.Deg2Rad;


        double z =
            zArcsec /
            3600.0 *
            Mathf.Deg2Rad;


        double theta =
            thetaArcsec /
            3600.0 *
            Mathf.Deg2Rad;


        double ra =
            rightAscensionHours *
            15.0 *
            Mathf.Deg2Rad;


        double dec =
            declinationDegrees *
            Mathf.Deg2Rad;


        double A =
            Math.Cos(dec) *
            Math.Sin(
                ra + zeta
            );


        double B =
            Math.Cos(theta) *
            Math.Cos(dec) *
            Math.Cos(
                ra + zeta
            )
            -
            Math.Sin(theta) *
            Math.Sin(dec);


        double C =
            Math.Sin(theta) *
            Math.Cos(dec) *
            Math.Cos(
                ra + zeta
            )
            +
            Math.Cos(theta) *
            Math.Sin(dec);


        C =
            Math.Max(
                -1.0,
                Math.Min(
                    1.0,
                    C
                )
            );


        double newRA =
            Math.Atan2(
                A,
                B
            )
            +
            z;


        double newDec =
            Math.Asin(
                C
            );


        double newRADegrees =
            newRA *
            Mathf.Rad2Deg;


        newRADegrees %=
            360.0;


        if (newRADegrees < 0)
        {
            newRADegrees +=
                360.0;
        }


        float finalRAHours =
            (float)(
                newRADegrees /
                15.0
            );


        float finalDecDegrees =
            (float)(
                newDec *
                Mathf.Rad2Deg
            );


        return new Vector2(
            finalRAHours,
            finalDecDegrees
        );
    }


    // =========================================================
    // NAME
    // =========================================================

    public static string NormalizeStarName(
        string value
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }


        return value.Trim();
    }


    // =========================================================
    // CLEAR
    // =========================================================

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