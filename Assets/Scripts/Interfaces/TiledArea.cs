using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Tiled Area class used to organize a bunch of IAreas (like houses) closely bunched together in a tile map type structure
/// </summary>
public class TiledArea
{
    /// <summary>
    /// Side Lengths must be at least twice as than this size
    /// </summary>
    public const float minAreaSize = 0.1f;
    public List<IArea> Areas { get; set; }
    /// <summary>
    /// The Minimum Distance kept between two IAreas in the current TiledArea
    /// </summary>
    public float SnapPadding { get; private set; }
    public Transform centerTransform;

    /// <summary>
    /// Tiled Area made from a starting area. It will be the center of the area
    /// </summary>
    /// <param name="centralArea"></param>
    public TiledArea(IArea centralArea)
    {
        Areas = new List<IArea>
        {
            centralArea
        };
        centerTransform = centralArea.ObjectTransform;
        SnapPadding = 1f;
    }
    
    public IArea FindCollidingAreaIfAny(IArea candidateArea)
    {
        var candidateCenter = candidateArea.Center;
        // First check if center position is inside any area
        IArea centerAreaCollider = FindAreaIfInsideAny(candidateCenter);
        if (!(centerAreaCollider is null))
        {
            return centerAreaCollider;
        }
        else
        {
            // The list of possible collision areas is reduced. First, the areas that are too far away are eliminated
            float sqrMaxRadiusFromCenter = (candidateArea.Dimensions / 2).sqrMagnitude;
            var reducedAreaList = Areas.FindAll(area => (candidateCenter - area.Center).sqrMagnitude <= ((area.Dimensions / 2).sqrMagnitude + sqrMaxRadiusFromCenter));
            if (!reducedAreaList.Any())
                return null;
            // Check if any of the 4 corners of the candidate area collide
            var cornerPts = GetFourCorners(candidateCenter, candidateArea.Dimensions, centerTransform);
            foreach (var cornerPt in cornerPts)
            {
                var cornerCollision = FindAreaIfInsideAnySubset(cornerPt, reducedAreaList);
                if (!(cornerCollision is null))
                {
                    return cornerCollision;
                }
            }
            // Check if any of the 4 corners of all the other areas collide with this area
            // Iterate over all other areas, and their corresponding corner points
            foreach (var otherArea in reducedAreaList)
            {
                var currentCornerPtSet = GetFourCorners(otherArea.Center, otherArea.Dimensions, centerTransform);

                foreach (var cornerPt in currentCornerPtSet)
                {
                    if (candidateArea.IsInside(cornerPt))
                    {
                        return otherArea;
                    }
                }
            }
            return null;
        }
        
    }
    private IArea FindAreaIfInsideAny(Vector3 position)
    {
        return Areas.Find(area => area.IsInside(position));

    }
    private IArea FindAreaIfInsideAnySubset(Vector3 position, List<IArea> subsetAreas)
    {
        return subsetAreas.Find(area => area.IsInside(position));
    }
    /// <summary>
    /// Return order: NW, NE, SE, SW
    /// </summary>
    /// <param name="centerPosition"></param>
    /// <param name="dimensions"></param>
    /// <param name="orientation"></param>
    /// <returns></returns>
    private List<Vector3> GetFourCorners(Vector3 centerPosition, Vector3 dimensions, Transform orientation)
    {
        float xHalfLength = dimensions.x / 2;
        float zHalfLength = dimensions.z / 2;

        Vector3 fwVec = centerTransform.forward;
        Vector3 bcVec = -fwVec;
        Vector3 rgVec = centerTransform.right;
        Vector3 lfVec = -rgVec;

        Vector3 forwardRight = fwVec * xHalfLength + rgVec * zHalfLength;
        Vector3 forwardLeft = fwVec * xHalfLength + lfVec * zHalfLength;

        Vector3 forwardRightPoint = centerPosition + forwardRight;
        Vector3 forwardLeftPoint = centerPosition + forwardLeft;
        Vector3 backwardRightPoint = centerPosition - forwardLeft;
        Vector3 backwardLeftPoint = centerPosition - forwardRight;

        return new List<Vector3> { 
            forwardLeftPoint,
            forwardRightPoint,
            backwardRightPoint,
            backwardLeftPoint
            };
    }
    /// <summary>
    /// Note: Must be called only after ascertaining it is inside the required area
    /// </summary>
    /// <param name="position"></param>
    /// <returns>Center of open space adjacent to tile map space</returns>
    public Vector3 SnapToClosestOpenSpace(IArea candidateArea)
    {
        var candidateCenter = candidateArea.Center;
        var collidingArea = FindCollidingAreaIfAny(candidateArea);
        if (collidingArea is null)
        {
            throw new MissingReferenceException(message: $"Unable to find Area to snap from, for candidate area at position {candidateCenter}");
        }
        // Continue Snapping until no more snapping is possible
        // Find direction of movement
        Vector3 deltaPosition = candidateCenter - collidingArea.Center;
        float fwMag = Vector3.Dot(deltaPosition, centerTransform.forward);
        float bcMag = Vector3.Dot(deltaPosition, -centerTransform.forward);
        float rgMag = Vector3.Dot(deltaPosition, centerTransform.right);
        float lfMag = Vector3.Dot(deltaPosition, -centerTransform.right);

        // Compare which of the above magnitudes are the highest
        var moveDir = centerTransform.forward;
        var movementDirectionEnum = MovementDirectionEnum.Forward;
        var maxMag = fwMag;
        // Check if rightward direction shows bigger magnitude
        if (rgMag > maxMag)
        {
            maxMag = rgMag;
            movementDirectionEnum = MovementDirectionEnum.Rightward;
            moveDir = centerTransform.right;
        }
        // Check if backward direction shows bigger magnitude
        if(bcMag > maxMag)
        {
            maxMag = bcMag;
            movementDirectionEnum = MovementDirectionEnum.Forward;
            moveDir = -centerTransform.forward;
        }
        // Check if backward direction shows bigger magnitude
        if (lfMag > maxMag)
        {
            movementDirectionEnum = MovementDirectionEnum.Rightward;
            moveDir = -centerTransform.right;
        }

        // We have found the direction of movement.
        bool edgeReached = false;
        IArea currentlyActiveArea = collidingArea;

        // NOTE: Debug max iterations value. Remove late
        int maxIter = 100000;
        int iterCount = 0;
        // Continue moving in this direction, until the edge of the tile map is reached
        while (!edgeReached)
        {
            iterCount += 1;
            var candidateAreaSideHalfLength = candidateArea.Dimensions.x / 2;
            var collidingAreaSideHalfLength = currentlyActiveArea.Dimensions.x / 2;
            if (movementDirectionEnum == MovementDirectionEnum.Rightward)
            {
                collidingAreaSideHalfLength = currentlyActiveArea.Dimensions.z / 2;
                candidateAreaSideHalfLength = candidateArea.Dimensions.z / 2;
            }
            Vector3 newTargetCenter = currentlyActiveArea.Center + moveDir * (collidingAreaSideHalfLength + candidateAreaSideHalfLength + SnapPadding);
            // The new target center has to be checked for any further collisions.
            var newCollidingArea = FindAreaIfInsideAny(newTargetCenter + moveDir * minAreaSize);
            if (newCollidingArea is null)
            {
                return newTargetCenter;
            }
            if (iterCount > maxIter)
            {
                throw new UnityException(message: $"Snapping could not complete in specified iteration limit starting from {candidateCenter}. Too many objects in tileset?");
            }
            if (currentlyActiveArea.Equals(newCollidingArea))
            {
                throw new UnityException(message: $"Recursive snapping loop detected at {currentlyActiveArea.Center}. Cannot proceed");
            }
            currentlyActiveArea = newCollidingArea;
        }
        return Vector3.zero;
    }
    public void AddArea(IArea area)
    {
        Areas.Add(area);
    }
}

public enum MovementDirectionEnum
{
    Forward,
    Rightward
}