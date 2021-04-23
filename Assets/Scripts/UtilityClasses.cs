using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityClasses
{
    [System.Serializable]
    public struct Range
    {
        public float min;
        public float max;

        public Range(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
    /// <summary>
    /// Utility Functions for procedural generation
    /// </summary>
    public static class UtilityFunctions
    {
        /// <summary>
        /// Creates a random Vector3 between the upper and lower bounds specified. 
        /// Note: This will push forward the random state by 3.
        /// </summary>
        public static Vector3 GetRandomVector3(Vector3 lowerBounds, Vector3 upperBounds)
        {
            var lbX = lowerBounds.x;
            var ubX = upperBounds.x;
            var genX = Random.Range(lbX, ubX);

            var lbY = lowerBounds.x;
            var ubY = upperBounds.x;
            var genY = Random.Range(lbX, ubX);

            var lbZ = lowerBounds.x;
            var ubZ = upperBounds.x;
            var genZ = Random.Range(lbX, ubX);

            return new Vector3(genX, genY, genZ);

        }
    }
}