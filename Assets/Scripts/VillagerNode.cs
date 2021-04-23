using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerNode : MonoBehaviour, INode
{
    public HashSet<INode> Linked { get; set; }
    public string Name { get; set; }
    public Guid Id { get; set; }
    public void SetUp(string name)
    {
        Name = name;
        Linked = new HashSet<INode>();
        Id = Guid.NewGuid();
    }
    public VillagerNode(string name)
    {
        Name = name;
        Linked = new HashSet<INode>();
        Id = Guid.NewGuid();
    }

    public VillagerNode(string name, HashSet<INode> linked)
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
        VillagerNode newNode = new VillagerNode(Name);
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
