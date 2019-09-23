using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LightBringer.TerrainGeneration
{
    public class WorldManager : MonoBehaviour
    {
        private const float ISLAND_RADIUS = 2.3f; // TODO --> new shapes of islands
        public const int WIDTH = 128;
        private const float DEPTH = 8f;
        public const int HEIGHT_POINT_PER_UNIT = 2;

        private const int MAX_TRY = 200;

        private const int NUMBER_OF_ISLAND_PER_SQUARE = 50;
        private const float MIN_DISTANCE_BETWEEN_ISLANDS = 70;
        private const int GEN_SQUARE_RADIUS = 512;

        public const int SLOPE_TEXTURE_ID = 2;

        // Loading distances
        private const float MIN_LOADED_TILE_DISTANCE = 192;
        private const float MAX_LOADED_TILE_DISTANCE = 384;


        public static WorldManager singleton; // Singleton

        private Transform playerTransform;

        [SerializeField] private TerrainLayer[] terrainLayers = null;



        [SerializeField] public ConditionnedTexture[] textures;

        // Tiles dictionary
        Dictionary<Dic2DKey, GameObject> loadedTiles;

        // Islands
        SpatialDictionary<Island> islands;

        // Debug checkBoxed
        public bool createWorldMap = true;
        public bool createWorldMapAndSaveBin = true;
        public bool createWorldMapFromBin = true;

        // Thread messages
        Dictionary<Dic2DKey, float[,]> heightsToAdd;
        Dictionary<Dic2DKey, float[,,]> mapsToAdd;
        int newTilesExpected = 0;
        int workDone = 0;
        object counterLock = new object();

        private void Start()
        {
            if (singleton != null)
            {
                throw new Exception("World manager singleton already exists.");
            }

            singleton = this;

            // Data
            loadedTiles = new Dictionary<Dic2DKey, GameObject>();
            heightsToAdd = new Dictionary<Dic2DKey, float[,]>();
            mapsToAdd = new Dictionary<Dic2DKey, float[,,]>();
            islands = new SpatialDictionary<Island>();


            // DEBUG
            InitList();
            for (int u = -2; u <= 1; u++)
            {
                for (int v = -2; v <= 1; v++)
                {
                    GenerateData(u, v, out float[,] heights, out float[,,] map);
                    GenerateNewTerrain(u, v, heights, map);
                }
            }

        }

        // Update is called once per frame
        void Update()
        {
            if (newTilesExpected > 0 && workDone > 0)
            {
                lock (heightsToAdd)
                {
                    foreach (KeyValuePair<Dic2DKey, float[,]> pair in heightsToAdd)
                    {
                        GenerateNewTerrain(pair.Key.x, pair.Key.y, pair.Value, mapsToAdd[pair.Key]);

                        lock(counterLock)
                        {
                            workDone--;
                            newTilesExpected--;
                        }
                    }
                    heightsToAdd.Clear();
                }
            }

            if (!createWorldMap)
            {
                createWorldMap = true;

                SpatialDictionary<Island> islands = new SpatialDictionary<Island>();
                for (int i = -2 * GEN_SQUARE_RADIUS; i <= 2 * GEN_SQUARE_RADIUS; i += 2 * GEN_SQUARE_RADIUS)
                {
                    for (int j = -2 * GEN_SQUARE_RADIUS; j <= 2 * GEN_SQUARE_RADIUS; j += 2 * GEN_SQUARE_RADIUS)
                    {
                        generateIslandsInSquare(ref islands, i, j);
                    }
                }
                MapPainter mp = new MapPainter();
                mp.Draw(ref islands, 0, 0, 3 * GEN_SQUARE_RADIUS);
            }

            if (!createWorldMapAndSaveBin)
            {
                createWorldMapAndSaveBin = true;

                for (int i = -2 * GEN_SQUARE_RADIUS; i <= 2 * GEN_SQUARE_RADIUS; i += 2 * GEN_SQUARE_RADIUS)
                {
                    for (int j = -2 * GEN_SQUARE_RADIUS; j <= 2 * GEN_SQUARE_RADIUS; j += 2 * GEN_SQUARE_RADIUS)
                    {
                        generateIslandsInSquare(ref islands, i, j);
                    }
                }

                // save to binary
                FileStream fs = new FileStream(Application.persistentDataPath + "/islands.dat", FileMode.Create);

                // Binary formatter with vector 2
                BinaryFormatter bf = new BinaryFormatter();
                SurrogateSelector surrogateSelector = new SurrogateSelector();
                Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate();
                surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2SS);
                bf.SurrogateSelector = surrogateSelector;

                try
                {
                    bf.Serialize(fs, islands);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }


            if (!createWorldMapFromBin)
            {
                createWorldMapFromBin = true;

                // load from binary
                FileStream fs = new FileStream(Application.persistentDataPath + "/islands.dat", FileMode.Open);

                // Binary formatter with vector 2
                BinaryFormatter bf = new BinaryFormatter();
                SurrogateSelector surrogateSelector = new SurrogateSelector();
                Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate();
                surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2SS);
                bf.SurrogateSelector = surrogateSelector;

                try
                {
                    SpatialDictionary<Island> islands = (SpatialDictionary<Island>)bf.Deserialize(fs);
                    MapPainter mp = new MapPainter();
                    mp.Draw(ref islands, 0, 0, 3 * GEN_SQUARE_RADIUS);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        void InitList()
        {
            // load from binary
            FileStream fs = new FileStream(Application.persistentDataPath + "/islands.dat", FileMode.Open);

            // Binary formatter with vector 2
            BinaryFormatter bf = new BinaryFormatter();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate();
            surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2SS);
            bf.SurrogateSelector = surrogateSelector;

            try
            {
                islands = (SpatialDictionary<Island>)bf.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        void GenerateNewTerrain(int xBase, int zBase, float[,] heights, float[,,] map)
        {
            Debug.Log("GenerateNewTerrain(" + xBase + ", " + zBase + ")");

            // Terrain data
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = WIDTH * HEIGHT_POINT_PER_UNIT + 1;
            terrainData.alphamapResolution = WIDTH * HEIGHT_POINT_PER_UNIT;
            terrainData.baseMapResolution = 1024;
            terrainData.SetDetailResolution(1024, 16);
            terrainData.terrainLayers = terrainLayers;
            terrainData.size = new Vector3(WIDTH, DEPTH, WIDTH);

            terrainData.SetHeights(0, 0, heights);
            terrainData.SetAlphamaps(0, 0, map);

            // Game objet creation
            GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
            terrainGO.name = "Terrain_" + xBase + "_" + zBase;
            terrainGO.transform.position = new Vector3(WIDTH * xBase, 0, WIDTH * zBase);

            // layer and tag
            terrainGO.tag = "Terrain";
            terrainGO.layer = 9;

            // add to tile list
            loadedTiles.Add(new Dic2DKey(xBase, zBase), terrainGO);
            Debug.Log("GenerateNewTerrain(" + xBase + ", " + zBase + ") done");
        }

        private void GenerateData(int xBase, int zBase, out float[,] heights, out float[,,] map)
        {
            Debug.Log("Generate data: " + xBase + "; " + zBase);

            xBase *= WIDTH;
            zBase *= WIDTH;

            heights = new float[WIDTH * HEIGHT_POINT_PER_UNIT + 1, WIDTH * HEIGHT_POINT_PER_UNIT + 1];
            List<Vector2Int> slopePoints = new List<Vector2Int>();

            
            Debug.Log("Generate data: " + xBase + "; " + zBase + " bis");

            // Look for islands to be added
            List<Island> islandList = islands.GetAround(
                    xBase + WIDTH / 2,
                    zBase + WIDTH / 2,
                    WIDTH / 2 + (int)((Island.MAX_POSSIBLE_RADIUS + 1) * Island.SCALE * 2)
                );


            Debug.Log("Generate data: " + xBase + "; " + zBase + " ter");

            foreach (Island island in islandList)
            {
                Debug.Log("Generate data: " + xBase + "; " + zBase + " quad");

                lock(island)
                {
                    island.GenerateIslandAndHeights(
                        ref heights,
                        new Vector2(xBase, zBase),
                        ref slopePoints);
                }
            }


            // Textures. Initialized with zeros
            map = new float[WIDTH * HEIGHT_POINT_PER_UNIT, WIDTH * HEIGHT_POINT_PER_UNIT, 3];

            // for all positions in the alpha maps
            for (int x = 0; x < WIDTH * HEIGHT_POINT_PER_UNIT; x++)
            {
                for (int y = 0; y < WIDTH * HEIGHT_POINT_PER_UNIT; y++)
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


            Debug.Log("Generate data: " + xBase + "; " + zBase + " done!");
        }

        public void generateIslandsInSquare(ref SpatialDictionary<Island> islands, int xCenter, int yCenter)
        {
            Random.InitState((int)DateTime.Now.Ticks & 0x0000FFFF);

            for (int i = 0; i < NUMBER_OF_ISLAND_PER_SQUARE; i++)
            {
                int tryCount = 0;

                while (tryCount < MAX_TRY)
                {
                    int x = Random.Range(xCenter - GEN_SQUARE_RADIUS, xCenter + GEN_SQUARE_RADIUS - 1);
                    int y = Random.Range(yCenter - GEN_SQUARE_RADIUS, yCenter + GEN_SQUARE_RADIUS - 1);

                    // Rejection
                    if (!IsRejected(ref islands, x, y, ISLAND_RADIUS))
                    {
                        // Add Island
                        Island island = new Island(new Vector2(x, y), ISLAND_RADIUS);
                        islands.Add(x, y, island);
                        break;
                    }

                    tryCount++;
                }
            }
        }

        private bool IsRejected(ref SpatialDictionary<Island> islands, int x, int y, float radius)
        {
            Vector2 islandCenter = new Vector2(x, y);
            float minDistance = MIN_DISTANCE_BETWEEN_ISLANDS + (radius + Island.MAX_POSSIBLE_RADIUS) * Island.SCALE;

            List<Island> possibleCollidings = islands.GetAround(x, y, (int)Mathf.Ceil(minDistance));

            foreach (Island pc in possibleCollidings)
            {
                if ((islandCenter - pc.centerInWorld).magnitude < minDistance)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetPlayerTransform(Transform t)
        {
            playerTransform = t;
            StartCoroutine(LoadedTilesCheck());
        }

        /*
         * Coroutines
         */
        #region Coroutines
        IEnumerator LoadedTilesCheck()
        {
            while (playerTransform != null)
            {
                Debug.Log(playerTransform.position + " tiles exp: " + newTilesExpected);
                Vector3 pos = playerTransform.position - new Vector3(WIDTH / 2, 0, WIDTH / 2);

                // Check for the tiles that should be loaded
                if (newTilesExpected == 0)
                {
                    for (
                           int i = (int)((pos.x - MIN_LOADED_TILE_DISTANCE) / WIDTH);
                           i * WIDTH - pos.x <= MIN_LOADED_TILE_DISTANCE;
                           i++
                       )
                    {
                        for (
                            int j = (int)((pos.z - MIN_LOADED_TILE_DISTANCE) / WIDTH);
                            j * WIDTH - pos.z <= MIN_LOADED_TILE_DISTANCE;
                            j++
                        )
                        {
                            if (!loadedTiles.ContainsKey(new Dic2DKey(i, j)))
                            {
                                // Call for thread
                                lock (counterLock)
                                {
                                    newTilesExpected++;
                                }
                                // DEBUG: pister le thread (plusieurs lancements ?)
                                Debug.Log("Appel thread: " + i + "; " + j);
                                ThreadPool.QueueUserWorkItem(GenerateDataThreaded, new object[] { i, j });
                            }
                        }
                    }
                }

                // TODO check for tiles that should be unloaded
                foreach (Dic2DKey key in loadedTiles.Keys)
                {
                    if (Mathf.Max(Mathf.Abs(key.x * WIDTH - pos.x), Mathf.Abs(key.y * WIDTH - pos.z)) > MAX_LOADED_TILE_DISTANCE)
                    {
                        Destroy(loadedTiles[key]);
                    }
                }

                yield return new WaitForSeconds(5);
            }

        }
        #endregion

        /*
         * Threads
         */
        #region Threads

        private void GenerateDataThreaded(object xzBase)
        {
            object[] array = xzBase as object[];
            int xBase = Convert.ToInt32(array[0]);
            int zBase = Convert.ToInt32(array[1]);

            Debug.Log("Thread pour: " + xBase + "; " + zBase);

            GenerateData(xBase, zBase, out float[,] heights, out float[,,] map);

            Debug.Log("Thread pour: " + xBase + "; " + zBase + " --> Data ok");
            lock (heightsToAdd)
            {
                heightsToAdd.Add(new Dic2DKey((int)xBase, (int)zBase), heights);
                mapsToAdd.Add(new Dic2DKey((int)xBase, (int)zBase), map);
            }

            lock (counterLock)
            {
                workDone++;
            }

            Debug.Log("Fin thread pour: " + xBase + "; " + zBase);
        }

        #endregion
    }
}
