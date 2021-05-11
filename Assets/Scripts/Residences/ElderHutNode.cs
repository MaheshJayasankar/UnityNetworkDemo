using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityClasses;

public class ElderHutNode : MonoBehaviour, INode, IResidence, IArea
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
    public Vector3 CenterPosition { get
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
    #region IAreaDefinition
    public AreaType AreaType { get; private set; }
    public Vector3 Dimensions { get; private set; }
    public Vector3 Center { get; private set; }
    public Transform ObjectTransform { get { return transform; } }
    public TiledArea TiledArea { get; set; }
    public bool IsInside(Vector3 position)
    {
        Vector3 deltaPosition = position - Center;
        float distFromCenter = deltaPosition.sqrMagnitude;
        float maxDistFromCenter = (Dimensions / 2).sqrMagnitude;
        if (distFromCenter > maxDistFromCenter)
        {
            return false;
        }
        float minDistFromCenter = new List<float> { Center.x, Center.y, Center.z }.Min();
        if (distFromCenter < minDistFromCenter * minDistFromCenter)
        {
            return true;
        }
        float deltaX = Vector3.Dot(ObjectTransform.right, deltaPosition);
        if (!(Mathf.Abs(deltaX) <= (Dimensions / 2).x))
        {
            return false;
        }
        float deltaZ = Vector3.Dot(ObjectTransform.forward, deltaPosition);
        if (!(Mathf.Abs(deltaZ) <= (Dimensions / 2).z))
        {
            return false;
        }
        float deltaY = Vector3.Dot(ObjectTransform.up, deltaPosition);
        if (!(Mathf.Abs(deltaY) <= (Dimensions / 2).y))
        {
            return false;
        }
        return true;
    }
    public void AllignAreaWith(Transform allignmentTransform)
    {
        ObjectTransform.rotation = allignmentTransform.rotation;
    }
    public void SetUpArea(Transform orientationTransform, Vector3 dimensions, TiledArea parentTiledArea)
    {
        ObjectTransform.rotation = orientationTransform.rotation;
        ObjectTransform.position = orientationTransform.position;
        Center = orientationTransform.position;
        Dimensions = dimensions;
        AreaType = AreaType.rect;
        TiledArea = parentTiledArea;
    }
    #endregion
}