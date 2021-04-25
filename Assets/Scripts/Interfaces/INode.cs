using System;
using System.Collections.Generic;
using UnityEngine;

public interface INode
{
    public HashSet<INode> Linked { get; set; }
    public string Name { get; set; }
    public Guid Id { get; set; }
    public void AddLink(INode node);
    public void RemoveLink(INode node);
    //TODO: Duplicate Node to be changed to instantiating new gameObject and then adding to it
    public INode DuplicateNode(bool selfLink);
    public void SetUp(string name);
}

/*
 * ====================================================
 * EXAMPLE DEFINITION OF AN INODE
 * Replace NewTypeNode with desired Node Type
 * ====================================================
 * 
 * 
public class NewTypeNode : MonoBehaviour, INode
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
        NewTypeNode newNode = newObject.AddComponent<NewTypeNode>();
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
 */