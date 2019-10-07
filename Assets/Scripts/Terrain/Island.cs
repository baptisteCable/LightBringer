using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    [Serializable]
    public class Island
    {
        public const float CLIFF_SLOPE = 3.5f;
        public const float SLOPE_WIDTH = .5f;
        public const float SLOPE_LANDING = .5f;
        public const float SLOPE_DESCENT = .75f; // proportion of the second segment used for going down
        private const float SLOPE_WAY_WIDTH = .3f;
        public const float ISLAND_RADIUS = 2.3f; // TODO --> new shapes of islands
        private const float BIOME_GROUND_DIST = 1.5f;

        public const float SCALE = 7f; // scale from island units to world units
        public const float MAX_POSSIBLE_RADIUS = 2.3f; // Used for rejection sampling

        private const int GROUND_1_FUNCTION_PARTS = 12;
        private const float GROUND_1_MIN = 4f;
        private const float GROUND_1_MAX = 8f;

        int seed = 0;

        public Biome.Type biomeType;

        public Vector2 centerInWorld;
        public float radius { get; }

        // Index of the segment of the first slope. The second one is on the opposite side
        private int[] slopes = null;
        private bool[] slopeTopOnRight = null;

        [NonSerialized]
        private List<Vector2> vertices = null;

        [NonSerialized]
        private System.Random rnd;

        private static System.Random staticRnd;

        [NonSerialized]
        private SlopeData[] slopeData;

        [NonSerialized]
        private float[] ground1Zone;

        public Island(Vector2 centerPosition, Biome.Type bt, int newSeed = 0)
        {
            centerInWorld = centerPosition;
            radius = ISLAND_RADIUS;

            biomeType = bt;

            if (newSeed == 0)
            {
                if (staticRnd == null)
                {
                    staticRnd = new System.Random();
                }
                seed = staticRnd.Next();
            }
            else
            {
                seed = newSeed;
            }
        }

        public static Vector2 Vector2FromAngle(float a)
        {
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        }

        // radian angle
        public static Vector2 RotateVector(Vector2 v, float angle)
        {
            return RotateVector(v, (double)angle);
        }

        public static Vector2 RotateVector(Vector2 v, double angle)
        {
            double x = v.x * Math.Cos(angle) - v.y * Math.Sin(angle);
            double y = v.x * Math.Sin(angle) + v.y * Math.Cos(angle);
            return new Vector2((float)x, (float)y);
        }

        public void GenerateIslandVertices()
        {
            if (vertices != null)
            {
                return;
            }

            rnd = new System.Random(seed);

            InitGround1Function();

            vertices = new List<Vector2>();

            Vector2 vector = RotateVector(new Vector2(1, 0), Mathf.PI / 4f);
            vertices.Add(new Vector2(radius, 0));

            // Compute vertices
            while (vertices.Count < 5 * radius || (vertices[0] - vertices[vertices.Count - 1]).magnitude > 3f)
            {
                float angle = randomAngle(new Vector2(0, 0), radius, vector, vertices[vertices.Count - 1], rnd);
                vector = RotateVector(vector, angle);
                vertices.Add(vertices[vertices.Count - 1] + vector);
            }

            // last 2 vertices
            LastTwoVertices();

            // slopes
            GenerateSlopes();
        }

        private void LastTwoVertices()
        {
            Vector2 last = vertices[vertices.Count - 1];
            Vector2 first = vertices[0];

            Vector2 vector = first - last;
            float distance = vector.magnitude;
            vector /= distance;

            float angle = -Mathf.Acos((distance - 1) / 2);

            // add vertex 1
            vertices.Add(vertices[vertices.Count - 1] + RotateVector(vector, angle));

            // add vertex 2
            vertices.Add(vertices[vertices.Count - 1] + vector);
        }

        private float angleDistrib(float angle)
        {
            return -Mathf.Abs(1 / Mathf.PI) + 1;
        }

        private float distDistrib(float angle, Vector2 center, float radius, Vector2 vector, Vector2 previousPoint)
        {
            Vector2 newVector = RotateVector(vector, angle);

            // 0 if going back
            Vector2 tangent = RotateVector(previousPoint - center, Mathf.PI / 2f);
            if (Vector2.Dot(tangent, newVector) < 0)
            {
                return 0;
            }

            // compute distribution
            float dist = (center - (previousPoint + newVector)).magnitude;
            float ratio = dist / radius - 1;
            return Mathf.Exp(-6 * ratio * ratio);
        }

        private float randomAngle(Vector2 center, float radius, Vector2 vector, Vector2 previousPoint, System.Random rdm)
        {
            float[] distributions = new float[9];
            float sum = 0f;
            int i;

            // compute raw distributions
            for (i = 0; i < 9; i++)
            {
                float angle = (i - 4) * Mathf.PI / 8f;
                distributions[i] = angleDistrib(angle) * distDistrib(angle, center, radius, vector, previousPoint);
                sum += distributions[i];
            }

            // normalize
            for (i = 0; i < 9; i++)
            {
                distributions[i] /= sum;
            }


            double rnd = rdm.NextDouble();

            i = 0;
            while (distributions[i] < rnd)
            {
                rnd -= distributions[i];
                i++;
            }

            return (i - 4) * Mathf.PI / 8f;
        }

        // return the distance to the island in island unit (1f is segment length)
        public float DistanceFromIslandInIslandUnit(Vector2 point)
        {
            if (vertices == null)
            {
                GenerateIslandVertices();
            }

            // closest point
            int closest = 0;
            float minDist = float.PositiveInfinity;
            float[] distances = new float[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                distances[i] = (point - vertices[i]).magnitude;
                if (distances[i] < minDist)
                {
                    closest = i;
                    minDist = distances[i];
                }
            }

            Vector2 first, second;

            if (distances[(closest + 1) % vertices.Count] > distances[(closest + vertices.Count - 1) % vertices.Count])
            {
                first = vertices[(closest + vertices.Count - 1) % vertices.Count];
                second = vertices[closest];
            }
            else
            {
                first = vertices[closest];
                second = vertices[(closest + 1) % vertices.Count];
            }

            Vector2 normal = RotateVector(second - first, Mathf.PI / 2f);
            Vector2 vect = point - first;

            float dotProdTangent = Vector2.Dot(second - first, vect);
            float dotProdNormal = Vector2.Dot(normal, vect);

            if (dotProdNormal >= 0)
            {
                return 0;
            }
            // external angle case (circle dist)
            else if (dotProdTangent < 0 || dotProdTangent > 1)
            {
                return minDist;
            }
            else
            {
                return -dotProdNormal;
            }
        }

        public float DistanceFromIslandInWorldUnit(Vector2 worldPos)
        {
            Vector2 point = (worldPos - centerInWorld) / SCALE;
            return SCALE * DistanceFromIslandInIslandUnit(point);
        }

        public void GenerateIslandHeightsAndAlphaMap(
            ref float[,] terrainHeights,
            ref Biome.Type[,] biomeMap,
            ref GroundType[,] groundMap,
            Vector2 terrainPosition)
        {
            // Generate island data from seed
            if (vertices == null)
            {
                GenerateIslandVertices();
            }

            GenerateHeightsAndAlphaMap(ref terrainHeights, ref biomeMap, ref groundMap, terrainPosition);
        }

        private void GenerateHeightsAndAlphaMap(
            ref float[,] terrainHeights,
            ref Biome.Type[,] biomeMap,
            ref GroundType[,] groundMap,
            Vector2 terrainPosition)
        {
            int mapSize = WorldManager.TERRAIN_WIDTH * WorldManager.HEIGHT_POINT_PER_UNIT;

            // find bounds
            float xMin = float.PositiveInfinity;
            float xMax = float.NegativeInfinity;
            float yMin = float.PositiveInfinity;
            float yMax = float.NegativeInfinity;

            foreach (Vector2 vertex in vertices)
            {
                if (vertex.x < xMin) xMin = vertex.x;
                if (vertex.x > xMax) xMax = vertex.x;
                if (vertex.y < yMin) yMin = vertex.y;
                if (vertex.y > yMax) yMax = vertex.y;
            }

            Vector2 localIslandCenter = centerInWorld - terrainPosition;
            Vector2 islandCenterInHeightCoord = localIslandCenter * WorldManager.HEIGHT_POINT_PER_UNIT
                + WorldManager.BLUR_RADIUS * Vector2.one;

            int margin = (int)(BIOME_GROUND_DIST * SCALE * WorldManager.HEIGHT_POINT_PER_UNIT) + 1;

            // find height points bounds
            int uMin = Mathf.Max(0, (int)(xMin * WorldManager.HEIGHT_POINT_PER_UNIT * SCALE + islandCenterInHeightCoord.x) - margin);
            int uMax = Mathf.Min(mapSize + 2 * WorldManager.BLUR_RADIUS, (int)(xMax * WorldManager.HEIGHT_POINT_PER_UNIT * SCALE + islandCenterInHeightCoord.x) + margin);
            int vMin = Mathf.Max(0, (int)(yMin * WorldManager.HEIGHT_POINT_PER_UNIT * SCALE + islandCenterInHeightCoord.y) - margin);
            int vMax = Mathf.Min(mapSize + 2 * WorldManager.BLUR_RADIUS, (int)(yMax * WorldManager.HEIGHT_POINT_PER_UNIT * SCALE + islandCenterInHeightCoord.y) + margin);

            // For each point in the region, compute height
            for (int u = uMin; u <= uMax; u++)
            {
                // convert to island unit (/SCALE)
                float x = (u - islandCenterInHeightCoord.x - WorldManager.BLUR_RADIUS) / WorldManager.HEIGHT_POINT_PER_UNIT / SCALE;

                for (int v = vMin; v <= vMax; v++)
                {
                    // convert to island unit (/SCALE)
                    float y = (v - islandCenterInHeightCoord.y - WorldManager.BLUR_RADIUS) / WorldManager.HEIGHT_POINT_PER_UNIT / SCALE;

                    Vector2 coord = new Vector2(x, y);
                    float height = TopOrCliffPointHeight(coord, out GroundType gType, out bool isIslandBiome);
                    if (isIslandBiome)
                    {
                        float slopeHeight = 0;

                        // if not on top, test slope
                        if (height != 1)
                        {
                            slopeHeight = SlopPointHeight(coord, ref gType, ref isIslandBiome);
                        }

                        // write heightmap
                        if (u >= WorldManager.BLUR_RADIUS && v >= WorldManager.BLUR_RADIUS
                            && u <= mapSize + WorldManager.BLUR_RADIUS && v <= mapSize + WorldManager.BLUR_RADIUS)
                        {
                            terrainHeights[v - WorldManager.BLUR_RADIUS, u - WorldManager.BLUR_RADIUS] = .5f * Mathf.Max(height, slopeHeight);
                        }

                        // Alpha map is smaller than height map
                        if (u < mapSize + 2 * WorldManager.BLUR_RADIUS && v < mapSize + 2 * WorldManager.BLUR_RADIUS)
                        {
                            // write alphaMap
                            groundMap[v, u] = gType;
                            biomeMap[v, u] = biomeType;
                        }
                    }
                }
            }
        }

        private void GenerateSlopes()
        {
            if (slopes != null)
            {
                return;
            }

            slopes = new int[2];
            slopeTopOnRight = new bool[2];

            System.Random rdm = new System.Random();
            int index = rdm.Next(vertices.Count);
            DetermineSlope(0, index);
            DetermineSlope(1, (slopes[0] - 1 + vertices.Count / 2) % vertices.Count);
        }

        private void DetermineSlope(int slopeIndex, int vertexIndex)
        {
            bool nextConvex;
            bool previousConvex;

            while (true)
            {
                while (!IsConvexVertex(vertexIndex))
                {
                    vertexIndex++;
                }

                nextConvex = IsConvexVertex((vertexIndex + 1) % vertices.Count);
                previousConvex = IsConvexVertex((vertexIndex - 1 + vertices.Count) % vertices.Count);

                if (nextConvex || previousConvex)
                {
                    slopes[slopeIndex] = vertexIndex;
                    break;
                }

                vertexIndex++;
            }


            if (nextConvex && !previousConvex)
            {
                slopeTopOnRight[slopeIndex] = true;
            }
            else if (!nextConvex && previousConvex)
            {
                slopeTopOnRight[slopeIndex] = false;
            }
            else
            {
                slopeTopOnRight[slopeIndex] = new System.Random().NextDouble() < .5f;
            }
        }

        private bool IsConvexVertex(int vertexIndex)
        {
            Vector2 vec1 = vertices[(vertexIndex + 1) % vertices.Count] - vertices[vertexIndex];
            Vector2 vec2 = vertices[(vertexIndex - 1 + vertices.Count) % vertices.Count] - vertices[vertexIndex];
            Vector2 norm1 = RotateVector(vec1, Mathf.PI / 2f);

            return Vector2.Dot(vec2, norm1) >= 0;
        }

        // returns 0 if not in slope. Height between 0 and 1
        private float SlopPointHeight(Vector2 coord, ref GroundType gType, ref bool isIslandBiome)
        {
            // Create slope data if not done
            if (slopeData == null)
            {
                slopeData = new SlopeData[2];
                for (int i = 0; i < 2; i++)
                {
                    slopeData[i] = new SlopeData(
                           vertices[slopes[i]],
                           vertices[(slopes[i] + 1) % vertices.Count],
                           vertices[(slopes[i] + vertices.Count - 1) % vertices.Count],
                           slopeTopOnRight[i]
                       );

                    // Debug.Log(slopeData[i]);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                SlopeData sd = slopeData[i];
                sd.SetPoint(coord);

                // slope
                if (sd.altDotNorm1 >= 0 && sd.altDotNorm1 <= SLOPE_WIDTH)
                {
                    // Top landing
                    if (sd.dot1 >= (1 - SLOPE_LANDING) / 2f && sd.dot1 <= (1 + SLOPE_LANDING) / 2f)
                    {
                        if ((
                                sd.dotNorm1 >= (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f &&
                                sd.dotNorm1 <= (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f &&
                                sd.dot1 <= (1 + SLOPE_WAY_WIDTH) / 2f
                            ) || (
                                sd.dotNorm1 <= (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f &&
                                sd.dot1 >= (1 - SLOPE_WAY_WIDTH) / 2f &&
                                sd.dot1 <= (1 + SLOPE_WAY_WIDTH) / 2f
                            ))
                        {
                            gType = GroundType.Path;
                        }
                        else
                        {
                            gType = GroundType.Top;
                        }

                        isIslandBiome = true;
                        return 1f;
                    }

                    // First slope part
                    else if (sd.dot1 >= 0 && sd.dot1 <= (1 - SLOPE_LANDING) / 2f)
                    {
                        if (sd.altDotNorm1 >= (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f && sd.altDotNorm1 <= (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f)
                        {
                            gType = GroundType.Path;
                        }
                        else
                        {
                            gType = GroundType.Top;
                        }

                        isIslandBiome = true;
                        return SlopeEquation((1f - SLOPE_LANDING) / 2f - sd.dot1);
                    }

                }

                // turn
                if (sd.dot1 < 0 && sd.dot2 < 0 && sd.baseDistance >= sd.pathDistInTurn
                    && sd.baseDistance <= sd.pathDistInTurn + SLOPE_WIDTH)
                {
                    if (sd.baseDistance > (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f && sd.baseDistance < (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f)
                    {
                        gType = GroundType.Path;
                    }
                    else
                    {
                        gType = GroundType.Top;
                    }

                    isIslandBiome = true;
                    return SlopeEquation((1 - SLOPE_LANDING) / 2f);
                }

                // second slope part
                if (sd.altDotNorm2 >= 0 && sd.altDotNorm2 <= SLOPE_WIDTH)
                {
                    if (sd.dot2 >= 0 && sd.dot2 <= SLOPE_DESCENT)
                    {
                        if (sd.altDotNorm2 >= (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f &&
                            sd.altDotNorm2 <= (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f)
                        {
                            gType = GroundType.Path;
                        }
                        else
                        {
                            gType = GroundType.Top;
                        }

                        isIslandBiome = true;
                        return SlopeEquation((1f - SLOPE_LANDING) / 2f + sd.dot2);
                    }
                }

                // slope cliff
                if (sd.altDotNorm1 >= 0 && sd.altDotNorm1 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE)
                {
                    if (sd.dot1 >= (1 + SLOPE_LANDING) / 2f && sd.dot1 <= (1 + SLOPE_LANDING) / 2f + 1 / CLIFF_SLOPE)
                    {
                        isIslandBiome = true;

                        if (sd.altDotNorm1 <= SLOPE_WIDTH)
                        {
                            float height = 1f - (sd.dot1 - (1 + SLOPE_LANDING) / 2f) * CLIFF_SLOPE;
                            if (height > 0)
                            {
                                gType = GroundType.Cliff;
                            }
                            return height;
                        }
                        else
                        {
                            float height = 1f - sd.cornerDistance * CLIFF_SLOPE;
                            if (height > 0)
                            {
                                gType = GroundType.Cliff;
                            }
                            return height;
                        }
                    }
                    else if (sd.dot1 >= (1 - SLOPE_LANDING) / 2f && sd.dot1 <= (1 + SLOPE_LANDING) / 2f)
                    {
                        isIslandBiome = true;
                        float height = 1f - (sd.altDotNorm1 - SLOPE_WIDTH) * CLIFF_SLOPE;
                        if (height > 0)
                        {
                            gType = GroundType.Cliff;
                        }
                        return height;
                    }
                    else if (sd.dot1 >= 0 && sd.dot1 <= (1 - SLOPE_LANDING) / 2f)
                    {
                        isIslandBiome = true;
                        float height = SlopeEquation((1f - SLOPE_LANDING) / 2f - sd.dot1) - (sd.altDotNorm1 - SLOPE_WIDTH) * CLIFF_SLOPE;
                        if (height > 0)
                        {
                            gType = GroundType.Cliff;
                        }
                        return height;
                    }
                }

                // turn
                if (sd.dot1 < 0 && sd.dot2 < 0 && sd.baseDistance >= sd.pathDistInTurn + SLOPE_WIDTH
                    && sd.baseDistance <= sd.pathDistInTurn + SLOPE_WIDTH + 1 / CLIFF_SLOPE)
                {
                    isIslandBiome = true;
                    float height = SlopeEquation((1 - SLOPE_LANDING) / 2f) - (sd.baseDistance - SLOPE_WIDTH) * CLIFF_SLOPE;
                    if (height > 0)
                    {
                        gType = GroundType.Cliff;
                    }
                    return height;
                }

                if (sd.altDotNorm2 >= SLOPE_WIDTH && sd.altDotNorm2 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE
                    && sd.dot2 >= 0 && sd.dot2 <= Island.SLOPE_DESCENT)
                {
                    float height = SlopeEquation((1f - SLOPE_LANDING) / 2f + sd.dot2) - (sd.altDotNorm2 - SLOPE_WIDTH) * CLIFF_SLOPE;
                    if (height > 0)
                    {
                        isIslandBiome = true;
                        gType = GroundType.Cliff;
                    }
                    return height;
                }
            }

            return 0;
        }

        public static float SlopeEquation(float x)
        {
            return 1 - 1f / (SLOPE_DESCENT + (1f - SLOPE_LANDING) / 2f) * x;
        }

        // height between 0 and 1
        private float TopOrCliffPointHeight(Vector2 coord, out GroundType gType, out bool inIslandBiome)
        {
            float dist = DistanceFromIslandInIslandUnit(coord);
            if (dist == 0)
            {
                gType = GroundType.Top;
                inIslandBiome = true;
            }
            else if (dist <= 1 / CLIFF_SLOPE)
            {
                gType = GroundType.Cliff;
                inIslandBiome = true;
            }
            else if (dist <= BIOME_GROUND_DIST)
            {
                gType = GroundType.Ground1;
                inIslandBiome = true;
            }
            else
            {
                gType = GroundType.Ground2;
                inIslandBiome = false;
            }

            return Mathf.Max(0, 1 - dist * CLIFF_SLOPE);
        }

        private void InitGround1Function()
        {
            ground1Zone = new float[GROUND_1_FUNCTION_PARTS];
            for (int i = 0; i < GROUND_1_FUNCTION_PARTS; i++)
            {
                ground1Zone[i] = (float)rnd.NextDouble() * (GROUND_1_MAX - GROUND_1_MIN) + GROUND_1_MIN;
            }
        }

        private float Ground1Dist(float angle)
        {
            angle = (angle + 2 * (float)Math.PI) % (2 * (float)Math.PI);
            float sectorLength = 2 * (float)Math.PI / GROUND_1_FUNCTION_PARTS;
            int i = (int)(angle / sectorLength);
            float t = (angle - i * sectorLength) / sectorLength;
            float prop = (1 - (float)Math.Cos(t * Math.PI)) * .5f;
            return ground1Zone[i] * (1 - prop) + ground1Zone[(i + 1) % GROUND_1_FUNCTION_PARTS] * prop;

        }

        // coord in island units
        public bool IsInGround1(Vector2 pos)
        {
            if (vertices == null)
            {
                GenerateIslandVertices();
            }

            return pos.magnitude < Ground1Dist(Vector2.SignedAngle(Vector2.left, pos) / 180 * (float)Math.PI);
        }

        public void GenerateGround1(ref GroundType[,] groundMap, Vector2 terrainPosition)
        {
            int mapSize = WorldManager.TERRAIN_WIDTH * WorldManager.HEIGHT_POINT_PER_UNIT;

            Vector2 islandCenterInHeightCoord = (centerInWorld - terrainPosition) * WorldManager.HEIGHT_POINT_PER_UNIT
                + WorldManager.BLUR_RADIUS * Vector2.one;

            int margin = (int)(GROUND_1_MAX * SCALE * WorldManager.HEIGHT_POINT_PER_UNIT);

            // find height points bounds
            int uMin = Mathf.Max(0, (int)(islandCenterInHeightCoord.x - margin));
            int uMax = Mathf.Min(mapSize - 1 + 2 * WorldManager.BLUR_RADIUS, (int)(islandCenterInHeightCoord.x + margin));
            int vMin = Mathf.Max(0, (int)(islandCenterInHeightCoord.y - margin));
            int vMax = Mathf.Min(mapSize - 1 + 2 * WorldManager.BLUR_RADIUS, (int)(islandCenterInHeightCoord.y + margin));

            // For each point in the region
            for (int u = uMin; u <= uMax; u++)
            {
                // convert to island unit (/SCALE)
                float x = (u - islandCenterInHeightCoord.x - WorldManager.BLUR_RADIUS) / WorldManager.HEIGHT_POINT_PER_UNIT / SCALE;

                for (int v = vMin; v <= vMax; v++)
                {
                    // convert to island unit (/SCALE)
                    float y = (v - islandCenterInHeightCoord.y - WorldManager.BLUR_RADIUS) / WorldManager.HEIGHT_POINT_PER_UNIT / SCALE;

                    if (IsInGround1(new Vector2(x, y)))
                    {
                        groundMap[v, u] = GroundType.Ground1;
                    }
                }
            }
        }
    }
}

