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
        villageNode.SetUp(defaultVillageName);
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
        ForestNode forestNode = newForest.AddComponent<ForestNode>();
        forestNode.SetUp(defaultForestName);
        return forestNode;
    }

    VillageNode RandomCreateVillage()
    {
        Vector3 lowerBound = new Vector3(-20, 0, -20);
        Vector3 upperBound = new Vector3(20, 0, 20);
        return RandomCreateVillage(lowerBound, upperBound);
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
