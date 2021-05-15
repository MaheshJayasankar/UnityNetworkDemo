using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Residence contains information about tenants, capacity, and other related information. Locational data stoed in associated IArea component.
/// </summary>
public interface IResidence
{
    public List<INode> Residents { get; set; }
    public IRegion ParentRegion { get; set; }
    public void SetUpResidence<T>(T region, List<INode> residents) where T : INode, IRegion;
    public void AddResidents(List<INode> resident);
    public Vector3 CenterPosition { get; }
}

/*
 * 
 * ====================================================
 * EXAMPLE DEFINITION OF AN IRESIDENCE
 * ====================================================
    #region IResidenceDefinition
    public List<INode> Residents { get; set; }
    public IRegion ParentRegion { get; set; }
    public Vector3 ResidenceDimensions { get; set; }
    public void SetUpResidence<T>(T region, List<INode> residents) where T : INode, IRegion
    {
        ParentRegion = region;
        Residents = new List<INode>();
        AddResidents(residents);
        AddLink(region);
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
    #endregion
 * 
 */
