using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LightBringer.TerrainGeneration
{
    [ExecuteInEditMode]
    public class WorldCreator : MonoBehaviour
    {
        // Biome constants
        private const int BIOME_GEN_SQUARE_RADIUS = 2048;
        private const int NUMBER_OF_BIOMES_PER_SQUARE = 150;
        public const float MIN_DISTANCE_BETWEEN_BIOMES_POLY = 150;
        private const int BIOME_MAX_TRY = 500;


        // Island constants
        private const float ISLAND_RADIUS = 2.3f; // TODO --> new shapes of islands
        private const int ISLAND_GEN_SQUARE_RADIUS = 512;
        private const int NUMBER_OF_ISLANDS_PER_SQUARE = 150;
        private const float MIN_DISTANCE_BETWEEN_ISLANDS = 40;
        private const int ISLAND_MAX_TRY = 200;

        // Loading distances
        private const float MIN_LOADED_TILE_DISTANCE = 192;
        private const float MAX_LOADED_TILE_DISTANCE = 384;

        // Debug checkBoxed
        public bool createWorldMap = true;
        public bool createWorldMapAndSaveBin = true;
        public bool createWorldMapFromBin = true;

        public bool createBiomeMapAndSaveBin = true;

        // Update is called once per frame
        void Update()
        {
            if (!createWorldMap)
            {
                createWorldMap = true;

                CreateAndPrintMap();
            }

            if (!createWorldMapAndSaveBin)
            {
                createWorldMapAndSaveBin = true;

                CreateWorldAndSaveToBinary();
            }

            if (!createWorldMapFromBin)
            {
                createWorldMapFromBin = true;

                LoadAndPrintMap();
            }

            if (!createBiomeMapAndSaveBin)
            {
                createBiomeMapAndSaveBin = true;

                CreateAndPrintBiomeMap();

                /*
                Biome biome = new Biome(new Vector2(0, 0));

                int mapRadius = 512;

                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(mapRadius * 2, mapRadius * 2);

                for (int i = 0; i < mapRadius * 2; i++)
                {
                    for (int j = 0; j < mapRadius * 2; j++)
                    {
                        Vector2 point = new Vector2(i - mapRadius, -(j - mapRadius));
                        float dist = biome.Distance(point);
                        bmp.SetPixel(i, j, System.Drawing.Color.FromArgb((int)dist * 5 %255, ((int)dist * 5 + 90) % 255 , ((int)dist * 5 + 180)% 255));
                    }
                }

                string path = Application.persistentDataPath + "/BiomeMap.png";
                Debug.Log("Save to: " + path);
                bmp.Save(path);
                */
            }
        }

        private void CreateWorldAndSaveToBinary()
        {
            SpatialDictionary<Island> islands = CreateAndPrintMap();

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

        private SpatialDictionary<Island> CreateAndPrintMap()
        {
            SpatialDictionary<Island> islands = CreateMap();

            MapPainter mp = new MapPainter();
            mp.DrawIslands(ref islands, 0, 0, 3 * ISLAND_GEN_SQUARE_RADIUS);

            return islands;
        }

        private SpatialDictionary<Island> CreateMap()
        {
            SpatialDictionary<Island> islands = new SpatialDictionary<Island>();
            for (int i = -2 * ISLAND_GEN_SQUARE_RADIUS; i <= 2 * ISLAND_GEN_SQUARE_RADIUS; i += 2 * ISLAND_GEN_SQUARE_RADIUS)
            {
                for (int j = -2 * ISLAND_GEN_SQUARE_RADIUS; j <= 2 * ISLAND_GEN_SQUARE_RADIUS; j += 2 * ISLAND_GEN_SQUARE_RADIUS)
                {
                    generateIslandsInSquare(ref islands, i, j);
                }
            }

            return islands;
        }

        private void LoadAndPrintMap()
        {
            SpatialDictionary<Island> islands = LoadFromBinary();

            MapPainter mp = new MapPainter();
            mp.DrawIslands(ref islands, 0, 0, 3 * ISLAND_GEN_SQUARE_RADIUS);
        }

        private SpatialDictionary<Island> LoadFromBinary()
        {
            // load from binary
            FileStream fs = new FileStream(Application.persistentDataPath + "/islands.dat", FileMode.Open);

            // Binary formatter with vector 2
            BinaryFormatter bf = new BinaryFormatter();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate();
            surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2SS);
            bf.SurrogateSelector = surrogateSelector;

            SpatialDictionary<Island> islands;

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

            return islands;
        }

        private void generateIslandsInSquare(ref SpatialDictionary<Island> islands, int xCenter, int yCenter)
        {
            System.Random rnd = new System.Random();

            for (int i = 0; i < NUMBER_OF_ISLANDS_PER_SQUARE; i++)
            {
                int tryCount = 0;

                while (tryCount < ISLAND_MAX_TRY)
                {
                    int x = rnd.Next(xCenter - ISLAND_GEN_SQUARE_RADIUS, xCenter + ISLAND_GEN_SQUARE_RADIUS - 1);
                    int y = rnd.Next(yCenter - ISLAND_GEN_SQUARE_RADIUS, yCenter + ISLAND_GEN_SQUARE_RADIUS - 1);

                    // Rejection
                    if (!IsRejectedIsland(ref islands, x, y, ISLAND_RADIUS))
                    {
                        // Add Island
                        Island island = new Island(new Vector2(x, y), ISLAND_RADIUS);
                        islands.Add(x, y, island);
                        break;
                    }

                    tryCount++;
                }

                if (tryCount == ISLAND_MAX_TRY)
                {
                    print("Max try");
                }
            }
        }

        private bool IsRejectedIsland(ref SpatialDictionary<Island> islands, int x, int y, float radius)
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

        private SpatialDictionary<Biome> CreateAndPrintBiomeMap()
        {
            SpatialDictionary<Biome> biomes = CreateBiomeMap();

            MapPainter mp = new MapPainter();
            mp.DrawBiomes(ref biomes, 0, 0, BIOME_GEN_SQUARE_RADIUS);

            return biomes;
        }

        private SpatialDictionary<Biome> CreateBiomeMap()
        {
            SpatialDictionary<Biome> biomes = new SpatialDictionary<Biome>();
            generateBiomessInSquare(ref biomes, 0, 0);
            return biomes;
        }


        private void generateBiomessInSquare(ref SpatialDictionary<Biome> biomes, int xCenter, int yCenter)
        {
            System.Random rnd = new System.Random();

            for (int i = 0; i < NUMBER_OF_BIOMES_PER_SQUARE; i++)
            {
                int tryCount = 0;

                while (tryCount < BIOME_MAX_TRY)
                {
                    int x = rnd.Next(xCenter - BIOME_GEN_SQUARE_RADIUS, xCenter + BIOME_GEN_SQUARE_RADIUS - 1);
                    int y = rnd.Next(yCenter - BIOME_GEN_SQUARE_RADIUS, yCenter + BIOME_GEN_SQUARE_RADIUS - 1);

                    // Rejection
                    if (!IsRejectedBiome(ref biomes, x, y))
                    {
                        // Add Island
                        Biome biome = new Biome(new Vector2(x, y));
                        biomes.Add(x, y, biome);
                        break;
                    }

                    tryCount++;
                }

                if (tryCount == BIOME_MAX_TRY)
                {
                    print("Max try");
                }
            }
        }


        private bool IsRejectedBiome(ref SpatialDictionary<Biome> biomes, int x, int y)
        {
            Vector2 biomeCenter = new Vector2(x, y);
            float minDistance = MIN_DISTANCE_BETWEEN_BIOMES_POLY + Biome.SQUARE_RADIUS * Biome.MAX_DEFORMATION_RATIO * 2;

            List<Biome> possibleCollidings = biomes.GetAround(x, y, (int)Mathf.Ceil(minDistance));

            foreach (Biome pc in possibleCollidings)
            {
                if (pc.Distance(biomeCenter) < MIN_DISTANCE_BETWEEN_BIOMES_POLY + Biome.SQUARE_RADIUS * Biome.MAX_DEFORMATION_RATIO)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
