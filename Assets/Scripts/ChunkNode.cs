using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkNode : MonoBehaviour, INode
{
    #region INodeFunctions
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

    
}
