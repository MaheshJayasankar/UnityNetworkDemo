using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResidence
{
    public List<INode> Residents { get; set; }
    public float ResidenceRadius { get; set; }
    public void SetUpResidence(List<INode> residents, float residenceRadius);
    public void AddResidents(List<INode> resident);

}

/*
 * 
 * ====================================================
 * EXAMPLE DEFINITION OF AN IRESIDENCE
 * ====================================================
    #region IResidenceDefinition
    public List<INode> Residents { get; set; }
    public float ResidenceRadius { get; set; }
    public void SetUpResidence(List<INode> residents, float residenceRadius)
    {
        Residents = new List<INode>();
        AddResidents(residents);
        ResidenceRadius = residenceRadius;
    }
    public void AddResidents(List<INode> residents)
    {
        foreach (var resident in residents)
        {
            if (!Residents.Contains(resident))
            {
                Residents.Add(resident);
                AddLink(resident);
            }
        }
    }
    #endregion
 * 
 */
