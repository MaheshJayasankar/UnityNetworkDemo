using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArea
{
    public AreaType AreaType { get; set; }
    public bool IsInside(Vector3 position);
    public bool IsColliding(IArea targetArea);
    public float MaxRadius { get; set; }
    public Vector3 CenterPosition { get;}
}


public enum AreaType
{
    circle
}

/// <summary>
/// Temporary class used for any area type object before generation. After checking for no collisions with this temporary class,
/// the true IArea object is created.
/// </summary>
public class TempArea: IArea
{
    public AreaType AreaType { get; set; }
    public bool IsInside(Vector3 position)
    {
        return false;
    }
    public bool IsColliding(IArea targetArea)
    {
        return false;
    }
    public float MaxRadius { get; set; }
    public Vector3 CenterPosition { get; private set; }
    public TempArea(AreaType areaType, float maxRadius, Vector3 position)
    {
        AreaType = areaType;
        MaxRadius = maxRadius;
        CenterPosition = position;
    }
}
