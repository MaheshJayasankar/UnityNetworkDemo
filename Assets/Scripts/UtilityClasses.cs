using System;
using System.Collections.Generic;
using System.Linq;
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
            return UnityEngine.Random.Range(min, max);
        }
        public bool Contains(float value)
        {
            return value >= min && value <= max;
        }
        public static MinMaxRangeFloat operator *(MinMaxRangeFloat a, float b) => new MinMaxRangeFloat(a.min * b, a.max * b);
        public static MinMaxRangeFloat operator +(MinMaxRangeFloat a, float b) => new MinMaxRangeFloat(a.min + b, a.max + b);
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
            return UnityEngine.Random.Range(min, max + 1);
        }
        public static MinMaxRangeInt operator *(MinMaxRangeInt a, int b) => new MinMaxRangeInt(a.min * b, a.max * b);
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
            float genX = UnityEngine.Random.Range(lbX, ubX);

            float lbY = lowerBounds.y;
            float ubY = upperBounds.y;
            float genY = UnityEngine.Random.Range(lbY, ubY);

            float lbZ = lowerBounds.z;
            float ubZ = upperBounds.z;
            float genZ = UnityEngine.Random.Range(lbZ, ubZ);

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

            float radial_length = Mathf.Sqrt(new MinMaxRangeFloat(0, radius * radius).RandomSample());
            float angle = new MinMaxRangeFloat(0, 360).RandomSample();
            Vector3 angleVec = Quaternion.AngleAxis(angle, rotationAxis) * zeroAngleVector;

            Vector3 newVector3 = center + (angleVec * radial_length);
            return newVector3;
        }
        /// <summary>
        /// A random vector with a specified center and minimum to maximum radius away from center
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radiusRange"></param>
        /// <returns></returns>
        public static Vector3 GetRandomVector3(Vector3 center, MinMaxRangeFloat radiusRange)
        {
            Vector3 zeroAngleVector = Vector3.right;
            Vector3 rotationAxis = Vector3.up;


            float radial_length = Mathf.Sqrt(new MinMaxRangeFloat(Mathf.Pow(radiusRange.min,2), Mathf.Pow(radiusRange.max,2)).RandomSample());
            float angle = new MinMaxRangeFloat(0, 360).RandomSample();
            Vector3 angleVec = Quaternion.AngleAxis(angle, rotationAxis) * zeroAngleVector;

            Vector3 newVector3 = center + (angleVec * radial_length);
            return newVector3;
        }
        /// <summary>
        /// Random boundary point on rectangle of specified dimensions
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static Vector2 GetRandomBoundaryPoint(Vector2 bounds)
        {
            var sideIndex = UnityEngine.Random.Range(0, 4);
            Vector2 finalVector = Vector2.zero;
            Vector2 startVector = Vector2.zero;
            float runLength = 0;
            switch (sideIndex)
            {
                case 0:
                    // Left side
                    // Start from left bottom
                    startVector = new Vector2(-bounds.x / 2, -bounds.y / 2);
                    // Run up a random length between 0 and sideLength
                    runLength = UnityEngine.Random.Range(0f, bounds.y);
                    finalVector = startVector + Vector2.up * runLength;
                    break;
                case 1:
                    // Top side
                    // Start from left top
                    startVector = new Vector2(-bounds.x / 2, bounds.y / 2);
                    // Run up a random length between 0 and sideLength
                    runLength = UnityEngine.Random.Range(0f, bounds.y);
                    finalVector = startVector + Vector2.right * runLength;
                    break;
                case 2:
                    // Right side
                    // Start from top right
                    startVector = new Vector2(bounds.x / 2, bounds.y / 2);
                    // Run up a random length between 0 and sideLength
                    runLength = UnityEngine.Random.Range(0f, bounds.y);
                    finalVector = startVector + Vector2.down * runLength;
                    break;
                default:
                    // Start from bottom right
                    startVector = new Vector2(bounds.x / 2, -bounds.y / 2);
                    // Run up a random length between 0 and sideLength
                    runLength = UnityEngine.Random.Range(0f, bounds.y);
                    finalVector = startVector + Vector2.left * runLength;
                    break;

            }
            return finalVector;
        }
        public static void PutObjectOnGround(Transform objectTransform)
        {
            LayerMask mask = 0;
            mask |= (1 << 3);
            // Vertical offset can be used in case collision errors show up
            float verticalOffset = 0f;

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
    public static class RandomTools
    {
        // The Random object this method uses.
        private static System.Random GlobalRand = null;

        // Constructor
        static RandomTools()
        {
            GlobalRand = new System.Random(0);
        }

        // Return num_items random values.
        public static List<T> RandomSubset<T>(List<T> originalList, int subsetSize)
        {
            subsetSize = Mathf.Min(subsetSize, originalList.Count);
            // Make an array of indexes 0 through values.Length - 1.
            int[] indexes =
                Enumerable.Range(0, originalList.Count).ToArray();

            // Build the return list.
            List<T> results = new List<T>();

            // Randomize the first subsetSize indexes.
            for (int i = 0; i < subsetSize; i++)
            {
                // Pick a random entry between i and values.Length - 1.
                int j = GlobalRand.Next(i, originalList.Count);

                // Swap the values.
                int temp = indexes[i];
                indexes[i] = indexes[j];
                indexes[j] = temp;

                // Save the ith value.
                results.Add(originalList[indexes[i]]);
            }

            // Return the selected items.
            return results;
        }
    }
}