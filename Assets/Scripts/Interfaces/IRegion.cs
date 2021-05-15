using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRegion
{
    public RegionLabel RegionLabel { get; set; }
    public RegionType RegionType { get; set; }
    public bool IsInside(Vector3 position);
    public bool IsColliding(IRegion targetRegion);
    public float MaxRadius { get; set; }
    public HashSet<TiledArea> TiledAreas { get; set; }
    public void SetUpRegion(float radius, Vector3 position);
    public Vector3 CenterPosition { get;}
}


public enum RegionType
{
    circle
}

public enum RegionLabel
{
    village,
    forest
}

/// <summary>
/// Temporary class used for any region type object before generation. After checking for no collisions with this temporary class,
/// the true IRegion object is created.
/// </summary>
public class TempRegion: IRegion
{
    public RegionLabel RegionLabel { get; set; }
    public RegionType RegionType { get; set; }
    public bool IsInside(Vector3 position)
    {
        return false;
    }
    public bool IsColliding(IRegion targetRegion)
    {
        return false;
    }
    public float MaxRadius { get; set; }
    public HashSet<TiledArea> TiledAreas { get; set; }
    public Vector3 CenterPosition { get; private set; }
    public void SetUpRegion(float radius, Vector3 position) { }
    public TempRegion(RegionType regionType, float maxRadius, Vector3 position)
    {
        RegionType = regionType;
        MaxRadius = maxRadius;
        CenterPosition = position;
    }
}
