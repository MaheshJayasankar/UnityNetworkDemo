using UnityEngine;
using UtilityClasses;

[CreateAssetMenu]
public class VillageGeneratorData : ScriptableObject
{
    public MinMaxRangeFloat spawnRadius;
    public MinMaxRangeInt headCount;
    public float villagerNohomoRadius;
    public string villageName;
    public GameObject villagerPrefab;
    public GameObject hutPrefab;
    public GameObject elderHutPrefab;
    public MinMaxRangeInt villagersPerHut;
    public MinMaxRangeFloat percentageHutSpawnRadius;
}