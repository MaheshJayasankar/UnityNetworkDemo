
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityClasses;
using Random = UnityEngine.Random;

/// <summary>
/// Class used for generation of villages
/// </summary>
public class VillageGenerator: MonoBehaviour
{
    public VillageGeneratorData villageGeneratorData;
    public ChunkNode parentChunk;
    private Random.State randomState;

    // TODO: Replace with ResidenceData properties
    // TODO: Hut Dimensions should be a property of the Hut. Find a way to extend HutDimensions to the hutprefab itself

    private Vector3 hutDimensions = new Vector3(8, 12, 8);

    private void Start()
    {
        randomState = Random.state;
    }
    public VillageNode GenerateVillage(Vector3 centerPosition, float maxRadius, string name = "My Village")
    {
        var oldState = Random.state;
        Random.state = randomState;

        // Obtain Village Parameters (Random Rolls)
        var villageData = GenerateVillageData(villageGeneratorData, maxRadius);

        // Instantiate Village
        VillageNode villageNode = CreateVillageNode(centerPosition, villageData, name);

        // Add Huts to Village (Random Placements)
        var hutsTuple = GenerateHuts(villageNode);

        GenerateVillagers(villageNode);

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
            HeadCount = villageGeneratorData.headCount.RandomSample(),
            hutSpawnRange = generatorData.percentageHutSpawnRadius * (maxRadius / 100),
            elderHutSpawnRange = generatorData.percentageElderHutSpawnRadius * (maxRadius / 100),
            maxRadius = maxRadius,
            villagersPerHut = generatorData.villagersPerHut,
            villagerPrefab = generatorData.villagerPrefab,
            elderHutPrefab = generatorData.elderHutPrefab,
            hutPrefab = generatorData.hutPrefab
        };

        return newVillageData;
    }
    /// <summary>
    /// Instantiates a new object and assigns a VillageNode to that object. Sets up that VillageNode with parameters according to villageData.
    /// </summary>
    /// <param name="centerPosition"></param>
    /// <param name="spawnRadius"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private VillageNode CreateVillageNode(Vector3 centerPosition, VillageData villageData, string name = "Default Village")
    {
        var spawnRadius = villageData.maxRadius;
        string newVillageName = name;
        // Initialize new Village Center
        GameObject newVillageObject = new GameObject(name: newVillageName);
        var newVillageNode = newVillageObject.AddComponent<VillageNode>();
        newVillageNode.SetUp(newVillageName);
        newVillageNode.SetUpRegion(spawnRadius, centerPosition);

        // Initializing Village for generation
        newVillageNode.SetUpVillage(villageData);

        return newVillageNode;
    }
    /// <summary>
    /// Traditional Hut Generation Sequence. Start with creating an Elder Hut in the Middle. Then Generate a number of huts in the sides.
    /// </summary>
    /// <param name="villageNode"></param>
    /// <param name="villageData"></param>
    private Tuple<ElderHutNode,HashSet<HutNode>> GenerateHuts(VillageNode villageNode)
    {
        // Find hut count
        int numberOfHutsToBeBuilt = DecideNumberOfHuts(villageNode.VillageData);

        // Village requires 1 Elder Hut
        var elderHutNode = GenerateElderHut(villageNode);
        numberOfHutsToBeBuilt -= 1;

        // Now Village is filled with rest of huts
        // Generate new huts in a loop. Stop if hut generation failed these many times in a row:
        const int maxTriesInARow = 100;
        int triesInARow = 0;

        HashSet<HutNode> hutNodes = new HashSet<HutNode>();

        while (numberOfHutsToBeBuilt > 0)
        {
            bool currentIterationSuccess;

            var newHutNode = TryGenerateHut(villageNode);
            currentIterationSuccess = !(newHutNode is null);

            if (!currentIterationSuccess)
            {
                triesInARow += 1;
                if (triesInARow >= maxTriesInARow)
                {
                    break;
                }
            }
            else
            {
                hutNodes.Add(newHutNode);
                numberOfHutsToBeBuilt -= 1;
                triesInARow = 0;
            }
        }
        return new Tuple<ElderHutNode, HashSet<HutNode>>(elderHutNode, hutNodes);
    }
    /// <summary>
    /// Decide how many huts are required in the traditional generation sequence.
    /// </summary>
    /// <param name="villageData"></param>
    /// <returns></returns>
    private int DecideNumberOfHuts(VillageData villageData)
    {
        int headCount = villageData.HeadCount;
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
    /// <summary>
    /// Finds a suitable location near center of village, creates elder hut along with TiledArea, and returns the ElderHutNode
    /// </summary>
    /// <param name="villageNode"></param>
    /// <param name="villageData"></param>
    /// <returns></returns>
    private ElderHutNode GenerateElderHut(VillageNode villageNode)
    {
        var villageData = villageNode.VillageData;
        // Strategy: In the center, the elder hut is first placed
        Vector3 elderHutPosition = UtilityFunctions.GetRandomVector3(villageNode.transform.position, villageData.elderHutSpawnRange);
        // Orientation of the elder hut will always be towards the center of the village
        Vector3 elderHutForward = villageNode.transform.position - elderHutPosition;
        Quaternion elderHutRotation = Quaternion.FromToRotation(Vector3.forward, elderHutForward);

        string elderHutName = $"{villageNode.Name}.Elder Hut";
        // Instantiation
        var elderHutNode = villageNode.CreateResidence<ElderHutNode>(villageData.elderHutPrefab, elderHutPosition, elderHutRotation, residenceName: elderHutName);

        return elderHutNode;
        
    }
    /// <summary>
    /// Finds a random location and tries to create a hut at that location. Returns null on failure
    /// </summary>
    /// <param name="villageNode"></param>
    /// <returns></returns>
    private HutNode TryGenerateHut(VillageNode villageNode)
    {
        var villageData = villageNode.VillageData;
        // Strategy: A hut is spawned in a circular radius around the center of the village.
        Vector3 hutPosition = UtilityFunctions.GetRandomVector3(villageNode.transform.position, villageData.hutSpawnRange);

        // Ensure no blockage by existing huts
        var dummyObj = new GameObject("Dummy Object");
        dummyObj.transform.position = hutPosition;
        var dummyArea = new TempArea(dummyObj.transform, hutDimensions);

        var collidingArea = villageNode.FindAreaIfColliding(dummyArea);
        HutNode newHutNode;
        // If no colliding area, then can safely add a new hut
        if (collidingArea is null)
        {
            // Add new hut routine
            // Location already determined. Set rotation towards center of village
            Vector3 hutForward = villageNode.transform.position - dummyArea.Center;
            dummyArea.ObjectTransform.forward = hutForward;

            string hutName = $"{villageNode.Name}.Hut";
            // Instantiation
            var hutNode = villageNode.CreateResidence<HutNode>(villageData.hutPrefab, dummyArea.Center, dummyArea.ObjectTransform.rotation, residenceName: hutName);

            newHutNode = hutNode;
        }
        else
        {
            // If snapping process is success, add Hut to this TiledArea
            var colldingTiledArea = collidingArea.TiledArea;
            // Snap to nearest empty space from colliding hut
            var newPosition = colldingTiledArea.SnapToClosestOpenSpace(dummyArea);
            dummyArea.ObjectTransform.position = newPosition;
            // Check if resultant open space is colliding with anything else
            var newCollidingArea = villageNode.FindAreaIfColliding(dummyArea);
            if (newCollidingArea is null)
            {
                // Add new hut routine
                string hutName = $"{villageNode.Name}.Hut";
                // Instantiation
                var hutNode = villageNode.CreateResidence<HutNode>(villageData.hutPrefab, dummyArea.Center, dummyArea.ObjectTransform.rotation, parentTiledArea: colldingTiledArea, residenceName: hutName);

                newHutNode = hutNode;
            }
            else
            {
                newHutNode = null;
            }
        }
        Destroy(dummyObj);
        return newHutNode;
    }
    private List<VillagerNode> GenerateVillagers(VillageNode villageNode)
    {
        var villagerList = new List<VillagerNode>();
        for (int idx = 0; idx < villageNode.VillageData.HeadCount; idx++)
        {
            villagerList.Add(GenerateVillager(villageNode.CenterPosition, villageNode));
        }
        return villagerList;
    }
    private VillagerNode GenerateVillager(Vector3 spawnPoint, VillageNode villageNode)
    {
        // How to generate villager position? Should we place at center, or place according to hut positions?
        return null;
    }
}