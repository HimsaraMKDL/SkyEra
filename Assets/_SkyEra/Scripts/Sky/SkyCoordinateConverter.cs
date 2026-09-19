using UnityEngine;

public class SkyCoordinateConverter : MonoBehaviour
{
    [Header("Celestial View Direction")]

    [Tooltip(
        "Right Ascension at the center of the screen, in hours. " +
        "Range 0 - 24."
    )]
    [Range(0f, 24f)]
    public float centerRightAscension = 6f;


    [Tooltip(
        "Declination at the center of the screen, in degrees."
    )]
    [Range(-90f, 90f)]
    public float centerDeclination = 10f;


    [Header("Field Of View")]

    [Tooltip(
        "Horizontal celestial field of view in degrees."
    )]
    [Range(30f, 160f)]
    public float horizontalFieldOfView = 100f;


    // =========================================================
    // RA / DEC -> SCREEN POSITION
    // =========================================================

    public Vector2 ConvertRaDecToScreenPosition(
        float rightAscensionHours,
        float declinationDegrees,
        RectTransform skyArea
    )
    {
        if (skyArea == null)
        {
            return Vector2.zero;
        }


        // -----------------------------------------------------
        // Convert RA hours -> degrees
        // -----------------------------------------------------

        float raDegrees =
            rightAscensionHours * 15f;

        float centerRaDegrees =
            centerRightAscension * 15f;


        // -----------------------------------------------------
        // Convert star RA/Dec to radians
        // -----------------------------------------------------

        float ra =
            raDegrees * Mathf.Deg2Rad;

        float dec =
            declinationDegrees * Mathf.Deg2Rad;


        float centerRa =
            centerRaDegrees * Mathf.Deg2Rad;

        float centerDec =
            centerDeclination * Mathf.Deg2Rad;


        // -----------------------------------------------------
        // Difference in RA
        // Shortest direction around celestial sphere
        // -----------------------------------------------------

        float deltaRaDegrees =
            Mathf.DeltaAngle(
                centerRaDegrees,
                raDegrees
            );

        float deltaRa =
            deltaRaDegrees *
            Mathf.Deg2Rad;


        // -----------------------------------------------------
        // GNOMONIC CELESTIAL PROJECTION
        // -----------------------------------------------------

        float sinDec =
            Mathf.Sin(dec);

        float cosDec =
            Mathf.Cos(dec);

        float sinCenterDec =
            Mathf.Sin(centerDec);

        float cosCenterDec =
            Mathf.Cos(centerDec);


        float cosDistance =
            sinCenterDec * sinDec
            +
            cosCenterDec *
            cosDec *
            Mathf.Cos(deltaRa);


        // Star is on the back side of the current celestial view
        if (cosDistance <= 0.001f)
        {
            return new Vector2(
                100000f,
                100000f
            );
        }


        float projectedX =
            cosDec *
            Mathf.Sin(deltaRa)
            /
            cosDistance;


        float projectedY =
            (
                cosCenterDec * sinDec
                -
                sinCenterDec *
                cosDec *
                Mathf.Cos(deltaRa)
            )
            /
            cosDistance;


        // -----------------------------------------------------
        // FIELD OF VIEW
        // -----------------------------------------------------

        float tanHalfHorizontalFov =
            Mathf.Tan(
                horizontalFieldOfView *
                0.5f *
                Mathf.Deg2Rad
            );


        float aspectRatio =
            skyArea.rect.width /
            skyArea.rect.height;


        float tanHalfVerticalFov =
            tanHalfHorizontalFov /
            aspectRatio;


        float normalizedX =
            projectedX /
            tanHalfHorizontalFov;


        float normalizedY =
            projectedY /
            tanHalfVerticalFov;


        // RA increases eastward.
        // Negative sign gives a natural planetarium-style screen direction.
        float x =
            -normalizedX *
            skyArea.rect.width *
            0.5f;


        float y =
            normalizedY *
            skyArea.rect.height *
            0.5f;


        return new Vector2(
            x,
            y
        );
    }


    // =========================================================
    // IS STAR CURRENTLY INSIDE SCREEN?
    // =========================================================

    public bool IsInsideCurrentView(
        float rightAscensionHours,
        float declinationDegrees,
        RectTransform skyArea
    )
    {
        Vector2 position =
            ConvertRaDecToScreenPosition(
                rightAscensionHours,
                declinationDegrees,
                skyArea
            );


        float halfWidth =
            skyArea.rect.width * 0.5f;

        float halfHeight =
            skyArea.rect.height * 0.5f;


        return
            Mathf.Abs(position.x) <= halfWidth
            &&
            Mathf.Abs(position.y) <= halfHeight;
    }


    // =========================================================
    // 360 DEGREE CELESTIAL ROTATION
    // =========================================================

    public void SetCenterRightAscension(
        float rightAscensionHours
    )
    {
        centerRightAscension =
            Mathf.Repeat(
                rightAscensionHours,
                24f
            );
    }


    public void SetCenterDeclination(
        float declinationDegrees
    )
    {
        centerDeclination =
            Mathf.Clamp(
                declinationDegrees,
                -90f,
                90f
            );
    }
}