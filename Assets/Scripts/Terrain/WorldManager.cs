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
        private const float ISLAND_RADIUS = 2.3f; // TODO --> new shapes of islands
        public const int TERRAIN_WIDTH = 128;
        private const float DEPTH = 8f;
        public const int HEIGHT_POINT_PER_UNIT = 2;

        public const int SLOPE_TEXTURE_ID = 2;

        // Loading distances
        private const float MIN_LOADED_TILE_DISTANCE = 192;
        private const float MAX_LOADED_TILE_DISTANCE = 384;
        private const float MIN_GENERATED_REGION_DISTANCE = 384;

        public static WorldManager singleton; // Singleton

        private Transform playerTransform;

        // Terrain painting
        [SerializeField] private TerrainLayer[] terrainLayers = null;
        [SerializeField] public ConditionnedTexture[] textures;

        // Nav mesh
        [SerializeField] private NavMeshSurface navMeshSurface = null;
        private List<NavMeshBuildSource> navSources;
        private NavMeshBuildSettings navMeshBuildSettings;

        // Tiles dictionary
        Dictionary<Dic2DKey, GameObject> loadedTiles;

        // Biomes, islands
        SpatialDictionary<Biome> biomes;
        SpatialDictionary<Island> islands;
        WorldCreator wc;

        // Debug checkBoxed
        public bool createWorldMap = true;
        public bool createWorldMapAndSaveBin = true;
        public bool createWorldMapFromBin = true;

        // Thread messages
        Dictionary<Dic2DKey, float[,]> heightsToAdd;
        Dictionary<Dic2DKey, float[,,]> mapsToAdd;
        int newTilesExpected = 0;
        int tileWorkDone = 0;
        object tileCounterLock = new object();
        object regionBoolLock = new object();
        object biomeIslandLock = new object();
        bool newRegionsExpected = false;
        bool regionWorkDone = false;

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

            // World Data
            InitWorldData();

            // Init 4 first tiles
            for (int u = -1; u <= 0; u++)
            {
                for (int v = -1; v <= 0; v++)
                {
                    GenerateTerrainData(u, v, out float[,] heights, out float[,,] map);
                    GenerateNewTerrain(u, v, heights, map);
                }
            }

            // Nav mesh
            CreateNavMesh();
        }

        // Update is called once per frame
        void Update()
        {
            if (newTilesExpected > 0 && tileWorkDone > 0)
            {
                lock (heightsToAdd)
                {
                    foreach (KeyValuePair<Dic2DKey, float[,]> pair in heightsToAdd)
                    {
                        GenerateNewTerrain(pair.Key.x, pair.Key.y, pair.Value, mapsToAdd[pair.Key]);

                        lock(tileCounterLock)
                        {
                            tileWorkDone--;
                            newTilesExpected--;
                        }
                    }
                    heightsToAdd.Clear();
                    mapsToAdd.Clear();
                }

                // if all work done, bake nav mesh
                if (newTilesExpected == 0)
                {
                    UpdateMesh();
                }
            }

            if (newRegionsExpected && regionWorkDone)
            {
                // TODO

                newRegionsExpected = false;
                regionWorkDone = false;
            }
        }

        private void CreateNavMesh()
        {
            navSources = new List<NavMeshBuildSource>();
            navMeshBuildSettings = new NavMeshBuildSettings();
            navMeshBuildSettings.agentClimb = .4f;
            navMeshBuildSettings.agentHeight = 7;
            navMeshBuildSettings.agentRadius = 2.3f;
            navMeshBuildSettings.agentSlope = 14;
            navMeshBuildSettings.agentTypeID = 1;
            navMeshSurface.BuildNavMesh();
        }

        private void UpdateMesh()
        {
            navSources.Clear();

            foreach (GameObject go in loadedTiles.Values)
            {
                Terrain t = go.GetComponent<Terrain>();
                var s = new NavMeshBuildSource();
                s.shape = NavMeshBuildSourceShape.Terrain;
                s.sourceObject = t.terrainData;
                // Terrain system only supports translation - so we pass translation only to back-end
                s.transform = Matrix4x4.TRS(go.transform.position, Quaternion.identity, Vector3.one);
                s.area = 0;
                navSources.Add(s);
            }

            NavMeshBuilder.UpdateNavMeshDataAsync(
                    navMeshSurface.navMeshData,
                    navMeshBuildSettings,
                    navSources,
                    new Bounds(playerTransform.position, new Vector3(768, 768, 768))
                );
        }

        void InitWorldData()
        {
            wc = new WorldCreator(Application.persistentDataPath + "/");
            wc.LoadData(out biomes, out islands);

            if (biomes == null || islands == null)
            {
                biomes = new SpatialDictionary<Biome>();
                islands = new SpatialDictionary<Island>();
                wc.CreateMapSector(ref biomes, ref islands, 0, 0);
            }
        }

        void GenerateNewTerrain(int xBase, int zBase, float[,] heights, float[,,] map)
        {
            // Terrain data
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT + 1;
            terrainData.alphamapResolution = TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT;
            terrainData.baseMapResolution = 1024;
            terrainData.SetDetailResolution(1024, 16);
            terrainData.terrainLayers = terrainLayers;
            terrainData.size = new Vector3(TERRAIN_WIDTH, DEPTH, TERRAIN_WIDTH);

            terrainData.SetHeights(0, 0, heights);
            terrainData.SetAlphamaps(0, 0, map);

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

            heights = new float[TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT + 1, TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT + 1];
            List<Vector2Int> slopePoints = new List<Vector2Int>();

            List<Island> islandList;

            // Look for islands to be added
            lock (biomeIslandLock)
            {
                islandList = islands.GetAround(
                    xBase + TERRAIN_WIDTH / 2,
                    zBase + TERRAIN_WIDTH / 2,
                    TERRAIN_WIDTH / 2 + (int)((Island.MAX_POSSIBLE_RADIUS + 1) * Island.SCALE * 2)
                );
            }

            foreach (Island island in islandList)
            {
                lock(island)
                {
                    island.GenerateIslandAndHeights(
                        ref heights,
                        new Vector2(xBase, zBase),
                        ref slopePoints);
                }
            }

            // Textures. Initialized with zeros
            map = new float[TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT, TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT, 3];

            // for all positions in the alpha maps
            for (int x = 0; x < TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT; x++)
            {
                for (int y = 0; y < TERRAIN_WIDTH * HEIGHT_POINT_PER_UNIT; y++)
                {
                    foreach (ConditionnedTexture texture in textures)
                    {
                        // does it fit?
                        if (texture.Fits(heights[x, y]))
                        {
                            // Write a 1 into the alpha map 
                            map[x, y, texture.groundTexIndex] = 1;
                        }
                    }
                }
            }

            // Paint slopes
            foreach (Vector2Int point in slopePoints)
            {
                for (int i = 0; i < textures.Length; i++)
                {
                    map[point.x, point.y, i] *= .25f;
                }
                map[point.x, point.y, SLOPE_TEXTURE_ID] = .75f;
            }
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
                    if (Mathf.Max(Mathf.Abs(key.x * TERRAIN_WIDTH - pos.x), Mathf.Abs(key.y * TERRAIN_WIDTH - pos.z)) > MAX_LOADED_TILE_DISTANCE)
                    {
                        keyToRemove.Add(key);
                    }
                }

                foreach (Dic2DKey key in keyToRemove)
                {
                    Destroy(loadedTiles[key]);
                    loadedTiles.Remove(key);
                }

                yield return new WaitForSeconds(5);
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
                            if (islands.IsEmpty(i, j, wc.generationSquareRadius))
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

                yield return new WaitForSeconds(4.77f);
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

            lock (heightsToAdd)
            {
                heightsToAdd.Add(new Dic2DKey(xBase, zBase), heights);
                mapsToAdd.Add(new Dic2DKey(xBase, zBase), map);
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
