using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityClasses;

public class VillageNode : MonoBehaviour, INode, IArea
{
    #region INodeDefinition
    public HashSet<INode> Linked { get; set; }
    public string Name { get; set; }
    public Guid Id { get; set; }
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
    public INode DuplicateNode(bool selfLink = false)
    {
        GameObject newObject = Instantiate(gameObject);
        VillageNode newNode = newObject.AddComponent<VillageNode>();
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
    #region IAreaDefinition
    public AreaType AreaType { get; set; }
    public float MaxRadius { get; set; }
    public void SetUpArea(float villageRadius, Vector3 position)
    {
        MaxRadius = villageRadius;
        transform.position = position;
    }
    public Vector3 centerPosition
    {
        get
        {
            return transform.position;
        }
    }
    public bool IsInside(Vector3 position)
    {
        bool isInside = (position - centerPosition).sqrMagnitude <= MaxRadius * MaxRadius;
        return isInside;
    }
    public bool IsColliding(IArea targetArea)
    {
        if (targetArea.AreaType == AreaType.circle)
        {
            float interCenterDistSquared = (targetArea.centerPosition - centerPosition).sqrMagnitude;
            return interCenterDistSquared
                   <= Mathf.Pow(MaxRadius + targetArea.MaxRadius, 2);
        }
        return false;
    }
    #endregion

    /// <summary>
    /// Elder hut will be spawned up to half this radius close to the center, and at most this radius avay
    /// </summary>
    const float elderHutVaryRadius = 3f;
    /// <summary>
    /// TODO: Replace with ResidenceData properties
    /// </summary>
    const float elderHutResidenceRadius = 6f;
    const string defaultVillagerName = "Villager";
    const string defaultElderHutName = "Elder Hut";
    public VillageData VillageData { get; set; }
    
    public List<VillagerNode> Villagers { get; set; }
    public List<VillagerNode> UnhousedVillages { get; set; }

    /// <summary>
    /// Function should be called upon creation. Sets up internal parameters, and then spawns the villagers
    /// </summary>
    /// <param name="prefab"></param>
    public void SetUpVillage(VillageData villageData)
    {
        VillageData = villageData;
        UnhousedVillages = GenerateVillagers();
        GenerateHuts();
    }
    void GenerateHuts()
    {
        // Strategy: In the center, the elder hut is first placed
        Vector3 elderHutPosition = UtilityFunctions.GetRandomVector3(transform.position, new MinMaxRangeFloat(elderHutVaryRadius / 2, elderHutVaryRadius));
        // Orientation of the elder hut will always be towards the center of the village
        Vector3 elderHutForward = transform.position - elderHutPosition;

        // Instantiation
        var elderHutNode = CreateResidence<ElderHutNode>(VillageData.elderHutPrefab, defaultElderHutName, elderHutPosition);
        // Allignment
        elderHutNode.transform.forward = elderHutForward;

        // Take a single resident. TODO make the resident the Village Elder
        var villageElders = new List<VillagerNode>
        {
            UnhousedVillages[UnityEngine.Random.Range(0, UnhousedVillages.Count - 1)]
        };
        elderHutNode.SetUpResidence(villageElders.Cast<INode>().ToList(), elderHutResidenceRadius);
        // Snap elders to near the hut. Also rename them
        foreach (var elder in villageElders)
        {
            elder.transform.position = UtilityFunctions.GetRandomVector3(elderHutNode.transform.position, new MinMaxRangeFloat(elderHutResidenceRadius, elderHutResidenceRadius));
            // TODO set up regulated name/gameObject name schemes, so that gameObject name and actual name always match
            elder.gameObject.name = $"{elder.Name}.elder";
        }

        UnhousedVillages = UnhousedVillages.Except(villageElders).ToList();

        // Start generating village huts. Every villager should be received in a house
        /* int unHousedResidents = UnhousedVillages.Count;
        while (unHousedResidents > 0)
        {
            int amountOfResidentsToTake = VillageData.villagersPerHut.RandomSample();

        }*/
    }
    List<VillagerNode> GenerateVillagers()
    {
        Villagers = new List<VillagerNode>();
        // Instantiate the villagers
        for (int i = 0; i < VillageData.headCount; i++)
        {
            string newVillagerName = $"{Name}.{defaultVillagerName}.{i}";
            Vector3 newVillagerPos = UtilityFunctions.GetRandomVector3(centerPosition, MaxRadius);
            VillagerNode newVillagerNode = CreateVillager(newVillagerName, newVillagerPos);
            Villagers.Add(newVillagerNode);
            AddLink(newVillagerNode);
        }
        return Villagers;
    }

    T CreateResidence<T>(GameObject residencePrefab, string residenceName, Vector3 position) where T : MonoBehaviour, INode, IResidence
    {
        GameObject newResidence = Instantiate(residencePrefab, position, Quaternion.identity);
        UtilityFunctions.PutObjectOnGround(newResidence.transform);

        T residenceNode = newResidence.AddComponent<T>();
        residenceNode.SetUp(residenceName);
        residenceNode.transform.parent = transform;
        return residenceNode;
    }

    VillagerNode CreateVillager(string villagerName, Vector3 position)
    {
        GameObject newVillager = Instantiate(VillageData.villagerPrefab, position, Quaternion.identity);
        UtilityFunctions.PutObjectOnGround(newVillager.transform);

        VillagerNode villagerNode = newVillager.AddComponent<VillagerNode>();
        villagerNode.SetUp(villagerName);
        villagerNode.transform.parent = transform;
        return villagerNode;
    }
}

public class VillageData
{
    public int headCount;
    public int hutCount;
    public MinMaxRangeInt villagersPerHut;
    public MinMaxRangeFloat hutSpawnRange;

    public GameObject villagerPrefab;
    public GameObject hutPrefab;
    public GameObject elderHutPrefab;

}