using System;
using System.Collections.Generic;
using UnityEngine;

public class ElderHutNode : MonoBehaviour, INode, IResidence
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
        ElderHutNode newNode = newObject.AddComponent<ElderHutNode>();
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
    public float ResidenceRadius { get; set; }
    public void SetUpResidence(List<INode> residents, float residenceRadius)
    {
        Residents = new List<INode>();
        AddResidents(residents);
        ResidenceRadius = residenceRadius;
    }
    public void AddResidents(List<INode> residents)
    {
        foreach (var resident in residents)
        {
            if (!Residents.Contains(resident))
            {
                Residents.Add(resident);
                AddLink(resident);
            }
        }
    }
    #endregion
}