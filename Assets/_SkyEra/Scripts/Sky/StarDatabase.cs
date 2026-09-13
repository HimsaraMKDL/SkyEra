using UnityEngine;


[CreateAssetMenu(
    fileName = "StarDatabase",
    menuName = "SkyEra/Star Database"
)]
public class StarDatabase : ScriptableObject
{
    public StarDataset[] datasets;
}