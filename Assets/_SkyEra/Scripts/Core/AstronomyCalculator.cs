using System;
using UnityEngine;

public class AstronomyCalculator : MonoBehaviour
{
    [Header("Observer Location")]

    [Tooltip("Observer latitude in degrees. North is positive.")]
    public double latitude = 6.9530;

    [Tooltip("Observer longitude in degrees. East is positive.")]
    public double longitude = 79.9297;


    [Header("Time Settings")]

    [Tooltip("Use the computer's current UTC date and time.")]
    public bool useCurrentTime = false;

    [Tooltip("Manual UTC time used when Use Current Time is disabled.")]
    public DateTime manualUtcTime = new DateTime(2026, 1, 15, 15, 40, 00);


    // =========================================================
    // PUBLIC
    // Calculate Altitude and Azimuth
    // =========================================================

    public Vector2 CalculateAltAz(
        float rightAscension,
        float declination
    )
    {
        // -----------------------------------------------------
        // 1. Get observation time
        // -----------------------------------------------------

        DateTime utcTime = GetUtcTime();


        // -----------------------------------------------------
        // 2. Convert UTC time to Julian Date
        // -----------------------------------------------------

        double julianDate = CalculateJulianDate(utcTime);


        // -----------------------------------------------------
        // 3. Calculate Greenwich Mean Sidereal Time
        // -----------------------------------------------------

        double gmst = CalculateGMST(julianDate);


        // -----------------------------------------------------
        // 4. Convert GMST to Local Sidereal Time
        // -----------------------------------------------------

        double lst = gmst + longitude;

        lst = NormalizeDegrees(lst);


        // -----------------------------------------------------
        // 5. Convert Right Ascension
        //
        // RA is stored in HOURS.
        // 1 hour of RA = 15 degrees.
        // -----------------------------------------------------

        double raDegrees = rightAscension * 15.0;


        // -----------------------------------------------------
        // 6. Calculate Hour Angle
        // -----------------------------------------------------

        double hourAngle = lst - raDegrees;

        hourAngle = NormalizeSignedDegrees(hourAngle);


        // -----------------------------------------------------
        // 7. Convert everything to radians
        // -----------------------------------------------------

        double haRad = hourAngle * Mathf.Deg2Rad;
        double decRad = declination * Mathf.Deg2Rad;
        double latRad = latitude * Mathf.Deg2Rad;


        // =====================================================
        // ALTITUDE
        // =====================================================

        double sinAltitude =
            Math.Sin(decRad) * Math.Sin(latRad)
            +
            Math.Cos(decRad)
            * Math.Cos(latRad)
            * Math.Cos(haRad);


        // Protect against floating-point errors
        sinAltitude = Math.Max(-1.0, Math.Min(1.0, sinAltitude));


        double altitude =
            Math.Asin(sinAltitude) *
            Mathf.Rad2Deg;


        // =====================================================
        // AZIMUTH
        // =====================================================

        double azimuth =
            Math.Atan2(
                -Math.Sin(haRad),
                Math.Tan(decRad) * Math.Cos(latRad)
                -
                Math.Sin(latRad) * Math.Cos(haRad)
            )
            * Mathf.Rad2Deg;


        // Convert to 0 - 360 degrees
        azimuth = NormalizeDegrees(azimuth);


        // -----------------------------------------------------
        // Debug information
        // -----------------------------------------------------

        Debug.Log(
            "AstronomyCalculator | " +
            "RA: " + rightAscension.ToString("F4") + "h | " +
            "Dec: " + declination.ToString("F4") + "° | " +
            "Alt: " + altitude.ToString("F2") + "° | " +
            "Az: " + azimuth.ToString("F2") + "°"
        );


        // -----------------------------------------------------
        // Return
        //
        // X = Altitude
        // Y = Azimuth
        // -----------------------------------------------------

        return new Vector2(
            (float)altitude,
            (float)azimuth
        );
    }


    // =========================================================
    // GET UTC TIME
    // =========================================================

    private DateTime GetUtcTime()
    {
        if (useCurrentTime)
        {
            return DateTime.UtcNow;
        }

        return DateTime.SpecifyKind(
            manualUtcTime,
            DateTimeKind.Utc
        );
    }


    // =========================================================
    // JULIAN DATE
    // =========================================================

    private double CalculateJulianDate(DateTime utc)
    {
        int year = utc.Year;
        int month = utc.Month;

        double day =
            utc.Day
            +
            utc.Hour / 24.0
            +
            utc.Minute / 1440.0
            +
            utc.Second / 86400.0
            +
            utc.Millisecond / 86400000.0;


        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }


        int A = year / 100;

        int B =
            2
            -
            A
            +
            (A / 4);


        double julianDate =
            Math.Floor(365.25 * (year + 4716))
            +
            Math.Floor(30.6001 * (month + 1))
            +
            day
            +
            B
            -
            1524.5;


        return julianDate;
    }


    // =========================================================
    // GREENWICH MEAN SIDEREAL TIME
    // =========================================================

    private double CalculateGMST(double julianDate)
    {
        double T =
            (julianDate - 2451545.0)
            / 36525.0;


        double gmst =
            280.46061837
            +
            360.98564736629 *
            (julianDate - 2451545.0)
            +
            0.000387933 * T * T
            -
            (T * T * T) / 38710000.0;


        return NormalizeDegrees(gmst);
    }


    // =========================================================
    // NORMALIZE 0 - 360
    // =========================================================

    private double NormalizeDegrees(double angle)
    {
        angle %= 360.0;

        if (angle < 0)
        {
            angle += 360.0;
        }

        return angle;
    }


    // =========================================================
    // NORMALIZE -180 TO +180
    // =========================================================

    private double NormalizeSignedDegrees(double angle)
    {
        angle %= 360.0;

        if (angle > 180.0)
        {
            angle -= 360.0;
        }

        if (angle < -180.0)
        {
            angle += 360.0;
        }

        return angle;
    }
}