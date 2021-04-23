using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityClasses;

public class EntityRelationsScript : MonoBehaviour
{
    private const string defaultVillageName = "My Village";
    private const string defaultForestName = "My Forest";

    /// <summary>
    /// Horizontal chunk size, total square side length.
    /// </summary>
    const float chunkSize = 500;

    [SerializeField] int seed;
    Random.State randState;
    [SerializeField] int updateRate;
    [SerializeField] int initialVillageHeadCount;
    [SerializeField] float spawnRadius;
    [SerializeField] float villageRadius;
    [SerializeField] float forestRadius;
    [SerializeField] float totalForestCount;

    [SerializeField] int minTreesInForest;
    [SerializeField] int maxTreesInForest;

    VillageNode mainVillage;
    List<VillagerNode> villagers;

    [SerializeField] GameObject villagePrefab;
    [SerializeField] GameObject villagerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(seed);
        randState = Random.state;
        _ = StartCoroutine(methodName: nameof(LogicUpdate));
        mainVillage = RandomCreateVillage();
        villagers = new List<VillagerNode>();
        for (int idx = 0; idx <= initialVillageHeadCount; idx++)
        {
            villagers.Add(RandomCreateVillager(mainVillage.transform.position, spawnRadius));
        }
    }

    VillageNode InstantiateVillage(Vector3 position)
    {
        GameObject newVillage = Instantiate(villagePrefab, position, Quaternion.identity);
        VillageNode villageNode = newVillage.AddComponent<VillageNode>();
        villageNode.SetUp(defaultVillageName, villageRadius);
        return villageNode;
    }

    VillagerNode InstantiateVillager(Vector3 position)
    {
        GameObject newVillager = Instantiate(villagerPrefab, position, Quaternion.identity);
        VillagerNode villagerNode = newVillager.AddComponent<VillagerNode>();
        villagerNode.SetUp(defaultVillageName);
        return villagerNode;
    }

    ForestNode InstantiateForest(Vector3 position, float radius, int treeCount)
    {
        GameObject newForest = Instantiate(villagePrefab, position, Quaternion.identity);
        ForestNode villageNode = newForest.AddComponent<ForestNode>();
        villageNode.SetUp(defaultForestName, radius, treeCount);
        return villageNode;
    }

    VillageNode RandomCreateVillage()
    {
        Vector3 lowerBound = new Vector3(-20, 0, -20);
        Vector3 upperBound = new Vector3(20, 0, 20);
        return RandomCreateVillage(lowerBound, upperBound);
    }
    
    /// <summary>
    /// Generate a list of Vector3 positions indicating the forest node centers in the current chunk block. 
    /// Generalise to the current chunk position.
    /// </summary>
    /// <param name="forestCount"></param>
    /// <param name="minRadius"></param>
    /// <param name="maxRadius"></param>
    /// <returns></returns>
    List<Vector3> CreateForestCenters(int forestCount, float minRadius, float maxRadius)

    {
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
        for (int i = 0; i < forestCount; i++)
        {
            float newForestRadius = Random.Range(minRadius, maxRadius);
            UtilityFunctions.GetRandomVector3(lowerBounds, upperBounds);
        }
        return null;
    }


    VillageNode RandomCreateVillage(Vector3 lowerBound, Vector3 upperBound)
    {
        float xPosition = Random.Range(lowerBound.x, upperBound.x);
        float yPosition = Random.Range(lowerBound.y, upperBound.y);
        float zPosition = Random.Range(lowerBound.z, upperBound.z);
        Vector3 targetPosition = new Vector3(xPosition, yPosition, zPosition);
        return InstantiateVillage(targetPosition);
    }

    VillagerNode RandomCreateVillager(Vector3 approximatePosition, float radius)
    {
        float angle = Random.Range(0f, 359f);
        float magnitude = Random.Range(0, radius);
        Vector3 offsetFromCenter = Quaternion.Euler(0, angle, 0) * (Vector3.forward * magnitude);
        Vector3 targetPosition = approximatePosition + offsetFromCenter;
        VillagerNode villager = InstantiateVillager(targetPosition);
        PutObjectOnGround(villager.transform);
        return villager;
    }

    void PutObjectOnGround(Transform objectTransform)
    {
        LayerMask mask = 0;
        mask |= (1 << 3);
        float verticalOffset = 2f;

        // raycast to find the y-position of the masked collider at the transforms x/z
        // note that the ray starts at 100 units
        Ray ray = new Ray(objectTransform.position + Vector3.up * 100, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask))
        {
            if (hit.collider != null)
            {
                // this is where the gameobject is actually put on the ground
                objectTransform.position = new Vector3(objectTransform.position.x, hit.point.y + verticalOffset, objectTransform.position.z);
            }
        }
    }

    IEnumerator LogicUpdate()
    {


        yield return new WaitForSeconds(updateRate);
    }

}
