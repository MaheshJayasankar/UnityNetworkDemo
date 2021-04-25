using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityClasses;

public class ChunkNode : MonoBehaviour, INode
{
    #region INodeDefinition
    public HashSet<INode> Linked { get; set; }
    public string Name { get; set; }
    public Guid Id { get; set; }
    public int HeadCount { get; set; }
    public float MaxRadius { get; set; }
    public void SetUp(string name)
    {
        Name = name;
        Linked = new HashSet<INode>();
        Id = Guid.NewGuid();
    }
    public void AddLink(INode node)
    {
        if (!node.Linked.Contains(this))
        {
            node.Linked.Add(this);
            Linked.Add(node);
        }
    }
    public void RemoveLink(INode node)
    {
        if (node.Linked.Contains(this))
        {
            node.Linked.Remove(this);
            Linked.Remove(node);
        }
    }
    /// <summary>
    /// Duplicates the gameObject, adds a new INode script, and returns the script
    /// </summary>
    /// <param name="selfLink"></param>
    /// <returns></returns>
    public INode DuplicateNode(bool selfLink = false)
    {
        GameObject newObject = Instantiate(gameObject);
        ChunkNode newNode = newObject.AddComponent<ChunkNode>();
        newNode.SetUp(name);
        foreach (var link in Linked)
        {
            newNode.AddLink(link);
        }
        if (selfLink)
        {
            newNode.AddLink(this);
        }
        return newNode;
    }
    #endregion

    public ForestGeneratorData forestGeneratorData;
    public VillageGeneratorData villageGeneratorData;
    public int debugForestCount;
    public int debugVillageCount;

    const float chunkSize = 500;
    // Number of areas inside the chunk. Used for collision detection during generation
    public List<IArea> Areas { get; set; }

    void Awake()
    {
        SetUp("MyChunk");
        Areas = new List<IArea>();
        ConstructChunkObjects();
    }

    void ConstructChunkObjects()
    {
        CreateVillageCenters(villageGeneratorData, debugVillageCount);
        CreateForestCenters(forestGeneratorData, debugForestCount);
    }

    private List<Vector3> CreateForestCenters(ForestGeneratorData forestGeneratorData, int forestCount)
    {
        List<Vector3> forestPositions = new List<Vector3>();
        // TODO: Generalise spawnpoint bounds
        float lowerSpawnBoundX = 0 - chunkSize / 2;
        // When the time comes to add vertical scaling, this will have to change based on a separate vertical bound
        float lowerSpawnBoundY = 0;
        float lowerSpawnBoundZ = 0 - chunkSize / 2;

        // TODO: Generalise spawnpoint bounds
        float upperSpawnBoundX = 0 + chunkSize / 2;
        // When the time comes to add vertical scaling, this will have to change based on a separate vertical bound
        float upperSpawnBoundY = 0;
        float upperSpawnBoundZ = 0 + chunkSize / 2;

        Vector3 lowerBounds = new Vector3(lowerSpawnBoundX, lowerSpawnBoundY, lowerSpawnBoundZ);
        Vector3 upperBounds = new Vector3(upperSpawnBoundX, upperSpawnBoundY, upperSpawnBoundZ);

        // Loop over each forest, generate a center for it
        for (int idx = 0; idx < forestCount; idx++)
        {
            float newForestRadius = forestGeneratorData.spawnRadius.RandomSample();
            Vector3 spawnPosition = UtilityFunctions.GetRandomVector3(lowerBounds, upperBounds);

            // Create Temp Area with the above parameters
            var tempArea = new TempArea(AreaType.circle, newForestRadius,spawnPosition);

            // Check for any collisions
            if (IsCircularAreaFree(tempArea))
            {
                string newForestName = $"{forestGeneratorData.forestName}.{idx}";
                // Initialize new ForestCenter
                GameObject newForestObject = new GameObject(name: newForestName);
                var newForestNode = newForestObject.AddComponent<ForestNode>();

                // Find new tree count
                int newTreeCount = forestGeneratorData.treeCount.RandomSample();

                newForestNode.SetUp(newForestName);
                newForestNode.SetUpArea(newForestRadius, spawnPosition);
                newForestNode.SetUpForest(forestGeneratorData.treePrefab, newTreeCount);

                forestPositions.Add(newForestNode.centerPosition);
                Areas.Add(newForestNode);
                AddLink(newForestNode);
            }
        }
        return forestPositions;
    }
    
    private List<Vector3> CreateVillageCenters(VillageGeneratorData villageGeneratorData, int villageCount)
    {
        List<Vector3> villagePositions = new List<Vector3>();
        // TODO: Generalise spawnpoint bounds
        float lowerSpawnBoundX = 0 - chunkSize / 2;
        // When the time comes to add vertical scaling, this will have to change based on a separate vertical bound
        float lowerSpawnBoundY = 0;
        float lowerSpawnBoundZ = 0 - chunkSize / 2;

        // TODO: Generalise spawnpoint bounds
        float upperSpawnBoundX = 0 + chunkSize / 2;
        // When the time comes to add vertical scaling, this will have to change based on a separate vertical bound
        float upperSpawnBoundY = 0;
        float upperSpawnBoundZ = 0 + chunkSize / 2;

        Vector3 lowerBounds = new Vector3(lowerSpawnBoundX, lowerSpawnBoundY, lowerSpawnBoundZ);
        Vector3 upperBounds = new Vector3(upperSpawnBoundX, upperSpawnBoundY, upperSpawnBoundZ);

        // Loop over each village, generate a center for it
        for (int idx = 0; idx < villageCount; idx++)
        {
            float newVillageRadius = villageGeneratorData.spawnRadius.RandomSample();
            Vector3 spawnPosition = UtilityFunctions.GetRandomVector3(lowerBounds, upperBounds);

            // Create Temp Area with the above parameters
            var tempArea = new TempArea(AreaType.circle, newVillageRadius, spawnPosition);

            // Check for any collisions
            if (IsCircularAreaFree(tempArea))
            {
                string newVillageName = $"{villageGeneratorData.villageName}.{idx}";
                // Initialize new Village Center
                GameObject newVillageObject = new GameObject(name: newVillageName);
                var newVillageNode = newVillageObject.AddComponent<VillageNode>();

                // Find new head count
                int newVillagerCount = villageGeneratorData.headCount.RandomSample();

                newVillageNode.SetUp(newVillageName);
                newVillageNode.SetUpArea(newVillageRadius, spawnPosition);

                var newVillageData = new VillageData
                {
                    headCount = newVillagerCount,
                    // huts will be grown as per necessity in the VillageNode script
                    hutCount = 0,
                    hutSpawnRange = villageGeneratorData.percentageHutSpawnRadius * (newVillageRadius / 100),

                    villagersPerHut = villageGeneratorData.villagersPerHut,
                    villagerPrefab = villageGeneratorData.villagerPrefab,
                    elderHutPrefab = villageGeneratorData.elderHutPrefab,
                    hutPrefab = villageGeneratorData.hutPrefab
                };

                newVillageNode.SetUpVillage(newVillageData);

                villagePositions.Add(newVillageNode.centerPosition);
                Areas.Add(newVillageNode);
                AddLink(newVillageNode);
            }
        }
        return villagePositions;
    }

    bool IsCircularAreaFree(IArea targetArea)
    {
        List<bool> collisionList = Areas.Select(area => area.IsColliding(targetArea)).ToList();
        return !collisionList.Contains(true);
    }
    private bool IsSpaceFree(Vector3 position)
    {
        // List that shows true or false entries depending on whether the position is in collision with an area or not
        List<bool> collisionList = (List<bool>)Areas.Select(area => area.IsInside(position));
        return !collisionList.Contains(true);
    }
}
