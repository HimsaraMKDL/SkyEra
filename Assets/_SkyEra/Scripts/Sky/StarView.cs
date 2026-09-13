using UnityEngine;
using UnityEngine.UI;

public class StarView : MonoBehaviour
{
    public Image coreImage;

    public void Setup(StarData data)
    {
        // -----------------------------
        // Star Position Setup 
        // -----------------------------
        // (Star position set karana code eka methana thiyenna oni)
        // eg: transform.position = data.position;

        // -----------------------------
        // Magnitude Based Star Size
        // -----------------------------
        float size = Mathf.Lerp(
            30f,
            8f,
            Mathf.InverseLerp(
                -1f,
                5f,
                data.magnitude
            )
        );
        transform.localScale = Vector3.one * (size / 20f);

        // -----------------------------
        // Brightness calculation
        // -----------------------------
        float brightness = Mathf.Clamp(
            1f - (data.magnitude / 8f),
            0.2f,
            1f
        );

        Color c = coreImage.color;
        c.a = brightness;
        coreImage.color = c;
    }
}