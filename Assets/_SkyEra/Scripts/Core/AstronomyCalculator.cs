using UnityEngine;

public class AstronomyCalculator : MonoBehaviour
{
    [Header("Observer Location - Sri Lanka")]
    public float latitude = 6.9271f;
    public float longitude = 79.8612f;


    // Calculate Local Sidereal Time
    public float CalculateSiderealTime()
    {
        // Temporary value
        // Full astronomical time calculation will be added next

        return 0f;
    }


    // Convert Right Ascension + Declination
    // into Altitude + Azimuth

    public Vector2 CalculateAltAz(
        float rightAscension,
        float declination
    )
    {
        float altitude = 0f;
        float azimuth = 0f;


        // Calculation will be added here


        return new Vector2(
            altitude,
            azimuth
        );
    }
}