using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityClasses;

public class HutNode : MonoBehaviour, INode, IResidence
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
        HutNode newNode = newObject.AddComponent<HutNode>();
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
    #region IResidenceDefinition
    public List<INode> Residents { get; set; }
    public Vector3 ResidenceDimensions { get; set; }
    public void SetUpResidence(List<INode> residents, Vector3 residenceDimensions)
    {
        Residents = new List<INode>();
        AddResidents(residents);
        ResidenceDimensions = residenceDimensions;
    }
    public void AddResidents(List<INode> residents)
    {
        foreach (INode resident in residents)
        {
            if (!Residents.Contains(resident))
            {
                Residents.Add(resident);
                AddLink(resident);
            }
        }
    }
    public Vector3 CenterPosition
    {
        get
        {
            return transform.position;
        }
    }
    public bool IsInsideBounds(Vector3 targetPosition)
    {
        MinMaxRangeFloat xBounds = new MinMaxRangeFloat(-ResidenceDimensions.x / 2, ResidenceDimensions.x / 2) + CenterPosition.x;
        MinMaxRangeFloat yBounds = new MinMaxRangeFloat(-ResidenceDimensions.y / 2, ResidenceDimensions.y / 2) + CenterPosition.y;
        MinMaxRangeFloat zBounds = new MinMaxRangeFloat(-ResidenceDimensions.z / 2, ResidenceDimensions.z / 2) + CenterPosition.z;
        return xBounds.Contains(targetPosition.x) && yBounds.Contains(targetPosition.y) && zBounds.Contains(targetPosition.z);
    }
    #endregion
}