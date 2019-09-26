using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{


    [Serializable]
    public class Biome
    {
        [Serializable]
        public enum Type
        {
            Light = 0,
            Darkness = 1,
            Fire = 2,
            Ice = 3,
            Earth = 4,
            Air = 5
        }

        public const float SQUARE_RADIUS = 100;
        public const float MAX_DEFORMATION_RATIO = 2.5f;

        public Vector2[] vertices;
        private float rotation; // between 0 and pi

        public Type type;

        [NonSerialized]
        private static System.Random rnd;

        public Biome(Vector2 center)
        {
            if (rnd == null)
            {
                rnd = new System.Random();
            }

            type = (Type)rnd.Next(Enum.GetNames(typeof(Type)).Length);

            rotation = (float)(rnd.NextDouble() % Math.PI);
            float ratio = (float)(rnd.NextDouble()) * (MAX_DEFORMATION_RATIO - 1) + 1;

            vertices = new Vector2[4];
            vertices[0] = center + Island.RotateVector(Vector2.right * ratio * SQUARE_RADIUS, rotation);
            vertices[1] = center + Island.RotateVector(Vector2.right / ratio * SQUARE_RADIUS, rotation + (float)Math.PI / 2f);
            vertices[2] = center + Island.RotateVector(Vector2.right * ratio * SQUARE_RADIUS, rotation + (float)Math.PI);
            vertices[3] = center + Island.RotateVector(Vector2.right / ratio * SQUARE_RADIUS, rotation + (float)Math.PI * 3f / 2f);
        }

        public float Distance(Vector2 point)
        {
            float minDist = float.PositiveInfinity;

            if (IsInsidePolygon(point))
            {
                return 0;
            }

            for (int i = 0; i < 4; i++)
            {
                float dist = DistanceFromSegment(point, vertices[i], vertices[(i + 1) % 4]);
                if (dist < minDist)
                {
                    minDist = dist;
                }
            }

            return minDist;
        }

        private float DistanceFromSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            // if closer to a segment inner point
            if (Vector2.Dot(b - a, point - a) > 0 && Vector2.Dot(a - b, point - b) > 0)
            {
                return Math.Abs(Vector2.Dot(Island.RotateVector(b - a, (float)Math.PI / 2f).normalized, point - a));
            }
            // else, closer to a vertex
            else
            {
                return Math.Min((point - a).magnitude, (point - b).magnitude);
            }
        }

        private bool IsInsidePolygon(Vector2 point)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Vector2.Dot(
                            Island.RotateVector(vertices[(i + 1) % 4] - vertices[i], (float)Math.PI / 2f),
                            point - vertices[i]
                        ) < 0
                    )
                {
                    return false;
                }
            }

            return true;
        }

        static public Biome GetBiome(SpatialDictionary<Biome> biomes, Vector2 position,
            int searchingDistance = (int)(5 * WorldCreator.MIN_DISTANCE_BETWEEN_BIOMES_POLY))
        {
            List<Biome> closeBiomes = biomes.GetAround((int)position.x, (int)position.y, searchingDistance);

            if (closeBiomes.Count == 0)
            {
                if (searchingDistance > 1000000)
                {
                    throw new Exception("No biomes found");
                }

                return GetBiome(biomes, position, searchingDistance * 2);
            }

            Biome closest = null;
            float minDist = float.PositiveInfinity;

            foreach (Biome biome in closeBiomes)
            {
                float dist = biome.Distance(position);
                if (dist == 0)
                {
                    return biome;
                }

                if (dist < minDist)
                {
                    minDist = dist;
                    closest = biome;
                }
            }

            return closest;

        }

    }
}
