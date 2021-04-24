using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityClasses;

public class ForestNode : MonoBehaviour, INode, IArea
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
    public ForestNode(string name)
    {
        Name = name;
        Linked = new HashSet<INode>();
        Id = Guid.NewGuid();
    }

    public ForestNode(string name, HashSet<INode> linked)
    {
        Name = name;
        Linked = linked;
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
        ForestNode newNode = newObject.AddComponent<ForestNode>();
        newNode.SetUp(name);
        foreach (INode link in Linked)
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
    public void SetUpArea(float forestRadius, Vector3 position)
    {
        MaxRadius = forestRadius;
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

    const string defaultTreeName = "Tree";
    public GameObject TreePrefab { get; set; }
    public int TreeCount { get; set; }
    public List<TreeNode> Trees { get; set; }
    // public List<TreeNode> Trees { get; set; }
    /// <summary>
    /// Function should be called upon creation. Sets up internal parameters, and then grows the trees
    /// </summary>
    /// <param name="prefab"></param>
    public void SetUpForest(GameObject prefab, int treeCount)
    {
        TreePrefab = prefab;
        TreeCount = treeCount;

        GenerateTrees();
    }
    void GenerateTrees()
    {
        Trees = new List<TreeNode>();
        // Instantiate the trees
        for (int i = 0; i < TreeCount; i++)
        {
            string newTreeName = $"{Name}.{defaultTreeName}.{i}";
            Vector3 newTreePos = UtilityFunctions.GetRandomVector3(centerPosition, MaxRadius);
            TreeNode newTreeNode = CreateTree(newTreeName, newTreePos);
            Trees.Add(newTreeNode);
            AddLink(newTreeNode);
        }
    }
    TreeNode CreateTree(string treeName, Vector3 position)
    {
        GameObject newVillager = Instantiate(TreePrefab, position, Quaternion.identity);
        TreeNode treeNode = newVillager.AddComponent<TreeNode>();
        treeNode.SetUp(treeName);
        treeNode.transform.parent = transform;
        return treeNode;
    }
}
