﻿using LightBringer.Scenery;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

namespace LightBringer.TerrainGeneration
{
    public class WorldManager : MonoBehaviour
    {
        public const int TERRAIN_WIDTH = 128;
        private const float DEPTH = 8f;
        public const int HEIGHT_POINT_PER_UNIT = 2;

        public const int SLOPE_TEXTURE_ID = 2;

        // Loading distances
        private const float MIN_LOADED_TILE_DISTANCE = 192;
        private const float MAX_LOADED_TILE_DISTANCE = 384;
        private const float MIN_GENERATED_REGION_DISTANCE = 384;

        // Map painting
        public const int NB_BIOME_TYPE = 6;
        public const int NB_GROUND_TYPE = 5;
        private const int BIOME_APPROX = 8;

        // Scenery density
        private const int NB_BEACON_PER_TURN = 12;

        // Blur
        public const int BLUR_RADIUS = 6;

        public static WorldManager singleton; // Singleton

        private Transform playerTransform;

        // Terrain painting
        [SerializeField] private TerrainLayer[] terrainLayers = null;

        // Nav mesh
        [SerializeField] private NavMeshSurface navMeshSurface = null;

        // Tiles dictionary
        private Dictionary<Dic2DKey, GameObject> loadedTiles;

        // Biomes, islands
        private SpatialDictionary<Biome> biomes;
        private SpatialDictionary<Island> islands;
        private WorldCreator wc;

        // Scenery
        private SpatialDictionary<SceneryElement> sceneryElements;

        // Thread messages
        private Dictionary<Dic2DKey, float[,]> heightsToAdd;
        private Dictionary<Dic2DKey, float[,,]> mapsToAdd;
        private Dictionary<Dic2DKey, List<SceneryElement>> sceneryElementsToAdd;
        private int newTilesExpected = 0;
        private int tileWorkDone = 0;
        private readonly object tileCounterLock = new object();
        private readonly object regionBoolLock = new object();
        private readonly object biomeIslandLock = new object();
        private bool newRegionsExpected = false;
        private bool regionWorkDone = false;

        private void Start()
        {
            if (singleton != null)
            {
                throw new Exception("World manager singleton already exists.");
            }

            singleton = this;

            // Tile Data
            loadedTiles = new Dictionary<Dic2DKey, GameObject>();
            heightsToAdd = new Dictionary<Dic2DKey, float[,]>();
            mapsToAdd = new Dictionary<Dic2DKey, float[,,]>();
            sceneryElementsToAdd = new Dictionary<Dic2DKey, List<SceneryElement>>();

            // World Data
            InitWorldData();
            //InitDebugWorldData();

            // Init 4 first tiles
            InitFirstTiles();

            // Nav mesh
            CreateNavMesh();
        }

        private void InitFirstTiles()
        {
            for (int u = -1; u <= 0; u++)
            {
                for (int v = -1; v <= 0; v++)
                {
                    GenerateTerrainData(u, v, out float[,] heights, out float[,,] map);
                    sceneryElementsToAdd.Add(new Dic2DKey(u, v), GenerateTileScenery(u, v));
                    AddSceneryElements();
                    GenerateNewTerrain(u, v, heights, map);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (newTilesExpected > 0 && tileWorkDone > 0)
            {
                lock (heightsToAdd)
                {
                    Dic2DKey key = (new List<Dic2DKey>(heightsToAdd.Keys))[0];
                    GenerateNewTerrain(key.x, key.y, heightsToAdd[key], mapsToAdd[key]);

                    lock (tileCounterLock)
                    {
                        tileWorkDone--;
                        newTilesExpected--;
                    }

                    heightsToAdd.Remove(key);
                    mapsToAdd.Remove(key);
                }

                // if all work done, bake nav mesh
                if (newTilesExpected == 0)
                {
                    UpdateMesh();
                }
            }

            if (newRegionsExpected && regionWorkDone)
            {
                newRegionsExpected = false;
                regionWorkDone = false;
            }

            if (sceneryElementsToAdd.Count > 0)
            {
                AddSceneryElements();
            }
        }

        private void CreateNavMesh()
        {
            navMeshSurface.BuildNavMesh();
        }

        private void UpdateMesh()
        {
            List<NavMeshBuildSource> navSources = new List<NavMeshBuildSource>();

            foreach (GameObject go in loadedTiles.Values)
            {
                Terrain t = go.GetComponent<Terrain>();
                var s = new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Terrain,
                    sourceObject = t.terrainData,
                    transform = go.transform.localToWorldMatrix,
                    area = 0
                };
                navSources.Add(s);
            }

            NavMeshBuilder.UpdateNavMeshDataAsync(
                    navMeshSurface.navMeshData,
                    navMeshSurface.GetBuildSettings(),
                    navSources,
                    new Bounds(playerTransform.position, new Vector3(2000, 2000, 2000))
                );
        }

        private void InitWorldData()
        {
            wc = new WorldCreator(Application.persistentDataPath + "/");
            wc.LoadData(out biomes, out islands, out sceneryElements);

            if (biomes == null || islands == null || sceneryElements == null)
            {
                biomes = new SpatialDictionary<Biome>();
                islands = new SpatialDictionary<Island>();
                sceneryElements = new SpatialDictionary<SceneryElement>();
                wc.CreateMapSector(ref biomes, ref islands, 0, 0);
            }
        }
        /*
        void InitDebugWorldData ()
        {
            wc = new WorldCreator (Application.persistentDataPath + "/");

            biomes = new SpatialDictionary<Biome> ();
            islands = new SpatialDictionary<Island> ();

            Biome b = new Biome (0, 0);
            b.type = Biome.Type.Light;
            biomes.Add (0, 0, b);
            b = new Biome (-500, 0);
            b.type = Biome.Type.Darkness;
            biomes.Add (-500, 0, b);
            b = new Biome (1280, 1280);
            b.type = Biome.Type.Earth;
            biomes.Add (1280, 1280, b);
            b = new Biome (1280, -1280);
            b.type = Biome.Type.Fire;
            biomes.Add (1280, -1280, b);

            islands.Add (0, 0, new Island (new Vector2 (0, 0), Biome.Type.Light, 1, 5));
        }
        */

        void GenerateNewTerrain(int xBase, int zBase, float[,] heights, float[,,] map)
        {
            // Terrain data
            TerrainData terrainData = new TerrainData
            {
                heightmapResolution = TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT + 1,
                alphamapResolution = TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT,
                baseMapResolution = TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT,
                terrainLayers = terrainLayers,
                size = new Vector3(TERRAIN_WIDTH, DEPTH, TERRAIN_WIDTH)
            };
            terrainData.SetDetailResolution(1024, 16);
            terrainData.SetHeights(0, 0, heights);
            terrainData.SetAlphamaps(0, 0, map);
            terrainData.SetDetailResolution(128, 128);

            // Game objet creation
            GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
            terrainGO.name = "Terrain_" + xBase + "_" + zBase;
            terrainGO.transform.position = new Vector3(TERRAIN_WIDTH * xBase, 0, TERRAIN_WIDTH * zBase);

            // layer and tag
            terrainGO.tag = "Terrain";
            terrainGO.layer = 9;

            // add to tile list
            loadedTiles.Add(new Dic2DKey(xBase, zBase), terrainGO);
        }

        private void GenerateTerrainData(int xBase, int zBase, out float[,] heights, out float[,,] map)
        {
            xBase *= TERRAIN_WIDTH;
            zBase *= TERRAIN_WIDTH;

            // Find impacting biomes
            List<Biome> impactingBiomes = new List<Biome>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    foreach (Dic2DKey key in Biome.Get4ClosestBiomes(biomes,
                        new Vector2(xBase + i * TERRAIN_WIDTH / 2, zBase + j * TERRAIN_WIDTH / 2), out _))
                    {
                        Biome biome = biomes.Get(key);
                        if (!impactingBiomes.Contains(biome))
                        {
                            impactingBiomes.Add(biome);
                        }
                    }
                }
            }

            int mapSize = HEIGHT_POINT_PER_UNIT * TERRAIN_WIDTH;
            map = new float[mapSize, mapSize, NB_GROUND_TYPE * NB_BIOME_TYPE];
            heights = new float[mapSize + 1, mapSize + 1];

            Biome.Type[,] biomeMap = new Biome.Type[mapSize + 2 * BLUR_RADIUS, mapSize + 2 * BLUR_RADIUS];
            GroundType[,] groundMap = new GroundType[mapSize + 2 * BLUR_RADIUS, mapSize + 2 * BLUR_RADIUS];

            FillBiomeMapBorder(impactingBiomes, ref biomeMap, xBase, zBase);
            FillBiomeMap(impactingBiomes, ref biomeMap, xBase, zBase, mapSize, BLUR_RADIUS, BLUR_RADIUS);

            List<Island> islandList;

            // Look for islands to be added
            lock (biomeIslandLock)
            {
                islandList = islands.GetAround(
                    xBase + TERRAIN_WIDTH / 2,
                    zBase + TERRAIN_WIDTH / 2,
                    TERRAIN_WIDTH / 2 + (int)((Island.MAX_RADIUS + 1) * Island.SCALE * 3)
                );
            }

            Vector2 terrainPosition = new Vector2(xBase, zBase);

            FillGroundMap(islandList, ref groundMap, terrainPosition);

            foreach (Island island in islandList)
            {
                lock (island)
                {
                    island.GenerateIslandHeightsAndAlphaMap(
                            ref map,
                            ref heights,
                            ref biomeMap,
                            ref groundMap,
                            terrainPosition
                        );
                }
            }

            PaintMap(ref map, ref biomeMap, ref groundMap);
        }

        private void FillBiomeMap(List<Biome> biomes, ref Biome.Type[,] biomeMap,
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

                        FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, i, j);

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
                    FillBiomeMap(biomes, ref biomeMap, xBase, zBase, size / 2, xStart, yStart);
                    FillBiomeMap(biomes, ref biomeMap, xBase, zBase, size / 2, xStart + size / 2, yStart);
                    FillBiomeMap(biomes, ref biomeMap, xBase, zBase, size / 2, xStart, yStart + size / 2);
                    FillBiomeMap(biomes, ref biomeMap, xBase, zBase, size / 2, xStart + size / 2, yStart + size / 2);
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
                        FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, i, j);
                    }
                }
            }
        }

        private void FillBiomeMapBorder(List<Biome> biomes, ref Biome.Type[,] biomeMap, int xBase, int zBase)
        {
            int mapSize = HEIGHT_POINT_PER_UNIT * TERRAIN_WIDTH;

            FillBiomeMapBorderH(biomes, ref biomeMap, xBase, zBase,
                0, mapSize + 2 * BLUR_RADIUS,
                0, BLUR_RADIUS);
            FillBiomeMapBorderH(biomes, ref biomeMap, xBase, zBase,
                0, mapSize + 2 * BLUR_RADIUS,
                mapSize + BLUR_RADIUS, mapSize + 2 * BLUR_RADIUS);
            FillBiomeMapBorderV(biomes, ref biomeMap, xBase, zBase,
                mapSize + BLUR_RADIUS, mapSize + 2 * BLUR_RADIUS,
                BLUR_RADIUS, mapSize + BLUR_RADIUS);
            FillBiomeMapBorderV(biomes, ref biomeMap, xBase, zBase,
                0, BLUR_RADIUS,
                BLUR_RADIUS, mapSize + BLUR_RADIUS);
        }

        private void FillBiomeMapBorderH(List<Biome> biomes, ref Biome.Type[,] biomeMap,
            int xBase, int zBase, int xMin, int xMax, int yMin, int yMax)
        {
            if (xMin - xMax < BIOME_APPROX)
            {
                for (int i = xMin; i < xMax; i++)
                {
                    for (int j = yMin; j < yMax; j++)
                    {
                        FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, i, j);
                    }
                }
            }
            else
            {
                FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, xMin, yMin);
                FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, xMin, yMax - 1);

                // if both first different, do not continue
                if (biomeMap[xMin, yMin] != biomeMap[xMin, yMax - 1])
                {
                    FillBiomeMapBorderH(biomes, ref biomeMap, xBase, zBase, xMin, xMin + BIOME_APPROX, yMin, yMax);
                    if (xMin + BIOME_APPROX < xMax)
                    {
                        FillBiomeMapBorderH(biomes, ref biomeMap, xBase, zBase, xMin + BIOME_APPROX, xMax, yMin, yMax);
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

                        FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, max, yMin);
                        FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, max, yMax - 1);

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

                    FillBiomeMapBorderH(biomes, ref biomeMap, xBase, zBase, max, xMax, yMin, yMax);
                }
            }
        }

        private void FillBiomeMapBorderV(List<Biome> biomes, ref Biome.Type[,] biomeMap,
            int xBase, int zBase, int xMin, int xMax, int yMin, int yMax)
        {
            if (yMin - yMax < BIOME_APPROX)
            {
                for (int i = xMin; i < xMax; i++)
                {
                    for (int j = yMin; j < yMax; j++)
                    {
                        FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, i, j);
                    }
                }
            }
            else
            {
                FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, xMin, yMin);
                FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, xMax - 1, yMin);

                // if both first different, do not continue
                if (biomeMap[xMin, yMin] != biomeMap[xMax - 1, yMin])
                {
                    FillBiomeMapBorderV(biomes, ref biomeMap, xBase, zBase, xMin, xMax, yMin, yMin + BIOME_APPROX);
                    if (yMin + BIOME_APPROX < yMax)
                    {
                        FillBiomeMapBorderV(biomes, ref biomeMap, xBase, zBase, xMin, xMax, yMin + BIOME_APPROX, yMax);
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

                        FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, xMin, max);
                        FillBiomeMapElement(biomes, ref biomeMap, xBase, zBase, xMax - 1, max);

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

                    FillBiomeMapBorderV(biomes, ref biomeMap, xBase, zBase, xMin, xMax, max, yMax);
                }
            }
        }

        private void FillBiomeMapElement(List<Biome> biomes, ref Biome.Type[,] biomeMap, int xBase, int zBase, int i, int j)
        {
            if (biomeMap[i, j] == Biome.Type.Undefined)
            {
                float r = (HEIGHT_POINT_PER_UNIT * TERRAIN_WIDTH + 1) / (float)(HEIGHT_POINT_PER_UNIT * TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT);
                biomeMap[i, j] = Biome.GetBiome(biomes, new Vector2(xBase + (j - BLUR_RADIUS) * r, zBase + (i - BLUR_RADIUS) * r)).type;
            }
        }

        private void FillGroundMap(List<Island> islandList, ref GroundType[,] groundMap, Vector2 terrainPosition)
        {
            foreach (Island island in islandList)
            {
                lock (island)
                {
                    island.GenerateGround1(ref groundMap, terrainPosition);
                }
            }
        }

        private void PaintMap(ref float[,,] map, ref Biome.Type[,] biomeMap, ref GroundType[,] groundMap)
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
                        PaintSquareNoBlur(ref map, xMin, xMax, yMin, yMax, bType, gType);
                    }
                    else
                    {
                        PaintSquareWithBlur(ref map, xMin, xMax, yMin, yMax, ref biomeMap, ref groundMap);
                    }
                }
            }
        }

        private void PaintSquareNoBlur(ref float[,,] map, int iMin, int iMax, int jMin, int jMax, Biome.Type bType, GroundType gType)
        {
            for (int i = iMin; i < iMax; i++)
            {
                for (int j = jMin; j < jMax; j++)
                {
                    map[i, j, GetLayerIndex(gType, bType)] = 1;
                }
            }
        }

        private void PaintSquareWithBlur(ref float[,,] map, int iMin, int iMax, int jMin, int jMax,
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
                        map[i, j, GetLayerIndex(gType, biomeMap[i + BLUR_RADIUS, j + BLUR_RADIUS])] = 1;
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
                                    map[i, j, GetLayerIndex(gType, biomeMap[u, v])] += 1;
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

        private List<SceneryElement> GenerateTileScenery(int xBase, int zBase)
        {
            xBase *= TERRAIN_WIDTH;
            zBase *= TERRAIN_WIDTH;

            List<SceneryElement> elements;

            lock (sceneryElements)
            {
                // if not in memory, generate it
                if (sceneryElements.IsEmpty(xBase + TERRAIN_WIDTH / 2, zBase + TERRAIN_WIDTH / 2, TERRAIN_WIDTH / 4))
                {
                    Debug.Log("Generate for " + xBase + " - " + zBase);
                    elements = new List<SceneryElement>();

                    // Look for islands to be added
                    List<Island> islandList;
                    lock (biomeIslandLock)
                    {
                        islandList = islands.GetAround(
                            xBase + TERRAIN_WIDTH / 2,
                            zBase + TERRAIN_WIDTH / 2,
                            TERRAIN_WIDTH / 2 + (int)((Island.MAX_RADIUS + 1) * Island.SCALE * 3)
                        );
                    }

                    Vector2 terrainPosition = new Vector2(xBase, zBase);
                    foreach (Island island in islandList)
                    {
                        Vector3 islandPosition = new Vector3(island.centerInWorld.x + 3, 0, island.centerInWorld.y + 3);

                        // Generate beacons
                        for (float angle = 0; angle < 2 * (float)Math.PI; angle += 2 * (float)Math.PI / NB_BEACON_PER_TURN)
                        {
                            float dist = island.Ground1Dist(angle);

                            float convertedAngle = -angle / 2f / (float)Math.PI * 360f - 90;

                            Vector3 position = islandPosition +
                                Quaternion.AngleAxis(convertedAngle, Vector3.up) * Vector3.forward * dist * Island.SCALE;

                            if (position.x >= xBase && position.x < xBase + TERRAIN_WIDTH
                                && position.z >= zBase && position.z < zBase + TERRAIN_WIDTH)
                            {
                                bool outsideEveryG1 = true;
                                foreach (Island isl in islandList)
                                {
                                    if (isl == island)
                                    {
                                        continue;
                                    }

                                    outsideEveryG1 = !isl.WorldIsInGround1(position);
                                    if (!outsideEveryG1)
                                    {
                                        break;
                                    }
                                }

                                if (outsideEveryG1)
                                {
                                    SceneryElement element = new SceneryElement(position, island.biomeType, SceneryElement.type.beacon);
                                    elements.Add(element);
                                    sceneryElements.Add((int)position.x, (int)position.z, element);
                                }
                            }
                        }
                    }
                }

                // If scenery exists, load it
                else
                {
                    Debug.Log("Load for " + xBase + " - " + zBase);
                    elements = sceneryElements.GetAround(
                        xBase + TERRAIN_WIDTH / 2,
                        zBase + TERRAIN_WIDTH / 2,
                        TERRAIN_WIDTH / 2
                    );
                }
            }

            return elements;
        }

        private void AddSceneryElements()
        {
            int counter = 0;
            lock (sceneryElementsToAdd)
            {
                List<Dic2DKey> keys = new List<Dic2DKey>(sceneryElementsToAdd.Keys);

                // load the scenery element in the terrain GO (if already created, else wait for it)
                // max 5 objects per frame
                while (keys.Count > 0 && counter < 5 && sceneryElementsToAdd[keys[0]].Count > 0 && loadedTiles.ContainsKey(keys[0]))
                {
                    SceneryElement element = sceneryElementsToAdd[keys[0]][0];
                    GameObject prefab = Resources.Load(element.PrefabPath()) as GameObject;
                    GameObject go = Instantiate(prefab, loadedTiles[keys[0]].transform);
                    go.transform.position = element.position;

                    sceneryElementsToAdd[keys[0]].RemoveAt(0);
                    if (sceneryElementsToAdd[keys[0]].Count == 0)
                    {
                        sceneryElementsToAdd.Remove(keys[0]);
                        keys = new List<Dic2DKey>(sceneryElementsToAdd.Keys);
                    }
                    counter += 1;
                }
            }
        }

        // works for 6 biomes
        static public int GetLayerIndex(GroundType type, Biome.Type biome)
        {
            return NB_BIOME_TYPE * (int)type + (int)biome - 1;
        }

        public void SetPlayerTransform(Transform t)
        {
            playerTransform = t;
            StartCoroutine(LoadedTilesCheck());
            StartCoroutine(GeneratedRegionsCheck());
        }

        /*
         * Coroutines
         */
        #region Coroutines
        IEnumerator LoadedTilesCheck()
        {
            while (playerTransform != null)
            {
                Vector3 pos = playerTransform.position - new Vector3(TERRAIN_WIDTH / 2, 0, TERRAIN_WIDTH / 2);

                // Check for the tiles that should be loaded
                if (newTilesExpected == 0)
                {
                    for (
                           int i = (int)((pos.x - MIN_LOADED_TILE_DISTANCE) / TERRAIN_WIDTH);
                           i * TERRAIN_WIDTH - pos.x <= MIN_LOADED_TILE_DISTANCE;
                           i++
                       )
                    {
                        for (
                            int j = (int)((pos.z - MIN_LOADED_TILE_DISTANCE) / TERRAIN_WIDTH);
                            j * TERRAIN_WIDTH - pos.z <= MIN_LOADED_TILE_DISTANCE;
                            j++
                        )
                        {
                            if (!loadedTiles.ContainsKey(new Dic2DKey(i, j)))
                            {
                                // Call for thread
                                lock (tileCounterLock)
                                {
                                    newTilesExpected++;
                                }

                                ThreadPool.QueueUserWorkItem(GenerateTerrainDataThreaded, new object[] { i, j });
                            }
                        }
                    }
                }

                // Check for tiles that should be unloaded
                List<Dic2DKey> keyToRemove = new List<Dic2DKey>();

                foreach (Dic2DKey key in loadedTiles.Keys)
                {
                    if (Mathf.Max(
                            Mathf.Abs(key.x * TERRAIN_WIDTH - pos.x),
                            Mathf.Abs(key.y * TERRAIN_WIDTH - pos.z)
                        ) > MAX_LOADED_TILE_DISTANCE)
                    {
                        keyToRemove.Add(key);
                    }
                }

                foreach (Dic2DKey key in keyToRemove)
                {
                    Destroy(loadedTiles[key]);
                    loadedTiles.Remove(key);
                }

                yield return new WaitForSeconds(3);
            }
        }

        IEnumerator GeneratedRegionsCheck()
        {
            while (playerTransform != null)
            {
                Vector3 pos = playerTransform.position;

                // Check for the tiles that should be loaded
                if (!newRegionsExpected)
                {
                    List<int[]> regions = new List<int[]>();

                    for (
                           int i = (int)Math.Round((pos.x - MIN_GENERATED_REGION_DISTANCE) / (2 * wc.generationSquareRadius))
                                    * 2 * wc.generationSquareRadius;
                           i <= pos.x + MIN_GENERATED_REGION_DISTANCE + wc.generationSquareRadius;
                           i += 2 * wc.generationSquareRadius
                       )
                    {
                        for (
                            int j = (int)Math.Round((pos.z - MIN_GENERATED_REGION_DISTANCE) / (2 * wc.generationSquareRadius))
                                    * 2 * wc.generationSquareRadius;
                           j <= pos.z + MIN_GENERATED_REGION_DISTANCE + wc.generationSquareRadius;
                           j += 2 * wc.generationSquareRadius
                        )
                        {
                            if (islands.CountAround(i, j, wc.generationSquareRadius) < wc.avgNbIslandsPerSquare / 4)
                            {
                                regions.Add(new int[] { i, j });
                            }
                        }
                    }

                    if (regions.Count > 0)
                    {
                        lock (regionBoolLock)
                        {
                            newRegionsExpected = true;
                        }
                        // Call for thread
                        ThreadPool.QueueUserWorkItem(GenerateRegionDataThreaded, regions);
                    }
                }

                yield return new WaitForSeconds(2.9999f);
            }
        }
        #endregion

        /*
         * Threads
         */
        #region Threads

        private void GenerateTerrainDataThreaded(object xzBase)
        {
            object[] array = xzBase as object[];
            int xBase = Convert.ToInt32(array[0]);
            int zBase = Convert.ToInt32(array[1]);

            GenerateTerrainData(xBase, zBase, out float[,] heights, out float[,,] map);
            List<SceneryElement> sceneryElements = GenerateTileScenery(xBase, zBase);

            lock (heightsToAdd)
            {
                heightsToAdd.Add(new Dic2DKey(xBase, zBase), heights);
                mapsToAdd.Add(new Dic2DKey(xBase, zBase), map);
            }

            lock (sceneryElementsToAdd)
            {
                sceneryElementsToAdd.Add(new Dic2DKey(xBase, zBase), sceneryElements);
            }

            lock (tileCounterLock)
            {
                tileWorkDone++;
            }
        }

        private void GenerateRegionDataThreaded(object regions)
        {
            List<int[]> coords = regions as List<int[]>;

            SpatialDictionary<Biome> biomesCopy = biomes.Copy();
            SpatialDictionary<Island> islandsCopy = islands.Copy();

            for (int i = 0; i < coords.Count; i++)
            {
                wc.CreateMapSector(ref biomesCopy, ref islandsCopy, coords[i][0], coords[i][1]);
            }

            lock (biomeIslandLock)
            {
                islands = islandsCopy;
                biomes = biomesCopy;
            }

            lock (regionBoolLock)
            {
                regionWorkDone = true;
            }

        }
        #endregion
    }
}
