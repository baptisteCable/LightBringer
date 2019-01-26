using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    [ExecuteInEditMode]
    public class TerrainGenerator : MonoBehaviour
    {
        private const int SLOPE_TEXTURE_ID = 2;

        private float depth = 8f;
        private int height = 256;
        private int width = 256;

        private int heightPointPerUnity = 2;

        public ConditionnedTexture[] textures;

        public bool generated = true;

        private List<Island> islands;

        private void Update()
        {
            if (!generated)
            {
                generated = true;
                GenerateTerrain();
            }
        }

        public void GenerateTerrain()
        {
            Terrain terrain = GetComponent<Terrain>();
            terrain.terrainData = GenerateData(terrain.terrainData);
        }

        private TerrainData GenerateData(TerrainData terrainData)
        {
            islands = new List<Island>();

            // Heights
            terrainData.heightmapResolution = width * heightPointPerUnity + 1;
            terrainData.size = new Vector3(width, depth, height);

            float[,] heights = GenerateFlat();

            AddIsland(ref heights, 110, 64, 10);
            AddIsland(ref heights, 110, 192, 10);
            AddIsland(ref heights, 166, 64, 10);
            AddIsland(ref heights, 166, 192, 10);
            AddIsland(ref heights, 90, 128, 10);
            AddIsland(ref heights, 192, 128, 10);

            heights = Smooth(heights, 1);

            terrainData.SetHeights(0, 0, heights);

            // Textures
            terrainData = GenerateAlphaMaps(terrainData);


            return terrainData;
        }

        private TerrainData GenerateAlphaMaps(TerrainData terrainData)
        {
            float[,,] map = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

            // Clear
            ClearAlphaMaps(map);

            // store with and height of the alpha maps
            int alphaMapWidth = terrainData.alphamapWidth;
            int alphaMapHeight = terrainData.alphamapHeight;

            // for all positions in the alpha maps
            for (int y = 0; y < alphaMapHeight; ++y)
            {
                for (int x = 0; x < alphaMapWidth; ++x)
                {
                    foreach (ConditionnedTexture texture in textures)
                    {
                        // does it fit?
                        if (texture.Fits(terrainData.GetHeight(y, x) / terrainData.heightmapScale.y))
                        {
                            // Write a 1 into the alpha map using the GroundTextIndex from the biom description
                            map[x, y, texture.groundTexIndex] = 1;
                        }
                    }
                }
            }

            // Paint slopes
            PaintSlopes(ref map);

            // hand alpha maps back to Unity
            terrainData.SetAlphamaps(0, 0, map);

            return terrainData;
        }

        private void PaintSlopes(ref float[,,] map)
        {
            foreach (Island island in islands)
            {
                foreach (Slope slope in island.slopes)
                {
                    foreach (Vector2Int point in slope.GetPointList(-2, 1, 1).Keys)
                    {
                        Vector2Int corner = island.GetStartingCorner();
                        int x = point.x + corner.x;
                        int y = point.y + corner.y;

                        for (int i = 0; i < textures.Length;i++)
                        {
                            map[x, y, i] = 0;
                        }
                        map[x, y, SLOPE_TEXTURE_ID] = 1;
                    }
                }
            }
        }

        private void ClearAlphaMaps(float[,,] map)
        {
            for (int x = 0; x < map.GetLength(0); ++x)
            {
                for (int y = 0; y < map.GetLength(1); ++y)
                {
                    for (int z = 0; z < map.GetLength(2); ++z)
                    {
                        map[x, y, z] = 0;
                    }
                }
            }
        }

        private float[,] GenerateFlat()
        {
            float[,] heights = new float[height * heightPointPerUnity, width * heightPointPerUnity];
            for (int i = 0; i < width * heightPointPerUnity; i++)
            {
                for (int j = 0; j < height * heightPointPerUnity; j++)
                {
                    heights[i, j] = 0f;
                }
            }

            return Smooth(heights, 1);
        }

        private void AddIsland(ref float[,] heights, int x, int y, float size)
        {
            Island island = new Island(x, y);
            island.GenerateIsland(ref heights);
            island.AddIslandToMap(ref heights);
            islands.Add(island);
        }

        private float[,] Smooth(float[,] heights, int radius)
        {
            float[,] smoothed = new float[height * heightPointPerUnity, width * heightPointPerUnity];
            int nbPoint = (2 * radius + 1) * (2 * radius + 1);

            for (int i = radius; i < width * 2 - radius; i++)
            {
                for (int j = radius; j < height * 2 - radius; j++)
                {
                    smoothed[i, j] = 0f;

                    for (int k = -radius; k <= radius; k++)
                    {
                        for (int l = -radius; l <= radius; l++)
                        {
                            smoothed[i, j] += heights[i + k, j + l];
                        }
                    }
                    smoothed[i, j] /= nbPoint;
                }
            }

            return smoothed;
        }

        public static Vector2 Vector2FromAngle(float a)
        {
            a *= Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        }

        public static Vector2 RotateVector(Vector2 v, float angle)
        {
            float radian = angle * Mathf.Deg2Rad;
            float x = v.x * Mathf.Cos(radian) - v.y * Mathf.Sin(radian);
            float y = v.x * Mathf.Sin(radian) + v.y * Mathf.Cos(radian);
            return new Vector2(x, y);
        }
    }

}

