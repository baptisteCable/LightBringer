using LightBringer.Scenery;
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
        public int generationSquareRadius;

        // Biome constants
        public float minDistanceBetweenBiomePolygones = 150;
        public int biomeMaxTry = 1500;

        // Island constants
        public int avgNbIslandsPerSquare;
        public float minDistanceBetwwenIslands = 35;
        public int islandsMaxTry = 2500;

        // Debug checkBoxed
        public bool createBiomeMapAndSaveBin = true;
        public bool createWorldMapAndSaveBin = true;
        public bool loadAndPrintMap = true;

        // random
        static System.Random rnd;

        // File path
        string path;

        public WorldCreator (string path, int genRadius = 512)
        {
            this.path = path;
            generationSquareRadius = genRadius;
            avgNbIslandsPerSquare = (int)(170 / 512f / 512f * generationSquareRadius * generationSquareRadius);
        }

        public void CreateMapSector (ref SpatialDictionary<Biome> biomes, ref SpatialDictionary<Island> islands, int xCenter, int yCenter)
        {
            rnd = new System.Random ();

            // create biomes
            GenerateBiomesInSquareAndNeighborSquares (ref biomes, xCenter, yCenter);

            // create islands
            GenerateIslandsInSquare (ref biomes, ref islands, xCenter, yCenter);
        }

        public void GenerateIslandsInSquare (ref SpatialDictionary<Biome> biomes,
            ref SpatialDictionary<Island> islands, int xCenter, int yCenter)
        {
            // Special islands
            foreach (Biome b in biomes.GetAround (xCenter, yCenter, generationSquareRadius))
            {
                Island island = new Island (b.coord.ToVector2 (), b.type, 1);
                islands.Add (b.coord.x, b.coord.y, island);
            }

            int tryCount = 0;
            int isCount = 0;

            while (tryCount < islandsMaxTry)
            {
                int x = rnd.Next (xCenter - generationSquareRadius, xCenter + generationSquareRadius);
                int y = rnd.Next (yCenter - generationSquareRadius, yCenter + generationSquareRadius);

                // Rejection
                if (!IsRejectedIsland (ref islands, x, y, 0))
                {
                    // Add Island
                    Biome.Type bt = Biome.GetBiome (biomes, new Vector2 (x, y)).type;
                    Island island = new Island (new Vector2 (x, y), bt, 0);
                    islands.Add (x, y, island);
                    tryCount = -1;
                    isCount++;
                }

                tryCount++;
            }
        }

        private bool IsRejectedIsland (ref SpatialDictionary<Island> islands, int x, int y, int type)
        {
            Vector2 islandCenter = new Vector2 (x, y);

            float minDistance = minDistanceBetwwenIslands + (Island.MAX_RADIUS + Island.GetAvgRadius (type)) * Island.SCALE;
            List<Island> possibleCollidings = islands.GetAround (x, y, (int)Math.Ceiling (minDistance));

            foreach (Island pc in possibleCollidings)
            {
                minDistance = minDistanceBetwwenIslands + (pc.GetAvgRadius () + Island.GetAvgRadius (type)) * Island.SCALE;
                if ((islandCenter - pc.centerInWorld).magnitude < minDistance)
                {
                    return true;
                }
            }

            return false;
        }

        public void SaveData (ref SpatialDictionary<Biome> biomes,
            ref SpatialDictionary<Island> islands,
            ref SpatialDictionary<SceneryElement> sceneryElements)
        {
            SaveSpDic (biomes, "biomes.dat");
            SaveSpDic(islands, "islands.dat");
            SaveSpDic(sceneryElements, "scenery.dat");
        }

        public void SaveSpDic (object spDic, string fileName)
        {
            // save to binary
            FileStream fs = new FileStream (path + fileName, FileMode.Create);

            // Binary formatter with vector 2
            BinaryFormatter bf = new BinaryFormatter ();
            SurrogateSelector surrogateSelector = new SurrogateSelector ();
            Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate ();
            surrogateSelector.AddSurrogate (typeof (Vector2), new StreamingContext (StreamingContextStates.All), vector2SS);
            bf.SurrogateSelector = surrogateSelector;

            try
            {
                bf.Serialize (fs, spDic);
                Debug.Log ("File saved to " + path + fileName);
            }
            catch (SerializationException e)
            {
                Console.WriteLine ("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close ();
            }
        }

        public void LoadData (out SpatialDictionary<Biome> biomes,
            out SpatialDictionary<Island> islands,
            out SpatialDictionary<SceneryElement> sceneryElements)
        {
            biomes = (SpatialDictionary<Biome>)LoadSpDic ("biomes.dat");
            islands = (SpatialDictionary<Island>)LoadSpDic ("islands.dat");
            sceneryElements = (SpatialDictionary<SceneryElement>)LoadSpDic("scenery.dat");
        }

        public object LoadSpDic (string fileName)
        {
            FileStream fs;

            // load from binary
            try
            {
                fs = new FileStream (path + fileName, FileMode.Open);
            }
            catch
            {
                return null;
            }

            Debug.Log ("Load file from " + path + fileName);

            // Binary formatter with vector 2
            BinaryFormatter bf = new BinaryFormatter ();
            SurrogateSelector surrogateSelector = new SurrogateSelector ();
            Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate ();
            surrogateSelector.AddSurrogate (typeof (Vector2), new StreamingContext (StreamingContextStates.All), vector2SS);
            bf.SurrogateSelector = surrogateSelector;

            object spDic = null;

            try
            {
                spDic = bf.Deserialize (fs);
            }
            catch (SerializationException e)
            {
                Debug.LogError ("Failed to serialize. Reason: " + e.Message);
            }
            finally
            {
                fs.Close ();
            }

            return spDic;
        }


        // Generate biomes in 9 squares centered on this one
        public void GenerateBiomesInSquareAndNeighborSquares (ref SpatialDictionary<Biome> biomes, int xCenter, int yCenter)
        {
            for (int i = xCenter - generationSquareRadius * 2; i <= xCenter + generationSquareRadius * 2; i += generationSquareRadius * 2)
            {
                for (int j = yCenter - generationSquareRadius * 2; j <= yCenter + generationSquareRadius * 2; j += generationSquareRadius * 2)
                {
                    if (biomes.IsEmpty (i, j, generationSquareRadius))
                    {
                        GenerateBiomesInSquare (ref biomes, i, j);
                        BiomeDetermineType (ref biomes, i, j);
                    }
                }
            }
        }

        private void GenerateBiomesInSquare (ref SpatialDictionary<Biome> biomes, int xCenter, int yCenter)
        {
            if (xCenter == 0 && yCenter == 0)
            {
                Biome biome = new Biome (0, 0);
                biome.type = Biome.Type.Light;
                biomes.Add (0, 0, biome);
            }

            int tryCount = 0;

            if (rnd == null)
            {
                rnd = new System.Random();
            }

            while (tryCount < biomeMaxTry)
            {
                int x = rnd.Next (xCenter - generationSquareRadius, xCenter + generationSquareRadius);
                int y = rnd.Next (yCenter - generationSquareRadius, yCenter + generationSquareRadius);

                // Rejection
                if (!IsRejectedBiome (ref biomes, x, y))
                {
                    // Add Biome
                    Biome biome = new Biome (x, y);
                    biomes.Add (x, y, biome);
                    tryCount = -1;
                }

                tryCount++;
            }
        }

        private bool IsRejectedBiome (ref SpatialDictionary<Biome> biomes, int x, int y)
        {
            Vector2 biomeCenter = new Vector2 (x, y);
            float minDistance = minDistanceBetweenBiomePolygones + Biome.SQUARE_RADIUS * Biome.MAX_DEFORMATION_RATIO * 2;

            List<Biome> possibleCollidings = biomes.GetAround (x, y, (int)Math.Ceiling (minDistance));

            foreach (Biome pc in possibleCollidings)
            {
                if (pc.Distance (biomeCenter) < minDistanceBetweenBiomePolygones + Biome.SQUARE_RADIUS * Biome.MAX_DEFORMATION_RATIO)
                {
                    return true;
                }
            }

            return false;
        }

        private Dictionary<Dic2DKey, List<Dic2DKey>> BiomeDetermineType (
            ref SpatialDictionary<Biome> biomes,
            int xCenter, int yCenter)
        {
            Dictionary<Dic2DKey, List<Dic2DKey>> keyNeighbors = new Dictionary<Dic2DKey, List<Dic2DKey>> ();
            FindNeighbors (biomes, ref keyNeighbors, xCenter, yCenter, generationSquareRadius + (int)minDistanceBetweenBiomePolygones * 2);

            // Biome neighborhood
            Dictionary<Dic2DKey, Neighborhood> biomeNeighbors = new Dictionary<Dic2DKey, Neighborhood> ();
            foreach (KeyValuePair<Dic2DKey, List<Dic2DKey>> pair in keyNeighbors)
            {
                biomeNeighbors.Add (pair.Key, new Neighborhood (biomes, pair.Value));
            }

            List<List<Biome>> orderList = BuildOrderList (biomes, biomeNeighbors);

            // Set biome types
            List<Biome> typingBiomeList = new List<Biome> ();

            foreach (List<Biome> biomeList in orderList)
            {
                while (true)
                {
                    // find the most constrained without type
                    int maxConstraints = -1;
                    Biome moreConstrainedBiome = null;

                    foreach (Biome biome in biomeList)
                    {
                        if (biome.type == Biome.Type.Undefined && biomeNeighbors[biome.coord].typedNeighborCount > maxConstraints)
                        {
                            maxConstraints = biomeNeighbors[biome.coord].typedNeighborCount;
                            moreConstrainedBiome = biome;
                        }
                    }

                    if (maxConstraints == -1)
                    {
                        break;
                    }

                    // give it a type
                    typingBiomeList.Add (moreConstrainedBiome);

                    List<Biome.Type> availableTypes = new List<Biome.Type> ();
                    foreach (Biome.Type t in Enum.GetValues (typeof (Biome.Type)))
                    {
                        if (t != Biome.Type.Undefined)
                        {
                            availableTypes.Add (t);
                        }
                    }

                    foreach (Biome b in biomeNeighbors[moreConstrainedBiome.coord].neighbors)
                    {
                        if (availableTypes.Contains (b.type))
                        {
                            availableTypes.Remove (b.type);
                        }
                    }

                    if (availableTypes.Count == 0)
                    {
                        moreConstrainedBiome.type = Biome.Type.Darkness;
                    }
                    else
                    {
                        int type = rnd.Next (availableTypes.Count);
                        moreConstrainedBiome.type = availableTypes[type];
                    }

                    foreach (Dic2DKey key in keyNeighbors[moreConstrainedBiome.coord])
                    {
                        biomeNeighbors[key].typedNeighborCount++;
                    }
                }
            }

            return keyNeighbors;
        }

        private static List<List<Biome>> BuildOrderList (SpatialDictionary<Biome> biomes, Dictionary<Dic2DKey, Neighborhood> biomeNeighbors)
        {
            // build order lists
            List<List<Biome>> orderList = new List<List<Biome>> ();

            // order zero is the more constrained biome
            int maxConstraints = int.MinValue;
            Biome orderZeroBiome = null;
            foreach (KeyValuePair<Dic2DKey, Neighborhood> pair in biomeNeighbors)
            {
                if (pair.Value.typedNeighborCount > maxConstraints)
                {
                    maxConstraints = pair.Value.typedNeighborCount;
                    orderZeroBiome = biomes.Get (pair.Key);
                }
            }

            List<Biome> orderZeroList = new List<Biome> ();
            orderZeroList.Add (orderZeroBiome);
            orderList.Add (orderZeroList);

            // next orders
            while (true)
            {
                int order = orderList.Count;

                List<Biome> currentOrder = new List<Biome> ();

                foreach (Biome previousBiome in orderList[order - 1])
                {
                    foreach (Biome neighbor in biomeNeighbors[previousBiome.coord].neighbors)
                    {
                        bool previousOrder = false;

                        foreach (List<Biome> biomeList in orderList)
                        {
                            if (biomeList.Contains (neighbor))
                            {
                                previousOrder = true;
                                break;
                            }
                        }

                        if (!previousOrder && !currentOrder.Contains (neighbor))
                        {
                            currentOrder.Add (neighbor);
                        }
                    }
                }

                if (currentOrder.Count == 0)
                {
                    break;
                }

                orderList.Add (currentOrder);
            }

            return orderList;
        }

        private static void FindNeighbors (
            SpatialDictionary<Biome> biomes,
            ref Dictionary<Dic2DKey, List<Dic2DKey>> neighbors,
            int xCenter, int yCenter, int squareRadius, int step = 32)
        {
            for (int i = xCenter - squareRadius; i < xCenter + squareRadius; i += step)
            {
                for (int j = yCenter - squareRadius; j < yCenter + squareRadius; j += step)
                {
                    // Find the 4 closest biomes
                    List<Dic2DKey> fourClosest = Biome.Get4ClosestBiomes (biomes, new Vector2 (i, j), out List<float> minDists);

                    if (Math.Abs (minDists[0] - minDists[3]) < step * 2 && step > 1)
                    {
                        // Look carefully at this zone
                        FindNeighbors (biomes, ref neighbors, i, j, step / 2, Math.Max (1, step / 4));
                    }
                    else
                    {
                        if (Math.Abs (minDists[0] - minDists[1]) < step * 2)
                        {
                            // 0 and 1 are neighbors
                            AddNeighbors (ref neighbors, fourClosest[0], fourClosest[1]);
                            AddNeighbors (ref neighbors, fourClosest[1], fourClosest[0]);
                        }

                        if (Math.Abs (minDists[0] - minDists[2]) < step * 2)
                        {
                            // 0 and 2 are neighbors
                            AddNeighbors (ref neighbors, fourClosest[0], fourClosest[2]);
                            AddNeighbors (ref neighbors, fourClosest[2], fourClosest[0]);
                        }
                    }
                }
            }
        }

        static private void AddNeighbors (ref Dictionary<Dic2DKey, List<Dic2DKey>> neighbors, Dic2DKey key1, Dic2DKey key2)
        {
            if (!neighbors.ContainsKey (key1))
            {
                neighbors[key1] = new List<Dic2DKey> ();
            }

            if (!neighbors[key1].Contains (key2))
            {
                neighbors[key1].Add (key2);
            }
        }
    }
}
