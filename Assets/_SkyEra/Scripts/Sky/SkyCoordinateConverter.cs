using UnityEngine;


public class SkyCoordinateConverter : MonoBehaviour
{

    public Vector2 ConvertToScreenPosition(
        float altitude,
        float azimuth,
        RectTransform skyArea
    )
    {

        // Convert azimuth to horizontal position

        float normalizedX =
            azimuth / 360f;


        // Convert altitude to vertical position

        float normalizedY =
            altitude / 90f;



        float x =
            (normalizedX - 0.5f)
            *
            skyArea.rect.width;



        float y =
            normalizedY
            *
            skyArea.rect.height
            -
            (skyArea.rect.height / 2);



        return new Vector2(x, y);
    }

}