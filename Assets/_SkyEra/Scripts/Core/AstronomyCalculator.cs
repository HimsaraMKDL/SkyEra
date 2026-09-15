using UnityEngine;

public class AstronomyCalculator : MonoBehaviour
{

    // Observer location (Sri Lanka)
    public float latitude = 7.8731f;
    public float longitude = 80.7718f;


    // Calculate Altitude and Azimuth
    public Vector2 CalculateAltAz(
        float rightAscension,
        float declination
    )
    {

        // Temporary Local Sidereal Time
        float lst = rightAscension + 6.0f;


        // Hour angle
        float hourAngle = lst - rightAscension;


        // Convert degrees
        float haRad = hourAngle * Mathf.Deg2Rad;
        float decRad = declination * Mathf.Deg2Rad;
        float latRad = latitude * Mathf.Deg2Rad;



        // Altitude calculation

        float altitude =
            Mathf.Asin(
                Mathf.Sin(decRad) * Mathf.Sin(latRad)
                +
                Mathf.Cos(decRad) * Mathf.Cos(latRad) * Mathf.Cos(haRad)
            );


        altitude *= Mathf.Rad2Deg;



        // Azimuth calculation

        float azimuth =
            Mathf.Atan2(
                -Mathf.Sin(haRad),
                Mathf.Tan(decRad) * Mathf.Cos(latRad)
                -
                Mathf.Sin(latRad) * Mathf.Cos(haRad)
            );


        azimuth *= Mathf.Rad2Deg;


        if (azimuth < 0)
            azimuth += 360;



        return new Vector2(
            altitude,
            azimuth
        );

    }

}