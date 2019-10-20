using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    [ExecuteInEditMode]
    public class EditWorldManager : MonoBehaviour
    {
        public const int TERRAIN_WIDTH = 128;
        private const float DEPTH = 8f;
        public const int HEIGHT_POINT_PER_UNIT = 2;

        public const int SLOPE_TEXTURE_ID = 2;

        public int nbOfTiles = 2;

        // Map painting
        public const int NB_BIOME_TYPE = 6;
        public const int NB_GROUND_TYPE = 5;
        private const int BIOME_APPROX = 8;

        // Blur
        public const int BLUR_RADIUS = 6;

        // Terrain painting
        [SerializeField] private TerrainLayer[] terrainLayers = null;

        // Biomes, islands
        private SpatialDictionary<Biome> biomes;
        private SpatialDictionary<Island> islands;
        private WorldCreator wc;

        // test button
        public bool doWork = true;

        private void Update ()
        {
            if (!doWork)
            {
                doWork = true;
                InitWorldData ();
                InitFirstTiles ();
            }
        }

        private void InitFirstTiles ()
        {
            for (int u = -nbOfTiles; u < nbOfTiles; u++)
            {
                for (int v = -nbOfTiles; v < nbOfTiles; v++)
                {
                    GenerateTerrainData (u, v, out float[,] heights, out float[,,] map);
                    GenerateNewTerrain (u, v, heights, map);
                }
            }
        }

        private void InitWorldData ()
        {
            wc = new WorldCreator (Application.persistentDataPath + "/");
            wc.LoadData (out biomes, out islands);

            if (biomes == null || islands == null)
            {
                biomes = new SpatialDictionary<Biome> ();
                islands = new SpatialDictionary<Island> ();
                wc.CreateMapSector (ref biomes, ref islands, 0, 0);
            }
        }

        void GenerateNewTerrain (int xBase, int zBase, float[,] heights, float[,,] map)
        {
            // Terrain data
            TerrainData terrainData = new TerrainData
            {
                heightmapResolution = TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT + 1,
                alphamapResolution = TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT,
                baseMapResolution = TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT,
                terrainLayers = terrainLayers,
                thickness = 2,
                size = new Vector3 (TERRAIN_WIDTH, DEPTH, TERRAIN_WIDTH)
            };
            terrainData.SetDetailResolution (1024, 16);
            terrainData.SetHeights (0, 0, heights);

            // Game objet creation
            GameObject terrainGO = Terrain.CreateTerrainGameObject (terrainData);
            terrainGO.name = "Terrain_" + xBase + "_" + zBase;
            terrainGO.transform.position = new Vector3 (TERRAIN_WIDTH * xBase, 0, TERRAIN_WIDTH * zBase);

            // layer and tag
            terrainGO.tag = "Terrain";
            terrainGO.layer = 9;

            // terrain data asset
            AssetDatabase.CreateAsset (terrainData, "Assets/Terrains/Data/" + terrainGO.name + ".asset");
            terrainData.SetAlphamaps (0, 0, map);
            PrefabUtility.SaveAsPrefabAsset (terrainGO, "Assets/Terrains/" + terrainGO.name + ".prefab");
        }

        private void GenerateTerrainData (int xBase, int zBase, out float[,] heights, out float[,,] map)
        {
            xBase *= TERRAIN_WIDTH;
            zBase *= TERRAIN_WIDTH;

            // Find impacting biomes
            List<Biome> impactingBiomes = new List<Biome> ();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    foreach (Dic2DKey key in Biome.Get4ClosestBiomes (biomes,
                        new Vector2 (xBase + i * TERRAIN_WIDTH / 2, zBase + j * TERRAIN_WIDTH / 2), out _))
                    {
                        Biome biome = biomes.Get (key);
                        if (!impactingBiomes.Contains (biome))
                        {
                            impactingBiomes.Add (biome);
                        }
                    }
                }
            }

            int mapSize = HEIGHT_POINT_PER_UNIT * TERRAIN_WIDTH;
            map = new float[mapSize, mapSize, NB_GROUND_TYPE * NB_BIOME_TYPE];
            heights = new float[mapSize + 1, mapSize + 1];

            Biome.Type[,] biomeMap = new Biome.Type[mapSize + 2 * BLUR_RADIUS, mapSize + 2 * BLUR_RADIUS];
            GroundType[,] groundMap = new GroundType[mapSize + 2 * BLUR_RADIUS, mapSize + 2 * BLUR_RADIUS];

            FillBiomeMapBorder (impactingBiomes, ref biomeMap, xBase, zBase);
            FillBiomeMap (impactingBiomes, ref biomeMap, xBase, zBase, mapSize, BLUR_RADIUS, BLUR_RADIUS);

            List<Island> islandList;

            // Look for islands to be added
            islandList = islands.GetAround (
                    xBase + TERRAIN_WIDTH / 2,
                    zBase + TERRAIN_WIDTH / 2,
                    TERRAIN_WIDTH / 2 + (int)((Island.MAX_RADIUS + 1) * Island.SCALE * 3)
                );

            Vector2 terrainPosition = new Vector2 (xBase, zBase);

            FillGroundMap (islandList, ref groundMap, terrainPosition);

            foreach (Island island in islandList)
            {
                island.GenerateIslandHeightsAndAlphaMap (
                            ref map,
                            ref heights,
                            ref biomeMap,
                            ref groundMap,
                            terrainPosition
                        );
            }

            PaintMap (ref map, ref biomeMap, ref groundMap);
        }

        private void FillBiomeMap (List<Biome> biomes, ref Biome.Type[,] biomeMap,
            int xBase, int zBase, int size, int xStart, int yStart)
        {
            int i, j;

            if (size >= BIOME_APPROX)
            {
                bool diff = false;
                i = xStart;
                while (i < xStart + size + 1 && !diff)
                {
                    j = yStart;
                    while (j < yStart + size + 1 && !diff)
                    {

                        FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, i, j);

                        if (biomeMap[i, j] != biomeMap[xStart, yStart])
                        {
                            diff = true;
                        }

                        j += BIOME_APPROX;
                    }

                    i += BIOME_APPROX;
                }

                if (diff)
                {
                    FillBiomeMap (biomes, ref biomeMap, xBase, zBase, size / 2, xStart, yStart);
                    FillBiomeMap (biomes, ref biomeMap, xBase, zBase, size / 2, xStart + size / 2, yStart);
                    FillBiomeMap (biomes, ref biomeMap, xBase, zBase, size / 2, xStart, yStart + size / 2);
                    FillBiomeMap (biomes, ref biomeMap, xBase, zBase, size / 2, xStart + size / 2, yStart + size / 2);
                }
                else
                {
                    // All the same biome
                    for (i = xStart; i < xStart + size; i++)
                    {
                        for (j = yStart; j < yStart + size; j++)
                        {
                            biomeMap[i, j] = biomeMap[xStart, yStart];
                        }
                    }
                }
            }
            else
            {
                for (i = xStart; i < xStart + size; i++)
                {
                    for (j = yStart; j < yStart + size; j++)
                    {
                        FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, i, j);
                    }
                }
            }
        }

        private void FillBiomeMapBorder (List<Biome> biomes, ref Biome.Type[,] biomeMap, int xBase, int zBase)
        {
            int mapSize = HEIGHT_POINT_PER_UNIT * TERRAIN_WIDTH;

            FillBiomeMapBorderH (biomes, ref biomeMap, xBase, zBase,
                0, mapSize + 2 * BLUR_RADIUS,
                0, BLUR_RADIUS);
            FillBiomeMapBorderH (biomes, ref biomeMap, xBase, zBase,
                0, mapSize + 2 * BLUR_RADIUS,
                mapSize + BLUR_RADIUS, mapSize + 2 * BLUR_RADIUS);
            FillBiomeMapBorderV (biomes, ref biomeMap, xBase, zBase,
                mapSize + BLUR_RADIUS, mapSize + 2 * BLUR_RADIUS,
                BLUR_RADIUS, mapSize + BLUR_RADIUS);
            FillBiomeMapBorderV (biomes, ref biomeMap, xBase, zBase,
                0, BLUR_RADIUS,
                BLUR_RADIUS, mapSize + BLUR_RADIUS);
        }

        private void FillBiomeMapBorderH (List<Biome> biomes, ref Biome.Type[,] biomeMap,
            int xBase, int zBase, int xMin, int xMax, int yMin, int yMax)
        {
            if (xMin - xMax < BIOME_APPROX)
            {
                for (int i = xMin; i < xMax; i++)
                {
                    for (int j = yMin; j < yMax; j++)
                    {
                        FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, i, j);
                    }
                }
            }
            else
            {
                FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, xMin, yMin);
                FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, xMin, yMax - 1);

                // if both first different, do not continue
                if (biomeMap[xMin, yMin] != biomeMap[xMin, yMax - 1])
                {
                    FillBiomeMapBorderH (biomes, ref biomeMap, xBase, zBase, xMin, xMin + BIOME_APPROX, yMin, yMax);
                    if (xMin + BIOME_APPROX < xMax)
                    {
                        FillBiomeMapBorderH (biomes, ref biomeMap, xBase, zBase, xMin + BIOME_APPROX, xMax, yMin, yMax);
                    }
                }
                // continue as they are identical
                else
                {
                    int max = xMin + BIOME_APPROX;
                    bool identical = true;

                    while (identical && max < xMax + BIOME_APPROX - 1)
                    {
                        if (max >= xMax)
                        {
                            max = xMax - 1;
                        }

                        FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, max, yMin);
                        FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, max, yMax - 1);

                        if (biomeMap[xMin, yMin] != biomeMap[max, yMin] || biomeMap[xMin, yMin] != biomeMap[max, yMax - 1])
                        {
                            identical = false;
                        }
                        else
                        {
                            max += BIOME_APPROX;
                        }
                    }

                    if (max == xMax - 1)
                    {
                        max = xMax;
                    }

                    if (!identical)
                    {
                        max -= BIOME_APPROX;
                    }

                    for (int i = xMin; i < max; i++)
                    {
                        for (int j = yMin; j < yMax; j++)
                        {
                            biomeMap[i, j] = biomeMap[xMin, yMin];
                        }
                    }

                    FillBiomeMapBorderH (biomes, ref biomeMap, xBase, zBase, max, xMax, yMin, yMax);
                }
            }
        }

        private void FillBiomeMapBorderV (List<Biome> biomes, ref Biome.Type[,] biomeMap,
            int xBase, int zBase, int xMin, int xMax, int yMin, int yMax)
        {
            if (yMin - yMax < BIOME_APPROX)
            {
                for (int i = xMin; i < xMax; i++)
                {
                    for (int j = yMin; j < yMax; j++)
                    {
                        FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, i, j);
                    }
                }
            }
            else
            {
                FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, xMin, yMin);
                FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, xMax - 1, yMin);

                // if both first different, do not continue
                if (biomeMap[xMin, yMin] != biomeMap[xMax - 1, yMin])
                {
                    FillBiomeMapBorderV (biomes, ref biomeMap, xBase, zBase, xMin, xMax, yMin, yMin + BIOME_APPROX);
                    if (yMin + BIOME_APPROX < yMax)
                    {
                        FillBiomeMapBorderV (biomes, ref biomeMap, xBase, zBase, xMin, xMax, yMin + BIOME_APPROX, yMax);
                    }
                }
                // continue as they are identical
                else
                {
                    int max = yMin + BIOME_APPROX;
                    bool identical = true;

                    while (identical && max < yMax + BIOME_APPROX - 1)
                    {
                        if (max >= yMax)
                        {
                            max = yMax - 1;
                        }

                        FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, xMin, max);
                        FillBiomeMapElement (biomes, ref biomeMap, xBase, zBase, xMax - 1, max);

                        if (biomeMap[xMin, yMin] != biomeMap[xMin, max] || biomeMap[xMin, yMin] != biomeMap[xMax - 1, max])
                        {
                            identical = false;
                        }
                        else
                        {
                            max += BIOME_APPROX;
                        }
                    }

                    if (max == yMax - 1)
                    {
                        max = yMax;
                    }

                    if (!identical)
                    {
                        max -= BIOME_APPROX;
                    }

                    for (int i = xMin; i < xMax; i++)
                    {
                        for (int j = yMin; j < max; j++)
                        {
                            biomeMap[i, j] = biomeMap[xMin, yMin];
                        }
                    }

                    FillBiomeMapBorderV (biomes, ref biomeMap, xBase, zBase, xMin, xMax, max, yMax);
                }
            }
        }

        private void FillBiomeMapElement (List<Biome> biomes, ref Biome.Type[,] biomeMap, int xBase, int zBase, int i, int j)
        {
            if (biomeMap[i, j] == Biome.Type.Undefined)
            {
                float r = (HEIGHT_POINT_PER_UNIT * TERRAIN_WIDTH + 1) / (float)(HEIGHT_POINT_PER_UNIT * TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT);
                biomeMap[i, j] = Biome.GetBiome (biomes, new Vector2 (xBase + (j - BLUR_RADIUS) * r, zBase + (i - BLUR_RADIUS) * r)).type;
            }
        }

        private void FillGroundMap (List<Island> islandList, ref GroundType[,] groundMap, Vector2 terrainPosition)
        {
            foreach (Island island in islandList)
            {
                lock (island)
                {
                    island.GenerateGround1 (ref groundMap, terrainPosition);
                }
            }
        }

        private void PaintMap (ref float[,,] map, ref Biome.Type[,] biomeMap, ref GroundType[,] groundMap)
        {
            int mapSize = HEIGHT_POINT_PER_UNIT * TERRAIN_WIDTH;

            for (int i = 0; i < mapSize; i += 8)
            {
                int xMin = i;
                int xMax = i + 8;
                if (xMax > mapSize)
                {
                    xMax = mapSize;
                }

                for (int j = 0; j < mapSize; j += 8)
                {
                    int yMin = j;
                    int yMax = j + 8;
                    if (yMax > mapSize)
                    {
                        yMax = mapSize;
                    }

                    // if a square of the same type, no blur
                    Biome.Type bType = biomeMap[xMin, yMin];
                    GroundType gType = groundMap[xMin, yMin];

                    bool same = true;

                    if (xMin != 0 && xMax != mapSize && yMin != 0 && yMax != mapSize)
                    {
                        foreach (int u in new int[] { xMin, xMin + BLUR_RADIUS, (xMax + xMin) / 2 + BLUR_RADIUS, xMax + BLUR_RADIUS, xMax + 2 * BLUR_RADIUS })
                        {
                            foreach (int v in new int[] { yMin, yMin + BLUR_RADIUS, (yMax + yMin) / 2 + BLUR_RADIUS, yMax + BLUR_RADIUS, yMax + 2 * BLUR_RADIUS })
                            {
                                if (bType != biomeMap[u, v] || gType != groundMap[u, v])
                                {
                                    same = false;
                                    break;
                                }
                            }
                            if (!same)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        same = false;
                    }

                    if (same)
                    {
                        PaintSquareNoBlur (ref map, xMin, xMax, yMin, yMax, bType, gType);
                    }
                    else
                    {
                        PaintSquareWithBlur (ref map, xMin, xMax, yMin, yMax, ref biomeMap, ref groundMap);
                    }
                }
            }
        }

        private void PaintSquareNoBlur (ref float[,,] map, int iMin, int iMax, int jMin, int jMax, Biome.Type bType, GroundType gType)
        {
            for (int i = iMin; i < iMax; i++)
            {
                for (int j = jMin; j < jMax; j++)
                {
                    map[i, j, GetLayerIndex (gType, bType)] = 1;
                }
            }
        }

        private void PaintSquareWithBlur (ref float[,,] map, int iMin, int iMax, int jMin, int jMax,
            ref Biome.Type[,] biomeMap, ref GroundType[,] groundMap)
        {
            for (int i = iMin; i < iMax; i++)
            {
                for (int j = jMin; j < jMax; j++)
                {
                    GroundType gType = groundMap[i + BLUR_RADIUS, j + BLUR_RADIUS];

                    // if not ground, no blur
                    if (gType == GroundType.Path || gType == GroundType.Top)
                    {
                        map[i, j, GetLayerIndex (gType, biomeMap[i + BLUR_RADIUS, j + BLUR_RADIUS])] = 1;
                    }
                    // Dont paint cliffs (done with blending in Islands)
                    else if (gType != GroundType.Cliff)
                    {
                        // count the ground ones and sum in map
                        int count = 0;
                        int uMin = i;
                        int uMax = i + 2 * BLUR_RADIUS;
                        int vMin = j;
                        int vMax = j + 2 * BLUR_RADIUS;

                        // Border value must be the same than on neighbor biomes
                        // Reduce the width of the blur near edges to get same value.
                        int mapSize = HEIGHT_POINT_PER_UNIT * TERRAIN_WIDTH;

                        if (i >= mapSize - BLUR_RADIUS)
                        {
                            uMin++;
                        }
                        else if (i <= BLUR_RADIUS)
                        {
                            uMax--;
                        }
                        if (j >= mapSize - BLUR_RADIUS)
                        {
                            vMin++;
                        }
                        else if (j <= BLUR_RADIUS)
                        {
                            vMax--;
                        }

                        for (int u = uMin; u <= uMax; u++)
                        {
                            for (int v = vMin; v <= vMax; v++)
                            {
                                gType = groundMap[u, v];
                                if (gType == GroundType.Ground1 || gType == GroundType.Ground2)
                                {
                                    map[i, j, GetLayerIndex (gType, biomeMap[u, v])] += 1;
                                    count++;
                                }
                            }
                        }

                        // divide by count in all ground ones
                        for (int h = 0; h < 2 * NB_BIOME_TYPE; h++)
                        {
                            map[i, j, h] /= count;
                        }
                    }
                }
            }
        }

        // works for 6 biomes
        static public int GetLayerIndex (GroundType type, Biome.Type biome)
        {
            return NB_BIOME_TYPE * (int)type + (int)biome - 1;
        }
    }
}
