using UnityEngine;
using UtilityClasses;

[CreateAssetMenu]
public class VillageGeneratorData : ScriptableObject
{
    public MinMaxRangeFloat spawnRadius;
    public MinMaxRangeInt headCount;
    public float villagerNohomoRadius;
    public string villageName;
}