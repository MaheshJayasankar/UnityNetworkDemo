using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityClasses;

/// <summary>
/// Class used for generation of villages
/// </summary>
public class VillageGenerator: MonoBehaviour
{
    public VillageGeneratorData villageGeneratorData;
    public ChunkNode parentChunk;
    private Random.State randomState;

    private void Start()
    {
        randomState = Random.state;
    }
    public VillageNode GenerateVillage(Vector3 centerPosition, float maxRadius, string name = "My Village")
    {
        var oldState = Random.state;
        Random.state = randomState;

        // Instantiation
        var villageNode = CreateVillageNode(centerPosition, maxRadius, name);

        // Obtaining Village Parameters
        var villageData = GenerateVillageData(villageGeneratorData, maxRadius);

        // Initializing Village for generation
        villageNode.SetUpVillage(villageData);

        GenerateHuts(villageNode, villageData);

        Random.state = oldState;
        return villageNode;
    }

    /// <summary>
    /// Generates a random instance of Village Data from the specified generator data.
    /// </summary>
    /// <param name="generatorData"></param>
    /// <returns></returns>
    private VillageData GenerateVillageData(VillageGeneratorData generatorData, float maxRadius)
    {

        var newVillageData = new VillageData
        {
            headCount = villageGeneratorData.headCount.RandomSample(),
            hutCount = 0,
            hutSpawnRange = generatorData.percentageHutSpawnRadius * (maxRadius / 100),
            elderHutSpawnRange = generatorData.percentageElderHutSpawnRadius * (maxRadius / 100),
            villagersPerHut = generatorData.villagersPerHut,
            villagerPrefab = generatorData.villagerPrefab,
            elderHutPrefab = generatorData.elderHutPrefab,
            hutPrefab = generatorData.hutPrefab
        };

        return newVillageData;
    }

    private VillageNode CreateVillageNode(Vector3 centerPosition, float spawnRadius, string name)
    {
        string newVillageName = name;
        // Initialize new Village Center
        GameObject newVillageObject = new GameObject(name: newVillageName);
        var newVillageNode = newVillageObject.AddComponent<VillageNode>();
        newVillageNode.SetUp(newVillageName);
        newVillageNode.SetUpRegion(spawnRadius, centerPosition);
        return newVillageNode;
    }
    /// <summary>
    /// Traditional Hut Generation Sequence. Start with creating an Elder Hut in the Middle. Then Generate a number of huts in the sides.
    /// </summary>
    /// <param name="villageNode"></param>
    /// <param name="villageData"></param>
    private void GenerateHuts(VillageNode villageNode, VillageData villageData)
    {
        // Find hut count
        int numberOfHuts = DecideNumberOfHuts(villageData);

        // Village requires 1 Elder Hut
        GenerateElderHut(villageNode, villageData);
    }

    /// <summary>
    /// Decide how many huts are required in the traditional generation sequence.
    /// </summary>
    /// <param name="villageData"></param>
    /// <returns></returns>
    private int DecideNumberOfHuts(VillageData villageData)
    {
        int headCount = villageData.headCount;
        // At the minimum, if each hut is maximally cramped, what is the number of huts needed?
        int minHutsNeeded = Mathf.CeilToInt((float)headCount / villageData.villagersPerHut.max);
        // If min villagers per hut is 0, then huts can be left empty. Thus there is no maximum capacity configuration
        if (villageData.villagersPerHut.min > 0)
        {
            // If a cap exists (that is, the number of huts cannot exceed a quantity)
            int maxHutsNeeded = Mathf.CeilToInt((float)headCount / villageData.villagersPerHut.min);
            // Return a random value between min and max
            return Random.Range(minHutsNeeded, maxHutsNeeded + 1);
        }
        else
        {
            // No cap on number of huts
            // Return a exponentially distributed random value between min case and 1 for each villager case
            float exponent = Random.Range(0f, 1f);
            return Mathf.RoundToInt(minHutsNeeded * Mathf.Pow(headCount / minHutsNeeded, exponent));
        }
    }
    private void GenerateElderHut(VillageNode villageNode, VillageData villageData)
    {
        // Strategy: In the center, the elder hut is first placed
        Vector3 elderHutPosition = UtilityFunctions.GetRandomVector3(villageNode.transform.position, villageData.elderHutSpawnRange);
        // Orientation of the elder hut will always be towards the center of the village
        Vector3 elderHutForward = villageNode.transform.position - elderHutPosition;
        string elderHutName = $"{villageNode.Name}.Elder Hut";
        // Instantiation
        var elderHutNode = villageNode.CreateResidence<ElderHutNode>(villageData.elderHutPrefab, elderHutName, elderHutPosition);
        // Allignment
        elderHutNode.transform.forward = elderHutForward;

        var newTiledArea = new TiledArea(elderHutNode);
        
    }
    private List<VillagerNode> GenerateVillagers(VillageNode villageNode, VillageData villageData)
    {
        var villagerList = new List<VillagerNode>();
        for (int idx = 0; idx < villageData.headCount; idx++)
        {
            villagerList.Add(GenerateVillager(villageNode, villageData));
        }
        return villagerList;
    }
    private VillagerNode GenerateVillager(VillageNode villageNode, VillageData villageData)
    {
        // How to generate villager position? Should we place at center, or place according to hut positions?
        return null;
    }
}