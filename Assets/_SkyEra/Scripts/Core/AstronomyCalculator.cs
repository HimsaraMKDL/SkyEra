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
    [Tooltip("Use the computer's current local date and time.")]
    public bool useCurrentTime = true;

    [Tooltip("Manual UTC time used when Use Current Time is disabled.")]
    public DateTime manualUtcTime = new DateTime(2026, 1, 15, 15, 40, 00);


    // ---------------------------------------------------------
    // Calculate Altitude and Azimuth
    // ---------------------------------------------------------

    public Vector2 CalculateAltAz(
        float rightAscension,
        float declination
    )
    {
        DateTime utcTime = GetUtcTime();

        double julianDate = CalculateJulianDate(utcTime);

        double gmst = CalculateGMST(julianDate);

        double lst = gmst + longitude;

        lst = NormalizeDegrees(lst);

        // Right Ascension is supplied in HOURS.
        // Convert RA from hours to degrees.
        double raDegrees = rightAscension * 15.0;

        // Hour Angle
        double hourAngle = lst - raDegrees;

        hourAngle = NormalizeDegrees(hourAngle);

        // Convert to radians
        double haRad = hourAngle * Mathf.Deg2Rad;
        double decRad = declination * Mathf.Deg2Rad;
        double latRad = latitude * Mathf.Deg2Rad;


        // -----------------------------------------------------
        // ALTITUDE
        // -----------------------------------------------------

        double sinAltitude =
            Math.Sin(decRad) * Math.Sin(latRad)
            +
            Math.Cos(decRad) *
            Math.Cos(latRad) *
            Math.Cos(haRad);

        // Protect against floating-point errors
        sinAltitude = Mathf.Clamp(
            (float)sinAltitude,
            -1f,
            1f
        );

        double altitude =
            Math.Asin(sinAltitude) *
            Mathf.Rad2Deg;


        // -----------------------------------------------------
        // AZIMUTH
        // -----------------------------------------------------

        double azimuth =
            Math.Atan2(
                -Math.Sin(haRad),
                Math.Tan(decRad) * Math.Cos(latRad)
                -
                Math.Sin(latRad) * Math.Cos(haRad)
            ) *
            Mathf.Rad2Deg;

        azimuth = NormalizeDegrees(azimuth);


        return new Vector2(
            (float)altitude,
            (float)azimuth
        );
    }


    // ---------------------------------------------------------
    // Get UTC Time
    // ---------------------------------------------------------

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


    // ---------------------------------------------------------
    // Julian Date
    // ---------------------------------------------------------

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


    // ---------------------------------------------------------
    // Greenwich Mean Sidereal Time
    // ---------------------------------------------------------

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


    // ---------------------------------------------------------
    // Normalize angle to 0 - 360 degrees
    // ---------------------------------------------------------

    private double NormalizeDegrees(double angle)
    {
        angle %= 360.0;

        if (angle < 0)
        {
            angle += 360.0;
        }

        return angle;
    }
}