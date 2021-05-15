using UnityEngine;
using UtilityClasses;

[CreateAssetMenu]
public class VillageGeneratorData : ScriptableObject
{
    public MinMaxRangeFloat spawnRadius;
    public MinMaxRangeInt headCount;
    public int elderCount;
    public float villagerNohomoRadius;
    public string villageName;
    public GameObject villagerPrefab;
    public GameObject hutPrefab;
    public GameObject elderHutPrefab;
    public MinMaxRangeInt hutCount;
    public MinMaxRangeInt villagersPerHut;
    public MinMaxRangeFloat percentageHutSpawnRadius;
    public MinMaxRangeFloat percentageElderHutSpawnRadius;
}