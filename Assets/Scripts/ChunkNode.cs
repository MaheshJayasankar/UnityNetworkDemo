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
    public VillageGenerator villageGenerator;
    public int debugForestCount;
    public int debugVillageCount;

    const float chunkSize = 500;
    // Number of areas inside the chunk. Used for collision detection during generation
    public List<IRegion> Regions { get; set; }

    void Awake()
    {
        SetUp("MyChunk");
        Regions = new List<IRegion>();
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
            var tempRegion = new TempRegion(RegionType.circle, newForestRadius,spawnPosition);

            // Check for any collisions
            bool isFreeRegion = IsCircularRegionFree(tempRegion);
            // One extra chance: If region is not free, generate a new region with half the min-max radius range
            if (!isFreeRegion)
            {
                // very hacky way to reduce min-max range to half the possible maximum range, retaining same minimum
                newForestRadius = (forestGeneratorData.spawnRadius + (-forestGeneratorData.spawnRadius.min) * 0.5f + forestGeneratorData.spawnRadius.min).RandomSample();
                spawnPosition = UtilityFunctions.GetRandomVector3(lowerBounds, upperBounds);

                // Create Temp Region with the above parameters
                tempRegion = new TempRegion(RegionType.circle, newForestRadius, spawnPosition);
            }

            // Check for any collisions
            if (IsCircularRegionFree(tempRegion))
            {
                string newForestName = $"{forestGeneratorData.forestName}.{idx}";
                // Initialize new ForestCenter
                GameObject newForestObject = new GameObject(name: newForestName);
                var newForestNode = newForestObject.AddComponent<ForestNode>();

                // Find new tree count
                int newTreeCount = forestGeneratorData.treeCount.RandomSample();

                newForestNode.SetUp(newForestName);
                newForestNode.SetUpRegion(newForestRadius, spawnPosition);
                newForestNode.SetUpForest(forestGeneratorData.treePrefab, newTreeCount);

                forestPositions.Add(newForestNode.CenterPosition);
                Regions.Add(newForestNode);
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
            var tempArea = new TempRegion(RegionType.circle, newVillageRadius, spawnPosition);

            // Check for any collisions
            if (IsCircularRegionFree(tempArea))
            {
                string newVillageName = $"{villageGeneratorData.villageName}.{idx}";
                // Initialize new Village Center
                GameObject newVillageObject = new GameObject(name: newVillageName);
                var newVillageNode = newVillageObject.AddComponent<VillageNode>();

                // Find new head count
                int newVillagerCount = villageGeneratorData.headCount.RandomSample();

                newVillageNode.SetUp(newVillageName);
                newVillageNode.SetUpRegion(newVillageRadius, spawnPosition);

                var newVillageData = new VillageData
                {
                    HeadCount = newVillagerCount,
                    // huts will be grown as per necessity in the VillageNode script
                    hutSpawnRange = villageGeneratorData.percentageHutSpawnRadius * (newVillageRadius / 100),

                    villagersPerHut = villageGeneratorData.villagersPerHut,
                    villagerPrefab = villageGeneratorData.villagerPrefab,
                    elderHutPrefab = villageGeneratorData.elderHutPrefab,
                    hutPrefab = villageGeneratorData.hutPrefab
                };

                newVillageNode.SetUpVillage(newVillageData);
                newVillageNode.BeginGenerationProcess();
                villagePositions.Add(newVillageNode.CenterPosition);
                Regions.Add(newVillageNode);
                AddLink(newVillageNode);
            }
        }
        return villagePositions;
    }

    private bool IsCircularRegionFree(IRegion targetRegion)
    {
        List<bool> collisionList = Regions.Select(region => region.IsColliding(targetRegion)).ToList();
        return !collisionList.Contains(true);
    }
    public IRegion FindRegionIfAny(Vector3 position)
    {
        // If region not found, will return null
        return Regions.Find(region => region.IsInside(position));
    }

    /// <summary>
    /// Create hut and/or place in tiled interface
    /// </summary>
    /// <returns></returns>
    public void DebugCreateHut(Transform targetTransform, IRegion region)
    {
        // Note: Ensure below cast is typesafe
        VillageNode village = region as VillageNode;

        var targetPosition = targetTransform.position;
        var targetRotationEuler = targetTransform.rotation.eulerAngles;
        var targetRotation = Quaternion.Euler(0, targetRotationEuler.y, 0);

        GameObject dummyObj = new GameObject("dummy object");
        dummyObj.transform.position = targetPosition;
        dummyObj.transform.rotation = targetRotation;
        UtilityFunctions.PutObjectOnGround(dummyObj.transform);
        targetPosition = dummyObj.transform.position;

        Debug.Log($"House construction at {Time.fixedTime}");
        var collidingArea = village.FindAreaIfWithinAny(targetPosition);
        Debug.Log($"Is this part of a residence area? {!(collidingArea is null)}");
        if (collidingArea is null)
        {
            // Hut construction can begin
            village.DebugConstructHut(dummyObj.transform);
            Debug.Log($"Hut constructed at {targetPosition}");
        }
        else
        {
            // Snap to nearest available tile
            village.DebugSnapToAndConstructHut(dummyObj.transform, collidingArea);
            Debug.Log($"Snapped to new position and Hut constructed at {dummyObj.transform.position}");
        }
        Destroy(dummyObj);
    }

    public void DebugCreateVillage(Vector3 targetPosition)
    {
        float maxRadius = villageGeneratorData.spawnRadius.RandomSample();

        GameObject dummyObj = new GameObject("dummy object");
        dummyObj.transform.position = targetPosition;
        UtilityFunctions.PutObjectOnGround(dummyObj.transform);
        targetPosition = dummyObj.transform.position;
        Destroy(dummyObj);

        var newVillage = villageGenerator.GenerateVillage(targetPosition, maxRadius, "DebugVillage");
        Regions.Add(newVillage);
    }
}
