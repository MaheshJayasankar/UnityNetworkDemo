using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IArea
{
    public AreaType AreaType { get; }
    public Vector3 Dimensions { get; }
    public Vector3 Center { get; }
    public Transform ObjectTransform { get; }
    public TiledArea TiledArea { get; set; }
    public bool IsInside(Vector3 position);
    public void AllignAreaWith(Transform allignmentTransform);
    //public void SetUpArea(Transform objectTransform, Vector3 Dimensions);
}

public enum AreaType
{
    rect
}

public class TempArea: IArea
{
    public AreaType AreaType { get; private set; }
    public Vector3 Dimensions { get; private set; }
    public Vector3 Center { get; private set; }
    public Transform ObjectTransform { get; private set; }
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
        float minDistFromCenter = new List<float>{ Center.x,Center.y,Center.z}.Min();
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
    public void SetUpArea(Transform objectTransform, Vector3 dimensions)
    {
        ObjectTransform = objectTransform;
        Center = objectTransform.position;
        Dimensions = dimensions;
        AreaType = AreaType.rect;
    }
    public TempArea(Transform objectTransform, Vector3 dimensions)
    {
        SetUpArea(objectTransform, dimensions);
    }
}