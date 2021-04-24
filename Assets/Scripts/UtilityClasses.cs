using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityClasses
{

    [System.Serializable]
    public struct MinMaxRangeFloat
    {
        public float min;
        public float max;

        public MinMaxRangeFloat(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
        public float RandomSample()
        {
            return Random.Range(min, max);
        }
    }
    [System.Serializable]
    public struct MinMaxRangeInt
    {
        public int min;
        public int max;

        public MinMaxRangeInt(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
        public int RandomSample()
        {
            return Random.Range(min, max);
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
            float lbX = lowerBounds.x;
            float ubX = upperBounds.x;
            float genX = Random.Range(lbX, ubX);

            float lbY = lowerBounds.y;
            float ubY = upperBounds.y;
            float genY = Random.Range(lbY, ubY);

            float lbZ = lowerBounds.z;
            float ubZ = upperBounds.z;
            float genZ = Random.Range(lbZ, ubZ);

            return new Vector3(genX, genY, genZ);

        }
        /// <summary>
        /// Returns a random vector inside a flat circular area around the central vector. Assumes surface is the xz plane.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns>Randomly Generated Vector3 within specified radius from center</returns>
        public static Vector3 GetRandomVector3(Vector3 center, float radius)
        {
            Vector3 zeroAngleVector = Vector3.right;
            Vector3 rotationAxis = Vector3.up;

            float radial_length = new MinMaxRangeFloat(0, radius).RandomSample();
            float angle = new MinMaxRangeFloat(0, 360).RandomSample();
            Vector3 angleVec = Quaternion.AngleAxis(angle, rotationAxis) * zeroAngleVector;

            Vector3 newVector3 = center + (angleVec * radial_length);
            return newVector3;
        }
        public static void PutObjectOnGround(Transform objectTransform)
        {
            LayerMask mask = 0;
            mask |= (1 << 3);
            float verticalOffset = 2f;

            // raycast to find the y-position of the masked collider at the transforms x/z
            // note that the ray starts at 100 units
            Ray ray = new Ray(objectTransform.position + (Vector3.up * 100), Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask))
            {
                if (hit.collider != null)
                {
                    // this is where the gameobject is actually put on the ground
                    objectTransform.position = new Vector3(objectTransform.position.x, hit.point.y + verticalOffset, objectTransform.position.z);
                }
            }
        }
    }
}