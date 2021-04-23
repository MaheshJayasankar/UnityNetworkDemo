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
}
