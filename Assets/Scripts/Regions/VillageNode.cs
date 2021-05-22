using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityClasses;

public class VillageNode : MonoBehaviour, INode, IRegion
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
    #region IRegionDefinition
    public RegionLabel RegionLabel { get; set; }
    public RegionType RegionType { get; set; }
    public float MaxRadius { get; set; }
    public HashSet<TiledArea> TiledAreas { get; set; }
    public void SetUpRegion(float villageRadius, Vector3 position)
    {
        MaxRadius = villageRadius;
        transform.position = position;
        RegionLabel = RegionLabel.village;
        TiledAreas = new HashSet<TiledArea>();
    }
    public Vector3 CenterPosition
    {
        get
        {
            return transform.position;
        }
    }
    public bool IsInside(Vector3 position)
    {
        bool isInside = (position - CenterPosition).sqrMagnitude <= MaxRadius * MaxRadius;
        return isInside;
    }
    public bool IsColliding(IRegion targetRegion)
    {
        if (targetRegion.RegionType == RegionType.circle)
        {
            float interCenterDistSquared = (targetRegion.CenterPosition - CenterPosition).sqrMagnitude;
            return interCenterDistSquared
                   <= Mathf.Pow(MaxRadius + targetRegion.MaxRadius, 2);
        }
        return false;
    }
    #endregion

    /// <summary>
    /// Elder hut will be spawned up to half this radius close to the center, and at most this radius avay
    /// </summary>
    const float elderHutVaryRadius = 3f;

    // TODO: Replace with ResidenceData properties
    private Vector3 elderHutDimensions = new Vector3(8, 12, 8);
    private Vector3 hutDimensions = new Vector3(8, 12, 8);

    const string defaultVillagerName = "Villager";
    const string defaultElderHutName = "Elder Hut";
    const string defaultHutName = "Hut";
    public VillageData VillageData { get; set; }
    public HashSet<VillagerNode> Villagers { get; set; }
    public HashSet<VillagerNode> UnhousedVillagers { get; set; }
    public HashSet<ElderHutNode> ElderHutNodes { get; set; }
    public HashSet<IResidence> Residences { get; set; }
    public HashSet<IArea> Areas { get; set; }

    /// <summary>
    /// Function should be called upon creation. Sets up internal parameters, and then spawns the villagers and huts
    /// </summary>
    /// <param name="prefab"></param>
    public void SetUpVillage(VillageData villageData)
    {
        Residences = new HashSet<IResidence>();
        Areas = new HashSet<IArea>();

        VillageData = villageData;
        
    }
    public void BeginGenerationProcess()
    {
        UnhousedVillagers = GenerateVillagers();
        GenerateHuts();
    }
    #region HutGeneration
    void GenerateHuts()
    {
        GenerateElderHut();

        // Start generating village huts. Every villager should be received in a house
        while (UnhousedVillagers.Count > 0)
        {
            // Randomly decide to take a number of residents in
            int amountOfResidentsToTake = Mathf.Min(VillageData.villagersPerHut.RandomSample(), UnhousedVillagers.Count);
            GenerateHut(amountOfResidentsToTake);
        }
    }
    /// <summary>
    /// Generate a single Elder Hut, and move a single resident in this elder hut
    /// </summary>
    private void GenerateElderHut()
    {
        // Strategy: In the center, the elder hut is first placed
        Vector3 elderHutPosition = UtilityFunctions.GetRandomVector3(transform.position, new MinMaxRangeFloat(elderHutVaryRadius / 2, elderHutVaryRadius));
        // Orientation of the elder hut will always be towards the center of the village
        Vector3 elderHutForward = transform.position - elderHutPosition;

        Quaternion elderHutRotation = Quaternion.FromToRotation(Vector3.forward, elderHutForward);

        // Instantiation
        var elderHutNode = CreateResidence<ElderHutNode>(VillageData.elderHutPrefab, elderHutPosition, elderHutRotation, residenceName: defaultElderHutName);
        // Allignment
        elderHutNode.transform.forward = elderHutForward;

        // Take a single resident. TODO make the resident the Village Elder. Give a separate attribute to this resident
        var villageElders = new HashSet<VillagerNode>
        {
            UnhousedVillagers.ToList()[UnityEngine.Random.Range(0, UnhousedVillagers.Count - 1)]
        };
        elderHutNode.AddResidents(villageElders.Cast<INode>().ToList());
        elderHutNode.SetUpArea(elderHutNode.transform, elderHutDimensions);
        // Snap elders to near the hut. Also rename them
        foreach (var elder in villageElders)
        {
            // spawn elder close to the hut
            var elderVillagerSpawnDimensions = new Vector2(elderHutDimensions.x, elderHutDimensions.z);
            var elderSpawnPosition = UtilityFunctions.GetRandomBoundaryPoint(elderVillagerSpawnDimensions);
            elder.transform.position = elderHutNode.transform.position + elderHutNode.transform.forward * elderSpawnPosition.x + elderHutNode.transform.right * elderSpawnPosition.y;
            UtilityFunctions.PutObjectOnGround(elder.transform);
            // TODO set up regulated name/gameObject name schemes, so that gameObject name and actual name always match
            elder.gameObject.name = $"{elder.Name}.elder";
        }

        UnhousedVillagers.ExceptWith(villageElders);
    }
    void GenerateHut(int numberOfResidents)
    {
        TiledArea newTiledArea = null;
        // Strategy: A hut is spawned in a circular radius around the center of the village.
        Vector3 hutPosition = UtilityFunctions.GetRandomVector3(transform.position, VillageData.hutSpawnRange);

        // Ensure no blockage by existing huts
        var dummyObj = new GameObject("Dummy Object");
        dummyObj.transform.position = hutPosition;
        var dummyArea = new TempArea(dummyObj.transform, hutDimensions);

        var collidingArea = FindAreaIfColliding(dummyArea);

        bool snappingOccurred = false;
        while (!(collidingArea is null))
        {
            var collidingTiledArea = collidingArea.TiledArea;
            hutPosition = collidingTiledArea.SnapToClosestOpenSpace(dummyArea);
            dummyObj.transform.position = hutPosition;
            collidingArea = FindAreaIfColliding(dummyArea);
            if (collidingArea is null)
            {
                snappingOccurred = true;
                newTiledArea = collidingTiledArea;
                break;
            }
            hutPosition = UtilityFunctions.GetRandomVector3(transform.position, VillageData.hutSpawnRange);
            dummyObj.transform.position = hutPosition;
            collidingArea = FindAreaIfColliding(dummyArea);
        }

        // Checked for collision with existing hut

        // Orientation of the hut will be towards village center
        // But, if adding to an existing TiledArea, use that orientation instead

        if (snappingOccurred)
        {
            dummyObj.transform.rotation = newTiledArea.centerTransform.rotation;
        }
        else
        {
            var hutForward = transform.position - hutPosition;
            // Allignment
            dummyObj.transform.forward = hutForward;
        }

        // Instantiation
        HutNode hutNode = CreateResidence<HutNode>(VillageData.hutPrefab, hutPosition, dummyObj.transform.rotation, parentTiledArea: newTiledArea, residenceName: defaultHutName);
        Destroy(dummyObj);

        // TODO: Replace with sophisticated initialization


        // Pick a random subset of unhoused residents.
        var newResidents = RandomTools.RandomSubset(UnhousedVillagers.ToList(), numberOfResidents);
        hutNode.AddResidents(newResidents.Cast<INode>().ToList());
        hutNode.SetUpArea(hutNode.transform, hutDimensions);
        // Snap villagers to near the hut. Also rename them
        foreach (var villager in newResidents)
        {
            // spawn villager close to the hut
            var villagerSpawnDimensions = new Vector2(hutDimensions.x, hutDimensions.z);
            // spawn villager in the 2-D rectangular boundary around the hut, in the xz plane
            var villagerSpawnPosition = UtilityFunctions.GetRandomBoundaryPoint(villagerSpawnDimensions);
            // The rectangular boundary is oriented using transform.right and transform.forward
            villager.transform.position = hutNode.transform.position + hutNode.transform.forward * villagerSpawnPosition.x + hutNode.transform.right * villagerSpawnPosition.y;
            UtilityFunctions.PutObjectOnGround(villager.transform);
            // TODO set up regulated name/gameObject name schemes, so that gameObject name and actual name always match
            villager.gameObject.name = $"{villager.Name}";
        }

        UnhousedVillagers.ExceptWith(newResidents);
    }
    #endregion
    #region CollisionChecks
    /// <summary>
    /// Function to check whether target position is part of the current residential area
    /// </summary>
    /// <param name="targetPoint"></param>
    /// <returns></returns>
    public IArea FindAreaIfWithinAny(Vector3 targetPoint)
    {
        return Areas.ToList().Find(area => area.IsInside(targetPoint));
    }
    public IArea FindAreaIfColliding(IArea candidateArea)
    {
        foreach (var tiledArea in TiledAreas)
        {
            var collidingArea = tiledArea.FindCollidingAreaIfAny(candidateArea);
            if (!(collidingArea is null))
            {
                return collidingArea;
            }
        }
        return null;
    }
    #endregion
    #region ResidenceFunctions
    public void ConfigureResidence<T>(IEnumerable<VillagerNode> newResidents) where T : MonoBehaviour, INode, IResidence
    {
        UnhousedVillagers.ExceptWith(newResidents);
    }
    /// <summary>
    /// Instantiate Residence Node, with the given parameters. If no TiledArea is specified, a new TiledArea is created and added to the region.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="residencePrefab"></param>
    /// <param name="position"></param>
    /// <param name="parentTiledArea"></param>
    /// <param name="residenceName"></param>
    /// <returns></returns>
    public T CreateResidence<T>(GameObject residencePrefab, Vector3 position, TiledArea parentTiledArea = null, string residenceName = "defaultResidence") where T : MonoBehaviour, INode, IResidence, IArea
    {
        return CreateResidence<T>(residencePrefab, position, Quaternion.identity, parentTiledArea, residenceName);
    }
    /// <summary>
    /// Instantiate Residence Node, with the given parameters. If no TiledArea is specified, a new TiledArea is created and added to the region. Default rotation is identity. See overload including rotation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="residencePrefab"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="parentTiledArea"></param>
    /// <param name="residenceName"></param>
    /// <returns></returns>
    public T CreateResidence<T>(GameObject residencePrefab, Vector3 position, Quaternion rotation, TiledArea parentTiledArea = null, string residenceName = "defaultResidence") where T : MonoBehaviour, INode, IResidence, IArea
    {
        GameObject newResidence = Instantiate(residencePrefab, position, rotation);
        UtilityFunctions.PutObjectOnGround(newResidence.transform);

        T residenceNode = newResidence.AddComponent<T>();
        residenceNode.SetUp(residenceName);
        residenceNode.transform.parent = transform;

        residenceNode.SetUpResidence(this, new List<INode>());
        // TODO: Generalise hut dimensions using data of the hut prefab itself
        residenceNode.SetUpArea(residenceNode.transform, hutDimensions);
        AddLink(residenceNode);
        Residences.Add(residenceNode);
        Areas.Add(residenceNode);
        VillageData.SetHutCount(VillageData.HutCount + 1);
        // If tiled area was not provided, then create a new one
        if (parentTiledArea is null)
        {
            var newTiledArea = new TiledArea(residenceNode);
            TiledAreas.Add(newTiledArea);
        }
        else
        {
            parentTiledArea.AddArea(residenceNode);
        }

        return residenceNode;
    }
    #endregion
    #region VillagerFunctions
    HashSet<VillagerNode> GenerateVillagers()
    {
        Villagers = new HashSet<VillagerNode>();
        // Instantiate the villagers
        for (int i = 0; i < VillageData.HeadCount; i++)
        {
            string newVillagerName = $"{Name}.{defaultVillagerName}.{i}";
            Vector3 newVillagerPos = UtilityFunctions.GetRandomVector3(CenterPosition, MaxRadius);
            VillagerNode newVillagerNode = CreateVillager(newVillagerName, newVillagerPos);
            Villagers.Add(newVillagerNode);
        }
        return Villagers;
    }
    public VillagerNode CreateVillager(string villagerName, Vector3 position)
    {
        GameObject newVillager = Instantiate(VillageData.villagerPrefab, position, Quaternion.identity);
        UtilityFunctions.PutObjectOnGround(newVillager.transform);

        VillagerNode villagerNode = newVillager.AddComponent<VillagerNode>();
        villagerNode.SetUp(villagerName);
        villagerNode.transform.parent = transform;

        Villagers.Add(villagerNode);
        AddLink(villagerNode);

        return villagerNode;
    }
    #endregion
    #region DebugFunctions
    public void DebugConstructHut(Transform dummyObjTransform)
    {
        var hutRotation = dummyObjTransform.rotation;
        var hutPosition = dummyObjTransform.position;

        // Instantiation
        HutNode hutNode = CreateResidence<HutNode>(VillageData.hutPrefab, hutPosition, hutRotation, residenceName: defaultHutName);

        // TODO: Replace with sophisticated initialization

        hutNode.SetUpArea(hutNode.transform, hutDimensions);
    }
    public void DebugSnapToAndConstructHut(Transform dummyObjTransform, IArea collidingArea)
    {
        var tiledArea = collidingArea.TiledArea;
        dummyObjTransform.rotation = tiledArea.centerTransform.rotation;
        var tempArea = new TempArea(dummyObjTransform, hutDimensions);
        var newPosition = tiledArea.SnapToClosestOpenSpace(tempArea);
        dummyObjTransform.position = newPosition;
        var newCollidingArea = FindAreaIfWithinAny(newPosition);
        Debug.Log($"Is snapped area part of a residence area? {!(newCollidingArea is null)}");

        var hutRotation = dummyObjTransform.rotation;
        var hutPosition = dummyObjTransform.position;

        // Instantiation
        HutNode hutNode = CreateResidence<HutNode>(VillageData.hutPrefab, hutPosition, hutRotation, parentTiledArea: tiledArea, residenceName: defaultHutName);

        // TODO: Replace with sophisticated initialization
        hutNode.SetUpArea(hutNode.transform, hutDimensions);

        tiledArea.AddArea(hutNode);
    }
    #endregion
}
/// <summary>
/// Class encapsulates commonly accessed Village Statistics. Each VillageNode has an instance of this class to keep track of its own stats.
/// </summary>
public class VillageData
{
    public int HeadCount { get; set; }
    public int elderCount;
    public int HutCount { get; private set; }

    public MinMaxRangeInt villagersPerHut;
    public MinMaxRangeFloat hutSpawnRange;
    public MinMaxRangeFloat elderHutSpawnRange;
    public float maxRadius;

    public GameObject villagerPrefab;
    public GameObject hutPrefab;
    public GameObject elderHutPrefab;
    public VillageData()
    {
        HutCount = 0;
    }
    public void SetHutCount(int hutCount)
    {
        HutCount = hutCount;
    }
}