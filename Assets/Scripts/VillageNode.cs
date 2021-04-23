using System;
using System.Collections.Generic;
using UnityEngine;

class VillageNode: MonoBehaviour, INode
{
    public HashSet<INode> Linked { get; set; }
    public string Name { get; set; }
    public Guid Id { get; set; }
    public int HeadCount { get; set; }
    public float MaxRadius { get; set; }
    public void SetUp(string name, float radius)
    {
        Name = name;
        Linked = new HashSet<INode>();
        Id = Guid.NewGuid();
        MaxRadius = radius;
    }
    public VillageNode(string name)
    {
        Name = name;
        Linked = new HashSet<INode>();
        Id = Guid.NewGuid();
    }

    public VillageNode(string name, HashSet<INode> linked)
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
        VillageNode newNode = new VillageNode(Name);
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

}