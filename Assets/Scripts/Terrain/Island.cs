using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    public class Island
    {
        private const float CLIFF_SLOPE = 3.5f;
        private const float RADIUS = 2.3f;
        private const float SCALE = 7f;
        private const int MARGIN = 12; // margin in the weightmap for the smooth and slope
        private const float SLOPE_WIDTH = .7f;
        private const float SLOPE_LANDING = .5f;
        private const float SLOPE_DESCENT = .75f; // proportion of the second segment used for going down
        private const float SLOPE_WAY_WIDTH = .4f;

        int seed = 0;
        Vector2 centerInWorld;

        // Index of the segment of the first slope. The second one is on the opposite side
        private int[] slopes = null;
        private bool[] slopeTopOnRight = null;

        private List<Vector2> vertices = null;

        public Island(Vector2 centerPosition, int newSeed = 0)
        {
            centerInWorld = centerPosition;

            if (newSeed == 0)
            {
                if (this.seed == 0)
                {
                    newSeed = Random.Range(0, int.MaxValue);
                }
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

        public static Vector2 RotateVector(Vector2 v, float angle)
        {
            float x = v.x * Mathf.Cos(angle) - v.y * Mathf.Sin(angle);
            float y = v.x * Mathf.Sin(angle) + v.y * Mathf.Cos(angle);
            return new Vector2(x, y);
        }

        private void GenerateIslandVertices(int seed, float radius)
        {
            if (vertices != null)
            {
                return;
            }

            Random.InitState(seed);

            vertices = new List<Vector2>();

            Vector2 vector = RotateVector(new Vector2(1, 0), Mathf.PI / 4f);
            vertices.Add(new Vector2(radius, 0));

            // Compute vertices
            while (vertices.Count < 5 * radius || (vertices[0] - vertices[vertices.Count - 1]).magnitude > 3f)
            {
                float angle = randomAngle(new Vector2(0, 0), radius, vector, vertices[vertices.Count - 1]);
                vector = RotateVector(vector, angle);
                vertices.Add(vertices[vertices.Count - 1] + vector);
            }

            // last 2 vertices
            LastTwoVertices();
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

        private float randomAngle(Vector2 center, float radius, Vector2 vector, Vector2 previousPoint)
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


            float rnd = Random.value;

            i = 0;
            while (distributions[i] < rnd)
            {
                rnd -= distributions[i];
                i++;
            }

            return (i - 4) * Mathf.PI / 8f;
        }

        // return the distanc eto the island in island unit (1f is segment length)
        private float DistanceFromIsland(Vector2 point)
        {
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
            int terrainWidth,
            int heightPointPerUnity,
            ref List<Vector2Int> slopePoints)
        {
            // Generate island data from seed
            GenerateIslandVertices(seed, RADIUS);

            GenerateSlopes();

            GenerateHeights(ref terrainHeights, terrainPosition, terrainWidth, heightPointPerUnity, ref slopePoints);

        }

        private void GenerateSlopes()
        {
            if (slopes != null)
            {
                return;
            }

            slopes = new int[2];
            slopeTopOnRight = new bool[2];

            // slopes[0]
            int index = Random.Range(0, vertices.Count);
            DetermineSlope(0, index);
            DetermineSlope(1, (slopes[0] - 1 + vertices.Count / 2) % vertices.Count);
        }

        private void DetermineSlope(int slopeIndex, int vertexIndex)
        {
            bool nextConvex = false;
            bool previousConvex = false;

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
                slopeTopOnRight[slopeIndex] = Random.value < .5f;
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
            int terrainWidth,
            int heightPointPerUnity,
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
            Vector2 islandCenterInHeightCoord = localIslandCenter * heightPointPerUnity;

            // find height points bounds
            int uMin = Mathf.Max(0, (int)(xMin * heightPointPerUnity * SCALE + islandCenterInHeightCoord.x) - MARGIN);
            int uMax = Mathf.Min(terrainWidth * heightPointPerUnity, (int)(xMax * heightPointPerUnity * SCALE + islandCenterInHeightCoord.x) + MARGIN);
            int vMin = Mathf.Max(0, (int)(yMin * heightPointPerUnity * SCALE + islandCenterInHeightCoord.y) - MARGIN);
            int vMax = Mathf.Min(terrainWidth * heightPointPerUnity, (int)(yMax * heightPointPerUnity * SCALE + islandCenterInHeightCoord.y) + MARGIN);

            // For each point in the region, compute height
            for (int u = uMin; u <= uMax; u++)
            {
                float x = (u - islandCenterInHeightCoord.x) / heightPointPerUnity / SCALE;

                for (int v = vMin; v <= vMax; v++)
                {
                    // convert to island unit (/SCALE)
                    float y = (v - islandCenterInHeightCoord.y) / heightPointPerUnity / SCALE;

                    Vector2 coord = new Vector2(x, y);
                    float height = TopOrCliffPointHeight(coord);
                    float slopeHeight = SlopPointHeight(coord, out bool isOnSlopeWay);

                    // add to slope points
                    if (isOnSlopeWay && slopeHeight > height && v < terrainWidth * heightPointPerUnity && u < terrainWidth * heightPointPerUnity)
                    {
                        slopePoints.Add(new Vector2Int(v, u));
                    }

                    terrainHeights[v, u] = .5f * Mathf.Max(height, slopeHeight);
                }
            }
        }


    }

}

