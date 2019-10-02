using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    public class WorldCreator
    {
        // General data
        public int generationSquareRadius = 1024;

        // Biome constants
        private int nbBiomesPerSquare = 60;
        public float minDistanceBetweenBiomePolygones = 150;
        public int biomeMaxTry = 500;

        // Island constants
        public float ISLAND_RADIUS = 2.3f; // TODO --> new shapes of islands
        private int nbIslandsPerSquare = 600;
        public float minDistanceBetwwenIslands = 40;
        public int islandsMaxTry = 200;

        // Debug checkBoxed
        public bool createBiomeMapAndSaveBin = true;
        public bool createWorldMapAndSaveBin = true;
        public bool loadAndPrintMap = true;

        // random
        static System.Random rnd;

        // File path
        string path;

        public WorldCreator(string path, int genRadius, float biomeDensity, float islandDensity)
        {
            this.path = path;
            generationSquareRadius = genRadius;
            nbBiomesPerSquare = (int)(genRadius * genRadius * biomeDensity);
            nbIslandsPerSquare = (int)(genRadius * genRadius * islandDensity);
        }

        public void CreateMapSector(ref SpatialDictionary<Biome> biomes, ref SpatialDictionary<Island> islands, int xCenter, int yCenter)
        {
            // create biomes
            GenerateBiomesInSquareAndNeighbourSquares(ref biomes, xCenter, yCenter);

            // create islands
            GenerateIslandsInSquare(ref biomes, ref islands, xCenter, yCenter);
        }

        public void GenerateIslandsInSquare(ref SpatialDictionary<Biome> biomes,
            ref SpatialDictionary<Island> islands, int xCenter, int yCenter)
        {
            rnd = new System.Random();

            Debug.Log("Generate Islands in : [" + (xCenter - generationSquareRadius) + ";" + (xCenter + generationSquareRadius) + "] x ["
                + (yCenter - generationSquareRadius) + ";" + (yCenter + generationSquareRadius) + "]");

            for (int i = 0; i < nbIslandsPerSquare; i++)
            {
                int tryCount = 0;

                while (tryCount < islandsMaxTry)
                {
                    int x = rnd.Next(xCenter - generationSquareRadius, xCenter + generationSquareRadius);
                    int y = rnd.Next(yCenter - generationSquareRadius, yCenter + generationSquareRadius);

                    // Rejection
                    if (!IsRejectedIsland(ref islands, x, y))
                    {
                        // Add Island
                        Biome.Type bt = Biome.GetBiome(biomes, new Vector2(x, y)).type;
                        Island island = new Island(new Vector2(x, y), ISLAND_RADIUS, bt);
                        islands.Add(x, y, island);
                        break;
                    }

                    tryCount++;
                }
            }
        }

        private bool IsRejectedIsland(ref SpatialDictionary<Island> islands, int x, int y)
        {
            Vector2 islandCenter = new Vector2(x, y);
            float minDistance = minDistanceBetwwenIslands + (ISLAND_RADIUS + Island.MAX_POSSIBLE_RADIUS) * Island.SCALE;

            List<Island> possibleCollidings = islands.GetAround(x, y, (int)Math.Ceiling(minDistance));

            foreach (Island pc in possibleCollidings)
            {
                if ((islandCenter - pc.centerInWorld).magnitude < minDistance)
                {
                    return true;
                }
            }

            return false;
        }

        public void SaveData(ref SpatialDictionary<Biome> biomes, ref SpatialDictionary<Island> islands)
        {
            SaveSpDic(biomes, "biomes.dat");
            SaveSpDic(islands, "islands.dat");
        }

        public void SaveSpDic(object spDic, string fileName)
        {
            // save to binary
            FileStream fs = new FileStream(path + fileName, FileMode.Create);

            // Binary formatter with vector 2
            BinaryFormatter bf = new BinaryFormatter();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate();
            surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2SS);
            bf.SurrogateSelector = surrogateSelector;

            try
            {
                bf.Serialize(fs, spDic);
                Debug.Log("File saved to " + path + fileName);
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

        public void LoadData(out SpatialDictionary<Biome> biomes, out SpatialDictionary<Island> islands)
        {
            biomes = (SpatialDictionary<Biome>)LoadSpDic("biomes.dat");
            islands = (SpatialDictionary<Island>)LoadSpDic("islands.dat");
        }

        public object LoadSpDic(string fileName)
        {
            // save to binary
            FileStream fs = new FileStream(path + fileName, FileMode.Open);

            Debug.Log("Load file from " + path + fileName);

            // Binary formatter with vector 2
            BinaryFormatter bf = new BinaryFormatter();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate();
            surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2SS);
            bf.SurrogateSelector = surrogateSelector;

            object spDic = null;

            try
            {
                spDic = bf.Deserialize(fs);
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

            return spDic;
        }


        // Generate biomes in 9 squares centered on this one
        public void GenerateBiomesInSquareAndNeighbourSquares(ref SpatialDictionary<Biome> biomes, int xCenter, int yCenter)
        {
            for (int i = xCenter - generationSquareRadius * 2; i <= xCenter + generationSquareRadius * 2; i += generationSquareRadius * 2)
            {
                for (int j = yCenter - generationSquareRadius * 2; j <= yCenter + generationSquareRadius * 2; j += generationSquareRadius * 2)
                {
                    if (biomes.IsEmpty(i, j, generationSquareRadius))
                    {
                        GenerateBiomesInSquare(ref biomes, i, j);
                        BiomeDetermineType(ref biomes, i, j);
                    }
                }
            }
        }

        private void GenerateBiomesInSquare(ref SpatialDictionary<Biome> biomes, int xCenter, int yCenter)
        {
            rnd = new System.Random();

            for (int i = 0; i < nbBiomesPerSquare; i++)
            {
                int tryCount = 0;

                while (tryCount < biomeMaxTry)
                {
                    int x = rnd.Next(xCenter - generationSquareRadius, xCenter + generationSquareRadius);
                    int y = rnd.Next(yCenter - generationSquareRadius, yCenter + generationSquareRadius);

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
            }
        }

        private bool IsRejectedBiome(ref SpatialDictionary<Biome> biomes, int x, int y)
        {
            Vector2 biomeCenter = new Vector2(x, y);
            float minDistance = minDistanceBetweenBiomePolygones + Biome.SQUARE_RADIUS * Biome.MAX_DEFORMATION_RATIO * 2;

            List<Biome> possibleCollidings = biomes.GetAround(x, y, (int)Math.Ceiling(minDistance));

            foreach (Biome pc in possibleCollidings)
            {
                if (pc.Distance(biomeCenter) < minDistanceBetweenBiomePolygones + Biome.SQUARE_RADIUS * Biome.MAX_DEFORMATION_RATIO)
                {
                    return true;
                }
            }

            return false;
        }

        private Dictionary<Dic2DKey, List<Dic2DKey>> BiomeDetermineType(
            ref SpatialDictionary<Biome> biomes,
            int xCenter, int yCenter)
        {
            Dictionary<Dic2DKey, List<Dic2DKey>> keyNeighbours = new Dictionary<Dic2DKey, List<Dic2DKey>>();
            FindNeighbours(biomes, ref keyNeighbours, xCenter, yCenter, generationSquareRadius + (int)minDistanceBetweenBiomePolygones * 2);

            // Biome neighbourhood
            Dictionary<Dic2DKey, Neighbourhood> biomeNeighbours = new Dictionary<Dic2DKey, Neighbourhood>();
            foreach (KeyValuePair<Dic2DKey, List<Dic2DKey>> pair in keyNeighbours)
            {
                biomeNeighbours.Add(pair.Key, new Neighbourhood(biomes, pair.Value));
            }

            List<List<Biome>> orderList = BuildOrderList(biomes, biomeNeighbours);

            // Set biome types
            rnd = new System.Random();
            List<Biome> typingBiomeList = new List<Biome>();

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
