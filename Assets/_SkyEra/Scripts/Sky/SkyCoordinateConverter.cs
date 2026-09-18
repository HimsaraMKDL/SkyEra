using UnityEngine;

public class SkyCoordinateConverter : MonoBehaviour
{
    public Vector2 ConvertToScreenPosition(
        float altitude,
        float azimuth,
        RectTransform skyArea
    )
    {
        // -----------------------------------------------------
        // Horizontal position
        //
        // Azimuth:
        // 0°   = North
        // 90°  = East
        // 180° = South
        // 270° = West
        // 360° = North again
        // -----------------------------------------------------

        float normalizedX = azimuth / 360f;


        float x =
            (normalizedX - 0.5f)
            * skyArea.rect.width;


        // -----------------------------------------------------
        // Vertical position
        //
        // 0°  altitude  = horizon / bottom
        // 90° altitude  = zenith / top
        // -----------------------------------------------------

        float normalizedY =
            Mathf.Clamp01(altitude / 90f);


        float y =
            (normalizedY - 0.5f)
            * skyArea.rect.height;


        return new Vector2(x, y);
    }
}