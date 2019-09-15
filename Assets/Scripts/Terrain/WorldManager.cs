using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    [ExecuteInEditMode]
    public class WorldManager : MonoBehaviour
    {
        public bool generated = true;

        [SerializeField] private TerrainLayer[] terrainLayers = null;

        private float depth = 8f;
        private int width = 128;

        private int heightPointPerUnity = 2;

        [SerializeField] public ConditionnedTexture[] textures;

        private List<Island> islands;

        // Update is called once per frame
        void Update()
        {
            if (!generated)
            {
                // DEBUG island list
                InitList();

                generated = true;
                LoadArround(0, 0);
            }
        }

        void InitList()
        {
            islands = new List<Island>();
            islands.Add(new Island(new Vector2(-70, -64)));
            islands.Add(new Island(new Vector2(-70, 0)));
            islands.Add(new Island(new Vector2(-70, 64)));
            islands.Add(new Island(new Vector2(70, 64)));
        }

        void LoadArround(float x, float z)
        {
            GenerateNewTerrain(0, 0);
            GenerateNewTerrain(-1, 0);
            GenerateNewTerrain(0, -1);
            GenerateNewTerrain(-1,-1);
        }

        void GenerateNewTerrain(int xBase, int zBase)
        {
            TerrainData terrainData = new TerrainData();

            terrainData.heightmapResolution = width * heightPointPerUnity + 1;
            terrainData.alphamapResolution = width * 4;
            terrainData.baseMapResolution = 1024;
            terrainData.SetDetailResolution(1024, 16);

            terrainData.terrainLayers = terrainLayers;

            terrainData.size = new Vector3(width, depth, width);

            GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
            terrainGO.name = "Terrain_" + xBase + "_" + zBase;
            terrainGO.transform.position = new Vector3(128 * xBase, 0, 128 * zBase);
            terrainGO.layer = 9;

            // Generate islands and textures
            terrainGO.GetComponent<Terrain>().terrainData = GenerateData(terrainData, xBase * width, zBase * width);
        }

        private TerrainData GenerateData(TerrainData terrainData, float xBase, float zBase)
        {
            float[,] heights = GenerateFlat();

            // Look for islands to be added
            foreach(Island island in islands)
            {
                // TODO add a distance condition with 2d index
                island.GenerateIslandAndHeights(
                    ref heights,
                    new Vector2(xBase, zBase),
                    width,
                    heightPointPerUnity);
            }

            terrainData.SetHeights(0, 0, heights);

            // Textures
            terrainData = GenerateAlphaMaps(terrainData);

            return terrainData;
        }

        private float[,] GenerateFlat()
        {
            float[,] heights = new float[width * heightPointPerUnity + 1, width * heightPointPerUnity + 1];
            for (int i = 0; i < width * heightPointPerUnity + 1; i++)
            {
                for (int j = 0; j < width * heightPointPerUnity + 1; j++)
                {
                    heights[i, j] = 0f;
                }
            }

            return heights;
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
            for (int y = 0; y < alphaMapHeight; y++)
            {
                for (int x = 0; x < alphaMapWidth; x++)
                {
                    foreach (ConditionnedTexture texture in textures)
                    {
                        // does it fit?
                        if (texture.Fits(terrainData.GetHeight(y, x) / terrainData.heightmapScale.y))
                        {
                            // Write a 1 into the alpha map 
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
        private void PaintSlopes(ref float[,,] map)
        {/*
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
            }*/
        }
    }
}
