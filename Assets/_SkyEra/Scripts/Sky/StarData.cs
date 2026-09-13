using UnityEngine;


[System.Serializable]
public class StarData
{
    // -----------------------------
    // Basic Information
    // -----------------------------

    public string starName;


    // -----------------------------
    // Astronomy Coordinates
    // -----------------------------

    // Right Ascension (hours)
    public float rightAscension;


    // Declination (degrees)
    public float declination;


    // -----------------------------
    // Observer Based Position
    // -----------------------------

    // Altitude above horizon
    public float altitude;


    // Direction around observer
    public float azimuth;


    // -----------------------------
    // Visual Properties
    // -----------------------------

    // Brightness value
    // Lower magnitude = brighter star
    public float magnitude;


    // Unity display size
    public float size;
}