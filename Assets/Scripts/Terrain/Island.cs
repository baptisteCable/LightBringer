using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    [Serializable]
    public class Island
    {
        private const float CLIFF_SLOPE = 3.5f;
        private const int MARGIN = 12; // margin in the weightmap for the smooth and slope
        private const float SLOPE_WIDTH = .5f;
        private const float SLOPE_LANDING = .5f;
        private const float SLOPE_DESCENT = .75f; // proportion of the second segment used for going down
        private const float SLOPE_WAY_WIDTH = .3f;

        public const float SCALE = 7f; // scale from island units to world units
        public const float MAX_POSSIBLE_RADIUS = 2.3f; // Used for rejection sampling

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
        private static System.Random rnd;

        public Island(Vector2 centerPosition, float radius, Biome.Type bt, int newSeed = 0)
        {
            centerInWorld = centerPosition;
            this.radius = radius;

            biomeType = bt;

            if (newSeed == 0)
            {
                if (rnd == null)
                {
                    rnd = new System.Random();
                }
                seed = rnd.Next();
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
            float x = v.x * Mathf.Cos(angle) - v.y * Mathf.Sin(angle);
            float y = v.x * Mathf.Sin(angle) + v.y * Mathf.Cos(angle);
            return new Vector2(x, y);
        }

        public void GenerateIslandVertices()
        {
            if (vertices != null)
            {
                return;
            }

            System.Random rdm = new System.Random(seed);

            vertices = new List<Vector2>();

            Vector2 vector = RotateVector(new Vector2(1, 0), Mathf.PI / 4f);
            vertices.Add(new Vector2(radius, 0));

            // Compute vertices
            while (vertices.Count < 5 * radius || (vertices[0] - vertices[vertices.Count - 1]).magnitude > 3f)
            {
                float angle = randomAngle(new Vector2(0, 0), radius, vector, vertices[vertices.Count - 1], rdm);
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


            float rnd = (float)rdm.NextDouble();

            i = 0;
            while (distributions[i] < rnd)
            {
                rnd -= distributions[i];
                i++;
            }

            return (i - 4) * Mathf.PI / 8f;
        }

        // return the distance to the island in island unit (1f is segment length)
        public float DistanceFromIsland(Vector2 point)
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

        public void GenerateIslandAndHeights(
            ref float[,] terrainHeights,
            Vector2 terrainPosition,
            ref List<Vector2Int> slopePoints)
        {
            // Generate island data from seed
            GenerateIslandVertices();

            GenerateHeights(ref terrainHeights, terrainPosition, ref slopePoints);

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
            int index = rdm.Next(vertices.Count) ;
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
        private float SlopPointHeight(Vector2 coord, out bool isOnSlopeWay)
        {
            isOnSlopeWay = false;

            for (int i = 0; i < 2; i++)
            {
                Vector2 vec = coord - vertices[slopes[i]];

                Vector2 vec1 = vertices[(slopes[i] + 1) % vertices.Count] - vertices[slopes[i]];
                Vector2 vec2 = vertices[(slopes[i] - 1 + vertices.Count) % vertices.Count] - vertices[slopes[i]];
                Vector2 norm1 = RotateVector(vec1, -Mathf.PI / 2f);
                Vector2 norm2 = RotateVector(vec2, Mathf.PI / 2f);

                if (slopeTopOnRight[i])
                {
                    Vector2 temp = vec1;
                    vec1 = vec2;
                    vec2 = temp;
                    temp = norm1;
                    norm1 = norm2;
                    norm2 = temp;
                }


                float dot1 = Vector2.Dot(vec1, vec);
                float dot2 = Vector2.Dot(vec2, vec);
                float dotNorm1 = Vector2.Dot(norm1, vec);
                float dotNorm2 = Vector2.Dot(norm2, vec);

                // slope
                if (dotNorm1 >= 0 && dotNorm1 <= SLOPE_WIDTH)
                {
                    // Top landing
                    if (dot1 >= (1 - SLOPE_LANDING) / 2f && dot1 <= (1 + SLOPE_LANDING) / 2f)
                    {
                        if ((
                                dotNorm1 >= (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f &&
                                dotNorm1 <= (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f &&
                                dot1 <= (1 + SLOPE_WAY_WIDTH) / 2f
                            ) || (
                                dotNorm1 <= (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f &&
                                dot1 >= (1 - SLOPE_WAY_WIDTH) / 2f &&
                                dot1 <= (1 + SLOPE_WAY_WIDTH) / 2f
                            ))
                        {
                            isOnSlopeWay = true;
                        }
                        return 1f;
                    }
                    // First slope part
                    else if (dot1 >= 0 && dot1 <= (1 - SLOPE_LANDING) / 2f)
                    {
                        if (dotNorm1 >= (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f && dotNorm1 <= (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f)
                        {
                            isOnSlopeWay = true;
                        }
                        return SlopeEquation((1f - SLOPE_LANDING) / 2f - dot1);
                    }
                    // turn
                    else if (dotNorm2 >= 0 && dotNorm2 <= SLOPE_WIDTH && dot2 < 0 && vec.magnitude <= SLOPE_WIDTH)
                    {
                        if (vec.magnitude > (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f && vec.magnitude < (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f)
                        {
                            isOnSlopeWay = true;
                        }
                        return SlopeEquation((1 - SLOPE_LANDING) / 2f);
                    }
                }

                // second slope part
                if (dotNorm2 >= 0 && dotNorm2 <= SLOPE_WIDTH)
                {
                    if (dot2 >= 0 && dot2 <= SLOPE_DESCENT)
                    {
                        if (dotNorm2 - dot2 * .3f >= (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f &&
                            dotNorm2 - dot2 * .3f <= (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f)
                        {
                            isOnSlopeWay = true;
                        }
                        return SlopeEquation((1f - SLOPE_LANDING) / 2f + dot2);
                    }
                }

                // slope cliff
                if (dotNorm1 >= 0 && dotNorm1 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE)
                {
                    if (dot1 >= (1 + SLOPE_LANDING) / 2f && dot1 <= (1 + SLOPE_LANDING) / 2f + 1 / CLIFF_SLOPE)
                    {
                        if (dotNorm1 <= SLOPE_WIDTH)
                        {
                            return 1f - (dot1 - (1 + SLOPE_LANDING) / 2f) * CLIFF_SLOPE;
                        }
                        else
                        {
                            Vector2 corner = vertices[slopes[i]] + vec1 * (1 + SLOPE_LANDING) / 2f + norm1 * SLOPE_WIDTH;
                            float cornerDist = (coord - corner).magnitude;
                            return 1f - cornerDist * CLIFF_SLOPE;
                        }
                    }
                    else if (dot1 >= (1 - SLOPE_LANDING) / 2f && dot1 <= (1 + SLOPE_LANDING) / 2f)
                    {
                        return 1f - (dotNorm1 - SLOPE_WIDTH) * CLIFF_SLOPE;
                    }
                    else if (dot1 >= 0 && dot1 <= (1 - SLOPE_LANDING) / 2f)
                    {
                        return SlopeEquation((1f - SLOPE_LANDING) / 2f - dot1) - (dotNorm1 - SLOPE_WIDTH) * CLIFF_SLOPE;
                    }
                    else if (dotNorm2 >= 0 && dotNorm2 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE && dot2 < 0 && vec.magnitude <= SLOPE_WIDTH + 1 / CLIFF_SLOPE)
                    {
                        return SlopeEquation((1 - SLOPE_LANDING) / 2f) - (vec.magnitude - SLOPE_WIDTH) * CLIFF_SLOPE;
                    }
                }

                if (dotNorm2 >= 0 && dotNorm2 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE)
                {
                    if (dot2 >= 0 && dot2 <= SLOPE_DESCENT)
                    {
                        return SlopeEquation((1f - SLOPE_LANDING) / 2f + dot2) - (dotNorm2 - SLOPE_WIDTH) * CLIFF_SLOPE;
                    }
                }

            }

            return 0;
        }

        private float SlopeEquation(float x)
        {
            return 1 - 1f / (SLOPE_DESCENT + (1f - SLOPE_LANDING) / 2f) * x;
        }

        // height between 0 and 1
        private float TopOrCliffPointHeight(Vector2 coord)
        {
            float dist = DistanceFromIsland(coord);
            return Mathf.Max(0, 1 - dist * CLIFF_SLOPE);
        }

        private void GenerateHeights(
            ref float[,] terrainHeights,
            Vector2 terrainPosition,
            ref List<Vector2Int> slopePoints)
        {
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
            Vector2 islandCenterInHeightCoord = localIslandCenter * WorldManager.HEIGHT_POINT_PER_UNIT;

            // find height points bounds
            int uMin = Mathf.Max(0, (int)(xMin * WorldManager.HEIGHT_POINT_PER_UNIT * SCALE + islandCenterInHeightCoord.x) - MARGIN);
            int uMax = Mathf.Min(WorldManager.WIDTH * WorldManager.HEIGHT_POINT_PER_UNIT, (int)(xMax * WorldManager.HEIGHT_POINT_PER_UNIT * SCALE + islandCenterInHeightCoord.x) + MARGIN);
            int vMin = Mathf.Max(0, (int)(yMin * WorldManager.HEIGHT_POINT_PER_UNIT * SCALE + islandCenterInHeightCoord.y) - MARGIN);
            int vMax = Mathf.Min(WorldManager.WIDTH * WorldManager.HEIGHT_POINT_PER_UNIT, (int)(yMax * WorldManager.HEIGHT_POINT_PER_UNIT * SCALE + islandCenterInHeightCoord.y) + MARGIN);

            // For each point in the region, compute height
            for (int u = uMin; u <= uMax; u++)
            {
                float x = (u - islandCenterInHeightCoord.x) / WorldManager.HEIGHT_POINT_PER_UNIT / SCALE;

                for (int v = vMin; v <= vMax; v++)
                {
                    // convert to island unit (/SCALE)
                    float y = (v - islandCenterInHeightCoord.y) / WorldManager.HEIGHT_POINT_PER_UNIT / SCALE;

                    Vector2 coord = new Vector2(x, y);
                    float height = TopOrCliffPointHeight(coord);
                    float slopeHeight = SlopPointHeight(coord, out bool isOnSlopeWay);

                    // add to slope points
                    if (isOnSlopeWay && slopeHeight > height && v < WorldManager.WIDTH * WorldManager.HEIGHT_POINT_PER_UNIT && u < WorldManager.WIDTH * WorldManager.HEIGHT_POINT_PER_UNIT)
                    {
                        slopePoints.Add(new Vector2Int(v, u));
                    }

                    terrainHeights[v, u] = .5f * Mathf.Max(height, slopeHeight);
                }
            }
        }
    }
}

