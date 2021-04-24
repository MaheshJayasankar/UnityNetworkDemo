using UnityEngine;
using UtilityClasses;

[CreateAssetMenu]
public class ForestGeneratorData : ScriptableObject
{
    public MinMaxRangeFloat spawnRadius;
    public MinMaxRangeInt treeCount;
    public float treeBufferRadius;
    public string forestName;
    public GameObject treePrefab;
}
