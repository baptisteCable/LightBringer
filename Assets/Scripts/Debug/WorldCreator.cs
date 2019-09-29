using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

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
        public bool createBiomeMapAndSaveBin = true;
        public bool createWorldMapAndSaveBin = true;
        public bool loadAndPrintMap = true;

        // random
        static System.Random rnd;

        // Update is called once per frame
        void Update()
        {
            if (!createBiomeMapAndSaveBin)
            {
                createBiomeMapAndSaveBin = true;
                CreateBiomesAndSaveToBinary(0, 0, NUMBER_OF_BIOMES_PER_SQUARE, BIOME_GEN_SQUARE_RADIUS);
            }

            if (!createWorldMapAndSaveBin)
            {
                createWorldMapAndSaveBin = true;
                CreateIslandsAndSaveToBinary();
            }

            if (!loadAndPrintMap)
            {
                loadAndPrintMap = true;
                LoadAndPrintMap();
            }
        }

        private void CreateIslandsAndSaveToBinary()
        {
            SpatialDictionary<Island> islands = CreateMap();

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

        private SpatialDictionary<Island> CreateMap()
        {
            SpatialDictionary<Biome> biomes = LoadBiomesFromBinary();
            SpatialDictionary<Island> islands = new SpatialDictionary<Island>();
            for (int i = -2 * ISLAND_GEN_SQUARE_RADIUS; i <= 2 * ISLAND_GEN_SQUARE_RADIUS; i += 2 * ISLAND_GEN_SQUARE_RADIUS)
            {
                for (int j = -2 * ISLAND_GEN_SQUARE_RADIUS; j <= 2 * ISLAND_GEN_SQUARE_RADIUS; j += 2 * ISLAND_GEN_SQUARE_RADIUS)
                {
                    generateIslandsInSquare(ref biomes, ref islands, i, j);
                }
            }

            return islands;
        }

        private SpatialDictionary<Island> LoadAndPrintMap()
        {
            SpatialDictionary<Island> islands = LoadIslandsFromBinary();
            SpatialDictionary<Biome> biomes = LoadBiomesFromBinary();

            MapPainter mp = new MapPainter();
            mp.DrawIslands(ref biomes, ref islands, 0, 0, 3 * ISLAND_GEN_SQUARE_RADIUS, 1);

            return islands;
        }

        private SpatialDictionary<Island> LoadIslandsFromBinary()
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

        private void generateIslandsInSquare(ref SpatialDictionary<Biome> biomes,
            ref SpatialDictionary<Island> islands, int xCenter, int yCenter)
        {
            rnd = new System.Random();

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
                        Biome.Type bt = Biome.GetBiome(biomes, new Vector2(x, y)).type;
                        Island island = new Island(new Vector2(x, y), ISLAND_RADIUS, bt);
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

        private void CreateBiomesAndSaveToBinary(int xCenter, int yCenter, int nbBiomesPerSquare, int squareRadius)
        {
            SpatialDictionary<Biome> biomes = CreateAndPrintBiomeMaps(xCenter, yCenter, nbBiomesPerSquare, squareRadius);

            // save to binary
            FileStream fs = new FileStream(Application.persistentDataPath + "/biomes.dat", FileMode.Create);

            // Binary formatter with vector 2
            BinaryFormatter bf = new BinaryFormatter();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate();
            surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2SS);
            bf.SurrogateSelector = surrogateSelector;

            try
            {
                bf.Serialize(fs, biomes);
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

        private SpatialDictionary<Biome> CreateAndPrintBiomeMaps(int xCenter, int yCenter, int nbBiomesPerSquare, int squareRadius)
        {
            // create biomes
            SpatialDictionary<Biome> biomes = new SpatialDictionary<Biome>();
            generateBiomessInSquare(ref biomes, nbBiomesPerSquare, xCenter, yCenter, squareRadius);

            // typing biomes
            Dictionary<Dic2DKey, List<Dic2DKey>> keyNeighbours = BiomeDetermineType(ref biomes,
                out List<Biome> typingBiomeList,
                out List<List<Biome>> orderList,
                xCenter, yCenter, squareRadius);

            MapPainter mp = new MapPainter();
            mp.DrawBiomes(ref biomes, xCenter, yCenter, squareRadius, 4);
            mp.DrawBiomesPoly(ref biomes, xCenter, yCenter, squareRadius);
            mp.DrawNeighbourhoodLines(ref keyNeighbours, xCenter, yCenter, squareRadius);
            mp.DrawBiomeTypingOrder(typingBiomeList, xCenter, yCenter, squareRadius);
            mp.DrawBiomeOrder(orderList, xCenter, yCenter, squareRadius);

            return biomes;
        }

        private void generateBiomessInSquare(ref SpatialDictionary<Biome> biomes, int nbBiomesPerSquare, int xCenter, int yCenter, int squareRadius)
        {
            rnd = new System.Random();

            for (int i = 0; i < nbBiomesPerSquare; i++)
            {
                int tryCount = 0;

                while (tryCount < BIOME_MAX_TRY)
                {
                    int x = rnd.Next(xCenter - squareRadius, xCenter + squareRadius - 1);
                    int y = rnd.Next(yCenter - squareRadius, yCenter + squareRadius - 1);

                    // Rejection
                    if (!IsRejectedBiome(ref biomes, x, y))
                    {
                        // Add Biome
                        Biome biome = new Biome(x, y);
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

        private SpatialDictionary<Biome> LoadBiomesFromBinary()
        {
            // load from binary
            FileStream fs = new FileStream(Application.persistentDataPath + "/biomes.dat", FileMode.Open);

            // Binary formatter with vector 2
            BinaryFormatter bf = new BinaryFormatter();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate();
            surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2SS);
            bf.SurrogateSelector = surrogateSelector;

            SpatialDictionary<Biome> biomes;

            try
            {
                biomes = (SpatialDictionary<Biome>)bf.Deserialize(fs);
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

            return biomes;
        }

        private Dictionary<Dic2DKey, List<Dic2DKey>> BiomeDetermineType(
            ref SpatialDictionary<Biome> biomes,
            out List<Biome> typingBiomeList,
            out List<List<Biome>> orderList,
            int xCenter, int yCenter, int squareRadius)
        {
            Dictionary<Dic2DKey, List<Dic2DKey>> keyNeighbours = new Dictionary<Dic2DKey, List<Dic2DKey>>();
            FindNeighbours(biomes, ref keyNeighbours, xCenter, yCenter, squareRadius);

            // Biome neighbourhood
            Dictionary<Dic2DKey, Neighbourhood> biomeNeighbours = new Dictionary<Dic2DKey, Neighbourhood>();
            foreach (KeyValuePair<Dic2DKey, List<Dic2DKey>> pair in keyNeighbours)
            {
                biomeNeighbours.Add(pair.Key, new Neighbourhood(biomes, pair.Value));
            }

            orderList = BuildOrderList(biomes, biomeNeighbours);

            // Set biome types
            rnd = new System.Random();
            typingBiomeList = new List<Biome>();

            foreach (List<Biome> biomeList in orderList)
            {
                while (true)
                {
                    // find the most constrained without type
                    int maxConstraints = -1;
                    Biome moreConstrainedBiome = null;

                    foreach (Biome biome in biomeList)
                    {
                        if (biome.type == Biome.Type.Undefined && biomeNeighbours[biome.coord].typedNeighbourCount > maxConstraints)
                        {
                            maxConstraints = biomeNeighbours[biome.coord].typedNeighbourCount;
                            moreConstrainedBiome = biome;
                        }
                    }

                    if (maxConstraints == -1)
                    {
                        break;
                    }

                    // give it a type
                    typingBiomeList.Add(moreConstrainedBiome);

                    List<Biome.Type> availableTypes = new List<Biome.Type>();
                    foreach (Biome.Type t in Enum.GetValues(typeof(Biome.Type)))
                    {
                        if (t != Biome.Type.Undefined)
                        {
                            availableTypes.Add(t);
                        }
                    }

                    foreach (Biome b in biomeNeighbours[moreConstrainedBiome.coord].neighbours)
                    {
                        if (availableTypes.Contains(b.type))
                        {
                            availableTypes.Remove(b.type);
                        }
                    }

                    if (availableTypes.Count == 0)
                    {
                        moreConstrainedBiome.type = Biome.Type.Light;
                    }
                    else
                    {
                        int type = rnd.Next(availableTypes.Count);
                        moreConstrainedBiome.type = availableTypes[type];
                    }

                    foreach (Dic2DKey key in keyNeighbours[moreConstrainedBiome.coord])
                    {
                        biomeNeighbours[key].typedNeighbourCount++;
                    }
                }
            }

            return keyNeighbours;
        }

        private static List<List<Biome>> BuildOrderList(SpatialDictionary<Biome> biomes, Dictionary<Dic2DKey, Neighbourhood> biomeNeighbours)
        {
            // build order lists
            List<List<Biome>> orderList = new List<List<Biome>>();

            // order zero is the more constrained biome
            int maxConstraints = int.MinValue;
            Biome orderZeroBiome = null;
            foreach (KeyValuePair<Dic2DKey, Neighbourhood> pair in biomeNeighbours)
            {
                if (pair.Value.typedNeighbourCount > maxConstraints)
                {
                    maxConstraints = pair.Value.typedNeighbourCount;
                    orderZeroBiome = biomes.Get(pair.Key);
                }
            }

            List<Biome> orderZeroList = new List<Biome>();
            orderZeroList.Add(orderZeroBiome);
            orderList.Add(orderZeroList);

            // next orders
            while (true)
            {
                int order = orderList.Count;

                List<Biome> currentOrder = new List<Biome>();

                foreach (Biome previousBiome in orderList[order - 1])
                {
                    foreach (Biome neighbour in biomeNeighbours[previousBiome.coord].neighbours)
                    {
                        bool previousOrder = false;

                        foreach (List<Biome> biomeList in orderList)
                        {
                            if (biomeList.Contains(neighbour))
                            {
                                previousOrder = true;
                                break;
                            }
                        }

                        if (!previousOrder && !currentOrder.Contains(neighbour))
                        {
                            currentOrder.Add(neighbour);
                        }
                    }
                }

                if (currentOrder.Count == 0)
                {
                    break;
                }

                orderList.Add(currentOrder);
            }

            return orderList;
        }

        private static void FindNeighbours(
            SpatialDictionary<Biome> biomes,
            ref Dictionary<Dic2DKey, List<Dic2DKey>> neighbours,
            int xCenter, int yCenter, int squareRadius, int step = 32)
        {
            for (int i = xCenter - squareRadius; i < xCenter + squareRadius; i += step)
            {
                for (int j = yCenter - squareRadius; j < yCenter + squareRadius; j += step)
                {
                    // Find the 4 closest biomes
                    List<Dic2DKey> fourClosest = Biome.Get4ClosestBiomes(biomes, new Vector2(i, j), out List<float> minDists);

                    if (Math.Abs(minDists[0] - minDists[3]) < step * 2 && step > 1)
                    {
                        // Look carefully at this zone
                        FindNeighbours(biomes, ref neighbours, i, j, step / 2, Math.Max(1, step / 4));
                    }
                    else
                    {
                        if (Math.Abs(minDists[0] - minDists[1]) < step * 2)
                        {
                            // 0 and 1 are neighbours
                            AddNeighbours(ref neighbours, fourClosest[0], fourClosest[1]);
                            AddNeighbours(ref neighbours, fourClosest[1], fourClosest[0]);
                        }

                        if (Math.Abs(minDists[0] - minDists[2]) < step * 2)
                        {
                            // 0 and 2 are neighbours
                            AddNeighbours(ref neighbours, fourClosest[0], fourClosest[2]);
                            AddNeighbours(ref neighbours, fourClosest[2], fourClosest[0]);
                        }
                    }
                }
            }
        }

        static private void AddNeighbours(ref Dictionary<Dic2DKey, List<Dic2DKey>> neighbours, Dic2DKey key1, Dic2DKey key2)
        {
            if (!neighbours.ContainsKey(key1))
            {
                neighbours[key1] = new List<Dic2DKey>();
            }

            if (!neighbours[key1].Contains(key2))
            {
                neighbours[key1].Add(key2);
            }
        }
    }
}
