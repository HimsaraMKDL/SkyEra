using UnityEngine;

[CreateAssetMenu(
    fileName = "New_Constellation",
    menuName = "SkyEra/Constellation Data"
)]
public class ConstellationData : ScriptableObject
{
    [Header("Basic Information")]
    public string constellationName;


    [Header("Era")]
    [Tooltip(
        "0 = Ancient, 1 = Historical, 2 = Modern"
    )]
    public int eraIndex = 0;


    [Header("Connected Stars")]
    public string[] starNames;


    [Header("Line Connections")]
    public int[] connectionStart;
    public int[] connectionEnd;


    [Header("Display")]
    public Color lineColor = Color.white;

    [Min(1f)]
    public float lineWidth = 3f;
}