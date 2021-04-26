using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResidence
{
    public List<INode> Residents { get; set; }
    /// <summary>
    /// Dimensions of the residence, in 3-D. Dimensions are spanned as a cuboid with side lengths as per the Vector3, centered at the residence center.
    /// Ideally, no other residence should overlap with this residence. Residences should be stacked (snapping face to face) if they are generated close.
    /// </summary>
    public Vector3 ResidenceDimensions { get; set; }
    public void SetUpResidence(List<INode> residents, Vector3 residenceRadius);
    public void AddResidents(List<INode> resident);
    public Vector3 CenterPosition { get; }
    public bool IsInsideBounds(Vector3 targetPosition);
}

/*
 * 
 * ====================================================
 * EXAMPLE DEFINITION OF AN IRESIDENCE
 * ====================================================
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
 * 
 */
