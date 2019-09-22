using System;
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

        private float depth = 8f;


        [SerializeField] public ConditionnedTexture[] textures;

        // Loaded tiles dictionary
        Dictionary<Dic2DKey, GameObject> loadedTiles;

        // Islands
        SpatialDictionary<Island> islands;

        // Debug checkBoxed
        public bool createWorldMap = true;
        public bool createWorldMapAndSaveBin = true;
        public bool createWorldMapFromBin = true;

        private void Start()
        {
            if (singleton != null)
            {
                throw new Exception("World manager singleton already exists.");
            }

            singleton = this;

            // Data
            loadedTiles = new Dictionary<Dic2DKey, GameObject>();
            islands = new SpatialDictionary<Island>();


            // DEBUG
            InitList();
            for (int u = -2; u <= 1; u++)
            {
                for (int v = -2; v <= 1; v++)
                {
                    GenerateNewTerrain(u, v);
                }
            }

        }

        // Update is called once per frame
        void Update()
        {
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

        void GenerateNewTerrain(int xBase, int zBase)
        {
            // Terrain data
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = WIDTH * HEIGHT_POINT_PER_UNIT + 1;
            terrainData.alphamapResolution = WIDTH * HEIGHT_POINT_PER_UNIT;
            terrainData.baseMapResolution = 1024;
            terrainData.SetDetailResolution(1024, 16);
            terrainData.terrainLayers = terrainLayers;
            terrainData.size = new Vector3(WIDTH, depth, WIDTH);

            // Game objet creation
            GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
            terrainGO.name = "Terrain_" + xBase + "_" + zBase;
            terrainGO.transform.position = new Vector3(128 * xBase, 0, 128 * zBase);

            // layer and tag
            terrainGO.tag = "Terrain";
            terrainGO.layer = 9;

            // add to tile list
            loadedTiles.Add(new Dic2DKey(xBase, zBase), terrainGO);

            // Generate islands and textures
            terrainGO.GetComponent<Terrain>().terrainData = GenerateData(terrainData, xBase * WIDTH, zBase * WIDTH);
        }

        private TerrainData GenerateData(TerrainData terrainData, float xBase, float zBase)
        {
            float[,] heights = GenerateFlat();
            List<Vector2Int> slopePoints = new List<Vector2Int>();

            // Look for islands to be added
            List<Island> islandList = islands.GetAround(
                    (int)xBase + WIDTH / 2,
                    (int)zBase + WIDTH / 2,
                    WIDTH / 2 + (int)((Island.MAX_POSSIBLE_RADIUS + 1) * Island.SCALE * 2)
                );
            foreach (Island island in islandList)
            {
                island.GenerateIslandAndHeights(
                    ref heights,
                    new Vector2(xBase, zBase),
                    ref slopePoints);
            }

            terrainData.SetHeights(0, 0, heights);

            // Textures
            terrainData = GenerateAlphaMaps(terrainData, ref slopePoints);

            return terrainData;
        }

        private float[,] GenerateFlat()
        {
            float[,] heights = new float[WIDTH * HEIGHT_POINT_PER_UNIT + 1, WIDTH * HEIGHT_POINT_PER_UNIT + 1];
            for (int i = 0; i < WIDTH * HEIGHT_POINT_PER_UNIT + 1; i++)
            {
                for (int j = 0; j < WIDTH * HEIGHT_POINT_PER_UNIT + 1; j++)
                {
                    heights[i, j] = 0f;
                }
            }

            return heights;
        }


        private TerrainData GenerateAlphaMaps(TerrainData terrainData, ref List<Vector2Int> slopePoints)
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
            PaintSlopes(ref map, ref slopePoints);

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
        private void PaintSlopes(ref float[,,] map, ref List<Vector2Int> slopePoints)
        {
            foreach (Vector2Int point in slopePoints)
            {
                for (int i = 0; i < textures.Length; i++)
                {
                    map[point.x, point.y, i] *= .25f;
                }
                map[point.x, point.y, SLOPE_TEXTURE_ID] = .75f;
            }
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

        IEnumerator LoadedTilesCheck()
        {
            while (playerTransform != null)
            {
                Debug.Log(playerTransform.position);
                Vector3 pos = playerTransform.position - new Vector3(WIDTH / 2, 0, WIDTH / 2);

                // Check for the tiles that should be loaded
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
                            GenerateNewTerrain(i, j);
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
    }
}
