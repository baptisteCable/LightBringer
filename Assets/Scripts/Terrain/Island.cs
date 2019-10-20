using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    [Serializable]
    public class Island
    {
        public const float CLIFF_SLOPE = 3.5f;
        public const float CLIFF_BLUR = .1f;
        public const float SLOPE_WIDTH = .5f;
        public const float SLOPE_LANDING = .5f;
        public const float SLOPE_DESCENT = .75f; // proportion of the second segment used for going down
        private const float SLOPE_WAY_WIDTH = .2f;
        private const float BIOME_GROUND_DIST = 1.5f; // Distance for ground 1 around island

        public const float MAX_RADIUS = 3.75f; // Update with new types of islands

        public const float SCALE = 7f; // scale from island units to world units

        private const int GROUND_1_FUNCTION_PARTS = 12;
        private float ground1Min;
        private float ground1Max;

        int seed;

        int type;

        public Biome.Type biomeType;

        public Vector2 centerInWorld;

        // Index of the segment of the slopes
        [NonSerialized]
        private int[] slopes = null;
        [NonSerialized]
        private bool[] slopeTopOnRight = null;

        [NonSerialized]
        private List<Vector2> vertices = null;

        [NonSerialized]
        private Vector2[] circleCenters;

        [NonSerialized]
        private float[] circleRadius;

        [NonSerialized]
        private int[] circleOrder;

        [NonSerialized]
        private System.Random rnd;

        private static System.Random staticRnd;

        // Optimize the slope computation for heights and alphaMap
        [NonSerialized]
        private SlopeData[] slopeData;

        [NonSerialized]
        private float[] ground1Zone;

        public Island (Vector2 centerPosition, Biome.Type bt, int type, int newSeed = 0)
        {
            centerInWorld = centerPosition;
            this.type = type;

            biomeType = bt;

            if (newSeed == 0)
            {
                if (staticRnd == null)
                {
                    staticRnd = new System.Random ();
                }
                seed = staticRnd.Next ();
            }
            else
            {
                seed = newSeed;
            }
        }

        public float GetAvgRadius ()
        {
            return GetAvgRadius (type);
        }

        static public float GetAvgRadius (int type)
        {
            switch (type)
            {
                case 0: return 2.1f;
                case 1: return 3.75f;
            }

            throw new Exception ("No valid type");
        }

        public static Vector2 Vector2FromAngle (float a)
        {
            return new Vector2 (Mathf.Cos (a), Mathf.Sin (a));
        }

        // radian angle
        public static Vector2 RotateVector (Vector2 v, float angle)
        {
            return RotateVector (v, (double)angle);
        }

        public static Vector2 RotateVector (Vector2 v, double angle)
        {
            double x = v.x * Math.Cos (angle) - v.y * Math.Sin (angle);
            double y = v.x * Math.Sin (angle) + v.y * Math.Cos (angle);
            return new Vector2 ((float)x, (float)y);
        }

        public void GenerateIslandVertices ()
        {
            if (vertices != null)
            {
                return;
            }

            rnd = new System.Random (seed);

            InitCircles ();

            // Ground 1 function
            InitGround1Function ();

            vertices = new List<Vector2> ();

            int c = 0;
            Vector2 vector = RotateVector (new Vector2 (1, 0), Mathf.PI / 4f);
            vertices.Add (circleCenters[circleOrder[c]] + new Vector2 (circleRadius[circleOrder[c]], 0));

            // Compute vertices
            while (vertices.Count < 10 || c < circleOrder.Length - 1 || (vertices[0] - vertices[vertices.Count - 1]).magnitude > 3f)
            {
                DetermineCurrentCircleOrder (ref c);

                float angle = randomAngle (
                    circleCenters[circleOrder[c]],
                    circleRadius[circleOrder[c]],
                    vector,
                    vertices[vertices.Count - 1],
                    rnd);
                vector = RotateVector (vector, angle);
                vertices.Add (vertices[vertices.Count - 1] + vector);
            }

            // last 2 vertices
            LastTwoVertices ();

            // Rotate island
            RotateIsland (rnd);

            // slopes
            GenerateSlopes ();
        }

        private void RotateIsland (System.Random rnd)
        {
            double angle = rnd.NextDouble () * Math.PI * 2;
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = RotateVector (vertices[i], angle);
            }
        }

        private void DetermineCurrentCircleOrder (ref int c)
        {
            while (c + 1 < circleOrder.Length)
            {
                float distToCurrent = (vertices[vertices.Count - 1] - circleCenters[circleOrder[c]]).magnitude / circleRadius[circleOrder[c]];
                float distToNext = (vertices[vertices.Count - 1] - circleCenters[circleOrder[c + 1]]).magnitude / circleRadius[circleOrder[c + 1]];

                if (distToNext <= distToCurrent)
                {
                    c++;
                }
                else
                {
                    break;
                }
            }
        }

        private void InitCircles ()
        {
            switch (type)
            {
                case 0:
                    circleCenters = new Vector2[1];
                    circleCenters[0] = Vector2.zero;
                    circleRadius = new float[1];
                    circleRadius[0] = 2.1f;
                    circleOrder = new int[1];
                    circleOrder[0] = 0;
                    slopes = new int[2];
                    ground1Min = 4;
                    ground1Max = 8;
                    break;
                case 1:
                    circleCenters = new Vector2[3];
                    circleCenters[0] = new Vector2 (1.25f, 0);
                    circleCenters[1] = new Vector2 (0, 0);
                    circleCenters[2] = new Vector2 (-1.25f, 0);
                    circleRadius = new float[3];
                    circleRadius[0] = 2.5f;
                    circleRadius[1] = 2.5f;
                    circleRadius[2] = 2.5f;
                    circleOrder = new int[5];
                    circleOrder[0] = 0;
                    circleOrder[1] = 1;
                    circleOrder[2] = 2;
                    circleOrder[3] = 1;
                    circleOrder[4] = 0;
                    slopes = new int[3];
                    ground1Min = 5;
                    ground1Max = 10;
                    break;
                default:
                    throw new Exception ("Invalid Island type : " + type);

            }
        }

        private void LastTwoVertices ()
        {
            Vector2 last = vertices[vertices.Count - 1];
            Vector2 first = vertices[0];

            Vector2 vector = first - last;
            float distance = vector.magnitude;
            vector /= distance;

            float angle = -Mathf.Acos ((distance - 1) / 2);

            // add vertex 1
            vertices.Add (vertices[vertices.Count - 1] + RotateVector (vector, angle));

            // add vertex 2
            vertices.Add (vertices[vertices.Count - 1] + vector);
        }

        private float angleDistrib (float angle)
        {
            return 3 - Mathf.Abs (angle);
        }

        private float distDistrib (float angle, Vector2 center, float radius, Vector2 vector, Vector2 previousPoint)
        {
            Vector2 newVector = RotateVector (vector, angle);

            // 0 if going back
            Vector2 tangent = RotateVector (previousPoint - center, Mathf.PI / 2f);
            if (Vector2.Dot (tangent, newVector) < 0)
            {
                return 0;
            }

            // compute distribution
            float dist = (center - (previousPoint + newVector)).magnitude;

            if (dist < radius / 2)
            {
                return 0;
            }

            float ratio = dist / radius - 1;
            return Mathf.Exp (-6 * ratio * ratio);
        }

        private float randomAngle (Vector2 center, float radius, Vector2 vector, Vector2 previousPoint, System.Random rdm)
        {
            float[] distributions = new float[9];
            float sum = 0f;
            int i;

            // compute raw distributions
            for (i = 0; i < 9; i++)
            {
                float angle = (i - 4) * Mathf.PI / 8f;
                distributions[i] = angleDistrib (angle) * distDistrib (angle, center, radius, vector, previousPoint);
                sum += distributions[i];
            }

            // normalize
            for (i = 0; i < 9; i++)
            {
                distributions[i] /= sum;
            }


            double rnd = rdm.NextDouble ();

            i = 0;
            while (distributions[i] < rnd)
            {
                rnd -= distributions[i];
                i++;
            }

            return (i - 4) * Mathf.PI / 8f;
        }

        // return the distance to the island in island unit (1f is segment length)
        public float DistanceFromIslandInIslandUnit (Vector2 point, bool insideNegative = false)
        {
            if (vertices == null)
            {
                GenerateIslandVertices ();
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

            Vector2 normal = RotateVector (second - first, -Mathf.PI / 2f);
            Vector2 vect = point - first;

            float dotProdTangent = Vector2.Dot (second - first, vect);
            float dotProdNormal = Vector2.Dot (normal, vect);

            if (dotProdNormal <= 0)
            {
                if (insideNegative)
                {
                    return dotProdNormal;
                }
                else
                {
                    return 0;
                }
            }
            // external angle case (circle dist)
            else if (dotProdTangent < 0 || dotProdTangent > 1)
            {
                return minDist;
            }
            else
            {
                return dotProdNormal;
            }
        }

        public float DistanceFromIslandInWorldUnit (Vector2 worldPos)
        {
            Vector2 point = (worldPos - centerInWorld) / SCALE;
            return SCALE * DistanceFromIslandInIslandUnit (point);
        }

        public void GenerateIslandHeightsAndAlphaMap (
            ref float[,,] alphaMap,
            ref float[,] terrainHeights,
            ref Biome.Type[,] biomeMap,
            ref GroundType[,] groundMap,
            Vector2 terrainPosition)
        {
            // Generate island data from seed
            if (vertices == null)
            {
                GenerateIslandVertices ();
            }

            GenerateHeights (ref terrainHeights, terrainPosition);
            GenerateAlphaMap (ref alphaMap, ref biomeMap, ref groundMap, terrainPosition);
        }

        private void GetBounds (int mapSize, Vector2 terrainPosition, int margin,
            out Vector2 islandCenterInIntCoord, out float pointPerUnit, out int uMin, out int uMax, out int vMin, out int vMax)
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
            pointPerUnit = (mapSize - 1) / (float)WorldManager.TERRAIN_WIDTH;
            islandCenterInIntCoord = localIslandCenter * pointPerUnit + WorldManager.BLUR_RADIUS * Vector2.one;

            // find height points bounds
            uMin = Mathf.Max (0, (int)(xMin * pointPerUnit * SCALE + islandCenterInIntCoord.x) - margin);
            uMax = Mathf.Min (mapSize + 2 * WorldManager.BLUR_RADIUS,
                            (int)Math.Ceiling (xMax * pointPerUnit * SCALE + islandCenterInIntCoord.x) + margin);
            vMin = Mathf.Max (0, (int)(yMin * pointPerUnit * SCALE + islandCenterInIntCoord.y) - margin);
            vMax = Mathf.Min (mapSize + 2 * WorldManager.BLUR_RADIUS,
                            (int)Math.Ceiling (yMax * pointPerUnit * SCALE + islandCenterInIntCoord.y) + margin);
        }

        private void GenerateHeights (
            ref float[,] terrainHeights,
            Vector2 terrainPosition)
        {
            int mapSize = WorldManager.TERRAIN_WIDTH * WorldManager.HEIGHT_POINT_PER_UNIT + 1;
            int margin = (int)(BIOME_GROUND_DIST * SCALE * WorldManager.HEIGHT_POINT_PER_UNIT);
            GetBounds (mapSize, terrainPosition, margin,
                out Vector2 islandCenterInHeightCoord, out float pointPerUnit, out int uMin, out int uMax, out int vMin, out int vMax);

            // For each point in the region, compute height
            for (int u = uMin; u <= uMax; u++)
            {
                // convert to island unit (/SCALE)
                float x = (u - islandCenterInHeightCoord.x - WorldManager.BLUR_RADIUS) / pointPerUnit / SCALE;

                for (int v = vMin; v <= vMax; v++)
                {
                    // convert to island unit (/SCALE)
                    float y = (v - islandCenterInHeightCoord.y - WorldManager.BLUR_RADIUS) / pointPerUnit / SCALE;

                    Vector2 coord = new Vector2 (x, y);
                    float height = TopOrCliffPointHeight (coord, out bool isIslandBiome);
                    if (isIslandBiome)
                    {
                        float slopeHeight = 0;

                        // if not on top, test slope
                        if (height != 1)
                        {
                            slopeHeight = SlopPointHeight (coord);
                        }

                        // write heightmap
                        if (u >= WorldManager.BLUR_RADIUS && v >= WorldManager.BLUR_RADIUS
                            && u < mapSize + WorldManager.BLUR_RADIUS && v < mapSize + WorldManager.BLUR_RADIUS)
                        {
                            terrainHeights[v - WorldManager.BLUR_RADIUS, u - WorldManager.BLUR_RADIUS] = .5f * Mathf.Max (height, slopeHeight);
                        }
                    }
                }
            }
        }

        private void GenerateAlphaMap (
            ref float[,,] alphaMap,
            ref Biome.Type[,] biomeMap,
            ref GroundType[,] groundMap,
            Vector2 terrainPosition)
        {
            int mapSize = WorldManager.TERRAIN_WIDTH * WorldManager.HEIGHT_POINT_PER_UNIT;
            int margin = (int)(BIOME_GROUND_DIST * SCALE * WorldManager.HEIGHT_POINT_PER_UNIT);
            GetBounds (mapSize, terrainPosition, margin,
                out Vector2 islandCenterInIntCoord, out float pointPerUnit, out int uMin, out int uMax, out int vMin, out int vMax);

            // For each point in the region, compute height
            for (int u = uMin; u < uMax; u++)
            {
                // convert to island unit (/SCALE)
                float x = (u - islandCenterInIntCoord.x - WorldManager.BLUR_RADIUS) / pointPerUnit / SCALE;

                for (int v = vMin; v < vMax; v++)
                {
                    // convert to island unit (/SCALE)
                    float y = (v - islandCenterInIntCoord.y - WorldManager.BLUR_RADIUS) / pointPerUnit / SCALE;

                    Vector2 coord = new Vector2 (x, y);
                    BotTopCliffBlend (coord, u, v, ref alphaMap, ref groundMap, out bool isIslandBiome);
                    if (isIslandBiome)
                    {
                        // if not on top, test slope
                        if (groundMap[v, u] != GroundType.Top)
                        {
                            SlopBlend (coord, u, v, ref alphaMap, ref groundMap);
                        }

                        biomeMap[v, u] = biomeType;
                    }
                }
            }
        }

        private void GenerateSlopes ()
        {
            slopeTopOnRight = new bool[slopes.Length];

            // Find all good vertex for slopes (convex)
            // List
            List<int> goodVertices = new List<int> ();
            for (int i = 0; i < vertices.Count; i++)
            {
                if (
                    IsConvexVertex (i) &&
                    (
                        IsConvexVertex ((i + 1) % vertices.Count) ||
                        IsConvexVertex ((i - 1 + vertices.Count) % vertices.Count)
                    )
                )
                {
                    goodVertices.Add (i);
                }
            }

            int largestStep = 0;
            int largestStepIndex = 0;
            for (int i = 0; i < goodVertices.Count; i++)
            {
                int step = (goodVertices[i] - goodVertices[(i - 1 + goodVertices.Count) % goodVertices.Count] + vertices.Count)
                    % vertices.Count;
                if (step > largestStep)
                {
                    largestStep = step;
                    largestStepIndex = i;
                }
            }

            // In this list, find a good equirepartition of all the slopes (middle of each part)
            // first is after largest step
            slopes[0] = goodVertices[largestStepIndex];

            // Next are dispatched around the island
            int index = largestStepIndex + 1;
            int lastSlope = 0;
            float partWidth = vertices.Count / (float)slopes.Length;

            while (index - largestStepIndex < goodVertices.Count)
            {
                int vertexIndex = goodVertices[index % goodVertices.Count];

                int part = (int)((vertexIndex - slopes[0] + vertices.Count + partWidth / 2f)
                    % vertices.Count / partWidth);

                if (part == 0)
                {
                    index++;
                    continue;
                }

                // add a new slope
                // include the case where the previous parts were not satisfied
                // prevent 2 consecutive slopes
                if (part > lastSlope && (vertexIndex - slopes[lastSlope] + vertices.Count) % vertices.Count > 2)
                {
                    lastSlope++;
                    slopes[lastSlope] = vertexIndex;
                }
                // move last slope if closer to ideal position
                else
                {
                    float d1 = ModuloDistance (vertexIndex, slopes[0] + part * partWidth, vertices.Count);
                    float d2 = ModuloDistance (slopes[part], slopes[0] + part * partWidth, vertices.Count);

                    if (d1 < d2)
                    {
                        slopes[part] = vertexIndex;
                    }
                }

                index++;
            }

            // Create slopes in a random direction at these points
            for (int i = 0; i < slopes.Length; i++)
            {
                bool nextConvex = IsConvexVertex ((slopes[i] + 1) % vertices.Count);
                bool previousConvex = IsConvexVertex ((slopes[i] - 1 + vertices.Count) % vertices.Count);

                if (!previousConvex)
                {
                    slopeTopOnRight[i] = true;
                }
                else if (!nextConvex)
                {
                    slopeTopOnRight[i] = false;
                }
                else
                {
                    slopeTopOnRight[i] = rnd.NextDouble () < .5f;
                }
            }
        }

        private float ModuloDistance (float a, float b, float modulo)
        {
            float dist = (a - b + 2 * modulo) % modulo;
            return Math.Min (dist, modulo - dist);
        }

        private bool IsConvexVertex (int vertexIndex)
        {
            Vector2 vec1 = vertices[(vertexIndex + 1) % vertices.Count] - vertices[vertexIndex];
            Vector2 vec2 = vertices[(vertexIndex - 1 + vertices.Count) % vertices.Count] - vertices[vertexIndex];
            Vector2 norm1 = RotateVector (vec1, Mathf.PI / 2f);

            return Vector2.Dot (vec2, norm1) >= 0;
        }

        // returns 0 if not in slope. Height between 0 and 1
        private float SlopPointHeight (Vector2 coord)
        {
            // Create slope data if not done
            if (slopeData == null)
            {
                slopeData = new SlopeData[slopes.Length];
                for (int i = 0; i < slopes.Length; i++)
                {
                    slopeData[i] = new SlopeData (
                           vertices[slopes[i]],
                           vertices[(slopes[i] + 1) % vertices.Count],
                           vertices[(slopes[i] + vertices.Count - 1) % vertices.Count],
                           slopeTopOnRight[i]
                       );
                }
            }

            for (int i = 0; i < slopes.Length; i++)
            {
                SlopeData sd = slopeData[i];
                sd.SetPoint (coord);

                // slope
                if (sd.altDotNorm1 >= 0 && sd.altDotNorm1 <= SLOPE_WIDTH)
                {
                    // Top landing
                    if (sd.dot1 >= (1 - SLOPE_LANDING) / 2f && sd.dot1 <= (1 + SLOPE_LANDING) / 2f)
                    {
                        return 1f;
                    }

                    // First slope part
                    else if (sd.dot1 >= 0 && sd.dot1 <= (1 - SLOPE_LANDING) / 2f)
                    {
                        return SlopeEquation ((1f - SLOPE_LANDING) / 2f - sd.dot1);
                    }

                }

                // turn
                if (sd.dot1 < 0 && sd.dot2 < 0 && sd.baseDistance >= sd.slopeDistInTurn
                    && sd.baseDistance <= sd.slopeDistInTurn + SLOPE_WIDTH)
                {
                    return SlopeEquation ((1 - SLOPE_LANDING) / 2f);
                }

                // second slope part
                if (sd.altDotNorm2 >= 0 && sd.altDotNorm2 <= SLOPE_WIDTH)
                {
                    if (sd.dot2 >= 0 && sd.dot2 <= SLOPE_DESCENT)
                    {
                        return SlopeEquation ((1f - SLOPE_LANDING) / 2f + sd.dot2);
                    }
                }

                // slope cliff
                if (sd.altDotNorm1 >= 0 && sd.altDotNorm1 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE)
                {
                    if (sd.dot1 >= (1 + SLOPE_LANDING) / 2f && sd.dot1 <= (1 + SLOPE_LANDING) / 2f + 1 / CLIFF_SLOPE)
                    {
                        if (sd.altDotNorm1 <= SLOPE_WIDTH)
                        {
                            float height = 1f - (sd.dot1 - (1 + SLOPE_LANDING) / 2f) * CLIFF_SLOPE;
                            return height;
                        }
                        else
                        {
                            float height = 1f - sd.cornerDistance * CLIFF_SLOPE;
                            return height;
                        }
                    }
                    else if (sd.dot1 >= (1 - SLOPE_LANDING) / 2f && sd.dot1 <= (1 + SLOPE_LANDING) / 2f)
                    {
                        float height = 1f - (sd.altDotNorm1 - SLOPE_WIDTH) * CLIFF_SLOPE;
                        return height;
                    }
                    else if (sd.dot1 >= 0 && sd.dot1 <= (1 - SLOPE_LANDING) / 2f)
                    {
                        float height = SlopeEquation ((1f - SLOPE_LANDING) / 2f - sd.dot1) - (sd.altDotNorm1 - SLOPE_WIDTH) * CLIFF_SLOPE;
                        return height;
                    }
                }

                // turn cliff
                if (sd.dot1 < 0 && sd.dot2 < 0 && sd.baseDistance >= sd.slopeDistInTurn + SLOPE_WIDTH
                    && sd.baseDistance <= sd.slopeDistInTurn + SLOPE_WIDTH + 1 / CLIFF_SLOPE)
                {
                    float height = SlopeEquation ((1 - SLOPE_LANDING) / 2f) - (sd.baseDistance - sd.slopeDistInTurn - SLOPE_WIDTH) * CLIFF_SLOPE;
                    return height;
                }

                if (sd.altDotNorm2 >= SLOPE_WIDTH && sd.altDotNorm2 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE
                    && sd.dot2 >= 0 && sd.dot2 <= Island.SLOPE_DESCENT)
                {
                    float height = SlopeEquation ((1f - SLOPE_LANDING) / 2f + sd.dot2) - (sd.altDotNorm2 - SLOPE_WIDTH) * CLIFF_SLOPE;
                    return height;
                }
            }

            return 0;
        }

        // returns 0 if not in slope. Height between 0 and 1
        private void SlopBlend (Vector2 coord, int u, int v, ref float[,,] alphaMap, ref GroundType[,] groundMap)
        {
            for (int i = 0; i < slopes.Length; i++)
            {
                SlopeData sd = slopeData[i];
                sd.SetPoint (coord);

                // Top landing
                if (sd.dot1 >= (1 - SLOPE_LANDING) / 2f && sd.dot1 <= (1 + SLOPE_LANDING) / 2f)
                {
                    // Path
                    if (
                            sd.altDotNorm1 <= (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f &&
                            sd.dot1 <= (1 + SLOPE_WAY_WIDTH) / 2f &&
                            (
                                sd.altDotNorm1 >= (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f ||
                                (
                                    sd.dot1 >= (1 - SLOPE_WAY_WIDTH) / 2f &&
                                    sd.dotNorm1 >= -CLIFF_BLUR
                                )
                            )
                        )
                    {
                        groundMap[v, u] = GroundType.Path;
                        ClearPaint (ref alphaMap, u, v);
                    }

                    // Top
                    else if (
                            sd.altDotNorm1 <= SLOPE_WIDTH - CLIFF_BLUR &&
                            sd.dot1 <= (1 + SLOPE_LANDING) - CLIFF_BLUR / 2f &&
                            (
                                sd.altDotNorm1 >= CLIFF_BLUR ||
                                (
                                    sd.dot1 >= (1 - SLOPE_LANDING) / 2f &&
                                    sd.dotNorm1 >= -CLIFF_BLUR
                                )
                            )
                        )
                    {
                        groundMap[v, u] = GroundType.Path;
                        ClearPaint (ref alphaMap, u, v);
                    }

                    // Top cliff blur 1
                    else if (
                            sd.dot1 <= (1 - SLOPE_LANDING) / 2f + CLIFF_BLUR &&
                            sd.altDotNorm1 <= CLIFF_BLUR &&
                            sd.dotNorm1 >= 0
                        )
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float topProp = (sd.dot1 - (1 - SLOPE_LANDING) / 2f) / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, topProp, 1 - topProp, 0);
                    }

                    // Top cliff blur 2
                    else if (
                            sd.dot1 >= (1 + SLOPE_LANDING) / 2f - CLIFF_BLUR &&
                            sd.altDotNorm1 <= SLOPE_WIDTH &&
                            sd.dotNorm1 >= 0
                        )
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float topProp = Math.Min (
                                ((1 + SLOPE_LANDING) / 2f - sd.dot1) / CLIFF_BLUR,
                                (SLOPE_WIDTH - sd.altDot1) / CLIFF_BLUR
                            );
                        PaintCliff (ref alphaMap, u, v, biomeType, topProp, 1 - topProp, 0);
                    }

                    // Bot cliff blur
                    else if (
                        sd.dot1 <= (1 + SLOPE_LANDING) / 2f - CLIFF_BLUR &&
                        sd.altDotNorm1 > SLOPE_WIDTH - CLIFF_BLUR &&
                        sd.altDotNorm1 <= SLOPE_WIDTH)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float topProp = (SLOPE_WIDTH - sd.altDotNorm1) / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, topProp, 1 - topProp, 0);
                    }
                    // Bot cliff
                    else if (sd.altDotNorm1 > SLOPE_WIDTH && sd.altDotNorm1 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1, 0);
                    }
                    // Bot cliff blur
                    else if (sd.altDotNorm1 > SLOPE_WIDTH + 1 / CLIFF_SLOPE && sd.altDotNorm1 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE + CLIFF_BLUR)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float botProp = (sd.altDotNorm1 - SLOPE_WIDTH - 1 / CLIFF_SLOPE) / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1 - botProp, botProp);
                    }
                }
                // Top landing side cliff
                else if (sd.dot1 > (1 + SLOPE_LANDING) / 2f &&
                    sd.dot1 <= (1 + SLOPE_LANDING) / 2f + 1 / CLIFF_SLOPE + CLIFF_BLUR &&
                    sd.dotNorm1 >= 0
                    )
                {
                    // Before corner
                    if (sd.altDotNorm1 <= SLOPE_WIDTH)
                    {
                        // cliff
                        if (sd.dotNorm1 > 1 / CLIFF_SLOPE)
                        {
                            groundMap[v, u] = GroundType.Cliff;
                            PaintCliff (ref alphaMap, u, v, biomeType, 0, 1, 0);
                        }
                        // blur
                        else if (groundMap[v, u] != GroundType.Cliff)
                        {
                            groundMap[v, u] = GroundType.Cliff;
                            float botProp = (sd.dot1 - (1 + SLOPE_LANDING) / 2f - 1 / CLIFF_SLOPE) / CLIFF_BLUR;
                            PaintCliff (ref alphaMap, u, v, biomeType, 0, 1 - botProp, botProp);
                        }

                    }
                    // corner cliff
                    else if (sd.cornerDistance <= 1 / CLIFF_SLOPE)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1, 0);
                    }
                    // corner cliff blur
                    else if (sd.cornerDistance <= 1 / CLIFF_SLOPE + CLIFF_BLUR && groundMap[v, u] != GroundType.Cliff)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float botProp = (sd.cornerDistance - 1 / CLIFF_SLOPE) / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1 - botProp, botProp);
                    }
                }

                // Slope 1
                else if (sd.dot1 >= 0 && sd.dot1 < (1 - SLOPE_LANDING) / 2f)
                {
                    // Path
                    if (sd.altDotNorm1 >= (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f &&
                        sd.altDotNorm1 <= (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f)
                    {
                        groundMap[v, u] = GroundType.Path;
                        ClearPaint (ref alphaMap, u, v);
                    }
                    // Top
                    else if (sd.altDotNorm1 >= CLIFF_BLUR &&
                        sd.altDotNorm1 <= SLOPE_WIDTH - CLIFF_BLUR)
                    {
                        groundMap[v, u] = GroundType.Top;
                        ClearPaint (ref alphaMap, u, v);
                    }
                    // Top cliff blur
                    else if (sd.altDotNorm1 >= 0 &&
                        sd.altDotNorm1 < CLIFF_BLUR)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float topProp = sd.altDotNorm1 / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, topProp, 1 - topProp, 0);
                    }
                    // Bot cliff blur
                    else if (sd.altDotNorm1 > SLOPE_WIDTH - CLIFF_BLUR &&
                        sd.altDotNorm1 <= SLOPE_WIDTH)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float topProp = (SLOPE_WIDTH - sd.altDotNorm1) / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, topProp, 1 - topProp, 0);
                    }
                    // Top cliff
                    else if (sd.dotNorm1 >= 0 && sd.altDotNorm1 <= 0)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1, 0);
                    }
                    // Bot cliff
                    else if (sd.altDotNorm1 > SLOPE_WIDTH && sd.dotNorm1 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1, 0);
                    }
                    // Bot cliff blur
                    else if (sd.dotNorm1 > SLOPE_WIDTH + 1 / CLIFF_SLOPE && sd.dotNorm1 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE + CLIFF_BLUR)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float botProp = (sd.dotNorm1 - SLOPE_WIDTH - 1 / CLIFF_SLOPE) / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1 - botProp, botProp);
                    }
                }

                // Turn
                else if (sd.dot1 < 0 && sd.dot2 < 0)
                {
                    // Path
                    if (sd.baseDistance >= sd.slopeDistInTurn + (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f &&
                        sd.baseDistance <= sd.slopeDistInTurn + (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f)
                    {
                        groundMap[v, u] = GroundType.Path;
                        ClearPaint (ref alphaMap, u, v);
                    }
                    // Top
                    else if (sd.baseDistance >= sd.slopeDistInTurn + CLIFF_BLUR &&
                        sd.baseDistance <= sd.slopeDistInTurn + SLOPE_WIDTH - CLIFF_BLUR)
                    {
                        groundMap[v, u] = GroundType.Top;
                        ClearPaint (ref alphaMap, u, v);
                    }
                    // Top cliff blur
                    else if (sd.baseDistance >= sd.slopeDistInTurn &&
                        sd.baseDistance < sd.slopeDistInTurn + CLIFF_BLUR)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float topProp = (sd.baseDistance - sd.slopeDistInTurn) / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, topProp, 1 - topProp, 0);
                    }
                    // Bot cliff blur
                    else if (sd.baseDistance > sd.slopeDistInTurn + SLOPE_WIDTH - CLIFF_BLUR &&
                        sd.baseDistance <= sd.slopeDistInTurn + SLOPE_WIDTH)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float topProp = (sd.slopeDistInTurn + SLOPE_WIDTH - sd.baseDistance) / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, topProp, 1 - topProp, 0);
                    }
                    // Top cliff
                    else if (sd.baseDistance >= 0 && sd.baseDistance <= sd.slopeDistInTurn)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1, 0);
                    }
                    // Bot cliff
                    else if (sd.baseDistance > sd.slopeDistInTurn + SLOPE_WIDTH && sd.baseDistance <= SLOPE_WIDTH + 1 / CLIFF_SLOPE)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1, 0);
                    }
                    // Bot cliff blur
                    else if (sd.baseDistance > SLOPE_WIDTH + 1 / CLIFF_SLOPE && sd.baseDistance <= SLOPE_WIDTH + 1 / CLIFF_SLOPE + CLIFF_BLUR)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float botProp = (sd.baseDistance - SLOPE_WIDTH - 1 / CLIFF_SLOPE) / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1 - botProp, botProp);
                    }
                }

                // Slope 2
                else if (sd.dot2 >= 0 && sd.dot2 <= SLOPE_DESCENT)
                {
                    // Path
                    if (sd.altDotNorm2 >= (SLOPE_WIDTH - SLOPE_WAY_WIDTH) / 2f &&
                        sd.altDotNorm2 <= (SLOPE_WIDTH + SLOPE_WAY_WIDTH) / 2f)
                    {
                        groundMap[v, u] = GroundType.Path;
                        ClearPaint (ref alphaMap, u, v);
                    }
                    // Top
                    else if (sd.altDotNorm2 >= CLIFF_BLUR &&
                        sd.altDotNorm2 <= SLOPE_WIDTH - CLIFF_BLUR)
                    {
                        groundMap[v, u] = GroundType.Top;
                        ClearPaint (ref alphaMap, u, v);
                    }
                    // Top cliff blur
                    else if (sd.altDotNorm2 >= 0 &&
                        sd.altDotNorm2 < CLIFF_BLUR)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float topProp = sd.altDotNorm2 / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, topProp, 1 - topProp, 0);
                    }
                    // Bot cliff blur
                    else if (sd.altDotNorm2 > SLOPE_WIDTH - CLIFF_BLUR &&
                        sd.altDotNorm2 <= SLOPE_WIDTH)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float topProp = (SLOPE_WIDTH - sd.altDotNorm2) / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, topProp, 1 - topProp, 0);
                    }
                    // Top cliff
                    else if (sd.dotNorm2 >= 0 && sd.altDotNorm2 <= 0)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1, 0);
                    }
                    // Bot cliff
                    else if (sd.altDotNorm2 > SLOPE_WIDTH && sd.dotNorm2 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1, 0);
                    }
                    // Bot cliff blur
                    else if (sd.dotNorm2 > SLOPE_WIDTH + 1 / CLIFF_SLOPE && sd.dotNorm2 <= SLOPE_WIDTH + 1 / CLIFF_SLOPE + CLIFF_BLUR)
                    {
                        groundMap[v, u] = GroundType.Cliff;
                        float botProp = (sd.dotNorm2 - SLOPE_WIDTH - 1 / CLIFF_SLOPE) / CLIFF_BLUR;
                        PaintCliff (ref alphaMap, u, v, biomeType, 0, 1 - botProp, botProp);
                    }
                }
            }
        }

        // x is the descent position. 0 for the top. The turn is at (1 - SLOPE_LANDING) / 2f
        public static float SlopeEquation (float x)
        {
            return 1 - 1f / (SLOPE_DESCENT + (1f - SLOPE_LANDING) / 2f) * x;
        }

        // x is the descent position. 0 for the top. The turn is at (1 - SLOPE_LANDING) / 2f
        public float CliffWidth (float x)
        {
            return Island.SlopeEquation (x) / Island.CLIFF_SLOPE;
        }

        // height between 0 and 1
        private float TopOrCliffPointHeight (Vector2 coord, out bool inIslandBiome)
        {
            float dist = DistanceFromIslandInIslandUnit (coord);
            inIslandBiome = dist <= BIOME_GROUND_DIST;
            return Mathf.Max (0, 1 - dist * CLIFF_SLOPE);
        }

        // height between 0 and 1
        private void BotTopCliffBlend (Vector2 coord, int u, int v, ref float[,,] alphaMap, ref GroundType[,] groundMap, out bool inIslandBiome)
        {
            float dist = DistanceFromIslandInIslandUnit (coord, true);
            if (dist <= -CLIFF_BLUR)
            {
                groundMap[v, u] = GroundType.Top;
                inIslandBiome = true;
            }
            // Cliff blur top
            else if (dist < 0)
            {
                groundMap[v, u] = GroundType.Cliff;
                float cliffProp = (dist + CLIFF_BLUR) / CLIFF_BLUR;
                PaintCliff (ref alphaMap, u, v, biomeType, 1 - cliffProp, cliffProp, 0);
                inIslandBiome = true;
            }
            else if (dist <= 1 / CLIFF_SLOPE)
            {
                groundMap[v, u] = GroundType.Cliff;
                PaintCliff (ref alphaMap, u, v, biomeType, 0, 1, 0);
                inIslandBiome = true;
            }
            else if (dist < 1 / CLIFF_SLOPE + CLIFF_BLUR)
            {
                groundMap[v, u] = GroundType.Cliff;
                float botProp = (dist - 1 / CLIFF_SLOPE) / CLIFF_BLUR;
                PaintCliff (ref alphaMap, u, v, biomeType, 0, 1 - botProp, botProp);
                inIslandBiome = true;
            }
            else if (dist <= BIOME_GROUND_DIST)
            {
                groundMap[v, u] = GroundType.Ground1;
                inIslandBiome = true;
            }
            else
            {
                inIslandBiome = false;
            }
        }

        void ClearPaint (ref float[,,] alphaMap, int u, int v)
        {
            PaintCliff (ref alphaMap, u, v, biomeType, 0, 0, 0);
        }

        void PaintCliff (ref float[,,] alphaMap, int u, int v, Biome.Type bType, float topProp, float cliffProp, float botProp)
        {
            if (
                u >= WorldManager.BLUR_RADIUS
                && v >= WorldManager.BLUR_RADIUS
                && u < WorldManager.HEIGHT_POINT_PER_UNIT * WorldManager.TERRAIN_WIDTH + WorldManager.BLUR_RADIUS
                && v < WorldManager.HEIGHT_POINT_PER_UNIT * WorldManager.TERRAIN_WIDTH + WorldManager.BLUR_RADIUS)
            {
                alphaMap[v - WorldManager.BLUR_RADIUS, u - WorldManager.BLUR_RADIUS,
                                    WorldManager.GetLayerIndex (GroundType.Top, bType)] = topProp;
                alphaMap[v - WorldManager.BLUR_RADIUS, u - WorldManager.BLUR_RADIUS,
                    WorldManager.GetLayerIndex (GroundType.Cliff, bType)] = cliffProp;
                alphaMap[v - WorldManager.BLUR_RADIUS, u - WorldManager.BLUR_RADIUS,
                    WorldManager.GetLayerIndex (GroundType.Ground1, bType)] = botProp;
            }
        }

        private void InitGround1Function ()
        {
            ground1Zone = new float[GROUND_1_FUNCTION_PARTS];
            for (int i = 0; i < GROUND_1_FUNCTION_PARTS; i++)
            {
                ground1Zone[i] = (float)rnd.NextDouble () * (ground1Max - ground1Min) + ground1Min;
            }
        }

        private float Ground1Dist (float angle)
        {
            angle = (angle + 2 * (float)Math.PI) % (2 * (float)Math.PI);
            float sectorLength = 2 * (float)Math.PI / GROUND_1_FUNCTION_PARTS;
            int i = (int)(angle / sectorLength);
            float t = (angle - i * sectorLength) / sectorLength;
            float prop = (1 - (float)Math.Cos (t * Math.PI)) * .5f;
            return ground1Zone[i] * (1 - prop) + ground1Zone[(i + 1) % GROUND_1_FUNCTION_PARTS] * prop;
        }

        // coord in island units
        public bool IsInGround1 (Vector2 pos)
        {
            if (vertices == null)
            {
                GenerateIslandVertices ();
            }

            return pos.magnitude < Ground1Dist (Vector2.SignedAngle (Vector2.left, pos) / 180 * (float)Math.PI);
        }

        public void GenerateGround1 (ref GroundType[,] groundMap, Vector2 terrainPosition)
        {
            if (vertices == null)
            {
                GenerateIslandVertices ();
            }

            int mapSize = WorldManager.TERRAIN_WIDTH * WorldManager.HEIGHT_POINT_PER_UNIT;
            int margin = (int)(ground1Max * SCALE * WorldManager.HEIGHT_POINT_PER_UNIT);
            GetBounds (mapSize, terrainPosition, margin,
                out Vector2 islandCenterInIntCoord, out float pointPerUnit, out int uMin, out int uMax, out int vMin, out int vMax);

            // For each point in the region
            for (int u = uMin; u < uMax; u++)
            {
                // convert to island unit (/SCALE)
                float x = (u - islandCenterInIntCoord.x - WorldManager.BLUR_RADIUS) / pointPerUnit / SCALE;

                for (int v = vMin; v < vMax; v++)
                {
                    // convert to island unit (/SCALE)
                    float y = (v - islandCenterInIntCoord.y - WorldManager.BLUR_RADIUS) / pointPerUnit / SCALE;

                    if (IsInGround1 (new Vector2 (x, y)))
                    {
                        groundMap[v, u] = GroundType.Ground1;
                    }
                }
            }
        }
    }
}

