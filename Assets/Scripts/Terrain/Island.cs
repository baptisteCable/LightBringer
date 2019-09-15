using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    public class Island
    {
        private const float SLOPE_LENGTH = 6f;
        private const float RADIUS = 2.7f;
        private const float SCALE = 10f;


        Vector2 centerInWorld;

        public List<Slope> slopes;

        /* -------- */
        private List<Vector2> vertices;

        public Island(Vector2 centerPosition)
        {
            centerInWorld = centerPosition;
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

        void LastTwoVertices()
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

        private bool IsInside(Vector2 point)
        {
            // closest point
            int closest = 0;
            double minDist = double.PositiveInfinity;
            double[] distances = new double[vertices.Count];

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

            return Vector2.Dot(normal, vect) >= 0;

        }

        public void GenerateIslandAndHeights(ref float[,] terrainHeights, Vector2 terrainPosition, int terrainWidth, int heightPointPerUnity, int seed)
        {
            // Generate island data from seed
            GenerateIslandVertices(seed, RADIUS);

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
            int uMin = Mathf.Max(0, (int)(xMin * heightPointPerUnity * SCALE + islandCenterInHeightCoord.x));
            int uMax = Mathf.Min(terrainWidth * heightPointPerUnity - 1, (int)(xMax * heightPointPerUnity * SCALE + islandCenterInHeightCoord.x));
            int vMin = Mathf.Max(0, (int)(yMin * heightPointPerUnity * SCALE + islandCenterInHeightCoord.y));
            int vMax = Mathf.Min(terrainWidth * heightPointPerUnity - 1, (int)(yMax * heightPointPerUnity * SCALE + islandCenterInHeightCoord.y));

            // For each point in the region, compute height
            for (int u = uMin; u <= uMax; u++)
            {
                float x = (u - islandCenterInHeightCoord.x) / heightPointPerUnity / SCALE;

                for (int v = vMin; v <= vMax; v++)
                {
                    float y = (v - islandCenterInHeightCoord.y) / heightPointPerUnity / SCALE;

                    if (IsInside(new Vector2(x, y)))
                    {
                        terrainHeights[u, v] = .5f;
                    }
                }
            }



        }

        /*
        

        

        private void GenerateAccesses()
        {
            // clear the slope list
            slopes = new List<Slope>();

            int[] xBorder = new int[6];
            int[] yBorder = new int[6];
            Vector2 direction = new Vector2(0, 1);

            // find 6 border points and find the farthest from the center
            float maxDist = 0;
            int maxI = 0;
            for (int i = 0; i < 6; i++)
            {
                FindBorderPoint(xLocalCenter, yLocalCenter, TerrainGenerator.RotateVector(direction, i * 60), out xBorder[i], out yBorder[i]);
                float dist = (xLocalCenter - xBorder[i]) * (xLocalCenter - xBorder[i]) + (yLocalCenter - yBorder[i]) * (yLocalCenter - yBorder[i]);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    maxI = i;
                }
            }
            CreateAccess(xBorder[maxI], yBorder[maxI], TerrainGenerator.RotateVector(direction, maxI * 60), 4f);


            // find farthest between the 3 opposite border points
            maxDist = 0;
            int newIStart = maxI + 2;
            for (int i = newIStart; i < newIStart + 3; i++)
            {
                float dist = (xLocalCenter - xBorder[i % 6]) * (xLocalCenter - xBorder[i % 6]) + (yLocalCenter - yBorder[i % 6]) * (yLocalCenter - yBorder[i % 6]);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    maxI = i % 6;
                }
            }
            CreateAccess(xBorder[maxI], yBorder[maxI], TerrainGenerator.RotateVector(direction, maxI * 60), 4f);
        }

        // in height unity
        private void FindBorderPoint(int xOrigin, int yOrigin, Vector2 direction, out int xBorder, out int yBorder)
        {
            xBorder = xOrigin;
            yBorder = yOrigin;

            float xNext = xOrigin;
            float yNext = yOrigin;
            int xNextInt = xOrigin;
            int yNextInt = yOrigin;

            while (xNextInt >= 0 && xNextInt < heights.GetLength(0) && yNextInt >= 0 && yNextInt < heights.GetLength(1))
            {
                if (heights[xNextInt, yNextInt] > 0)
                {
                    xBorder = xNextInt;
                    yBorder = yNextInt;
                }

                xNext += direction.x;
                yNext += direction.y;
                xNextInt = (int)Mathf.Round(xNext);
                yNextInt = (int)Mathf.Round(yNext);
            }
        }

        // in height unity
        private void CreateAccess(int xBorder, int yBorder, Vector2 comingDirection, float radius)
        {
            int startingX = (int)(xBorder + radius * comingDirection.x);
            int startingY = (int)(yBorder + radius * comingDirection.y);

            Vector2 slopeDirection = FindSlopeDirection(startingX, startingY, comingDirection, radius, false);

            if (slopeDirection == Vector2.zero)
            {
                Debug.LogError("No possible slope");
                return;
            }

            float x = xBorder;
            float y = yBorder;

            for (int i = 0; i < (int)radius; i++)
            {
                x -= slopeDirection.x;
                y -= slopeDirection.y;
                AddCircle((int)x, (int)y, radius * 2, REQUIRED_FREE_SPACE_ACCESS);
            }

            CreateSlope(startingX, startingY, slopeDirection, radius);
        }

        private Vector2 FindSlopeDirection(int x, int y, Vector2 direction, float radius, bool trigo)
        {
            float angleDiff = 10f;
            float angle = -270f;
            if (!trigo)
            {
                angleDiff *= -1;
                angle *= -1;
            }

            float length = SLOPE_LENGTH * heightPointPerUnity;
            direction.Normalize();

            direction = TerrainGenerator.RotateVector(direction, angle);
            while (Mathf.Abs(angle) <= 270)
            {
                direction = TerrainGenerator.RotateVector(direction, angleDiff);
                angle += angleDiff;
                Vector2 normal = TerrainGenerator.RotateVector(direction, 90);
                Vector2 nearL = -normal * radius + new Vector2(x, y) + direction * length;
                Vector2 nearR = normal * radius + new Vector2(x, y) + direction * length;
                Vector2 farL = nearL + radius * 2 * direction;
                Vector2 farR = nearR + radius * 2 * direction;

                if (FreePlace(nearL) && FreePlace(nearR) && FreePlace(farL) && FreePlace(farR))
                {
                    return direction;
                }
            }

            return Vector2.zero;
        }

        private bool FreePlace(Vector2 pointInHeights)
        {
            int x = (int)Mathf.Round(pointInHeights.x);
            int y = (int)Mathf.Round(pointInHeights.y);

            return heights[x, y] > -.01 && heights[x, y] < .01;
        }

        private void CreateSlope(int x, int y, Vector2 direction, float radius)
        {
            float length = SLOPE_LENGTH * heightPointPerUnity;
            direction.Normalize();

            Vector2Int botPoint = new Vector2Int((int)Mathf.Round(x + direction.x * length), (int)Mathf.Round(y + direction.y * length));
            Slope slope = new Slope(new Vector2Int(x, y), botPoint, radius);

            Dictionary<Vector2Int, Vector2> points = slope.GetPointList();

            foreach (KeyValuePair<Vector2Int, Vector2> pair in points)
            {
                heights[pair.Key.x, pair.Key.y] = pair.Value.x * .5f;
            }

            slopes.Add(slope);
        }

        private int FindFirstX()
        {
            for (int x = 0; x < heights.GetLength(0); x++)
            {
                for (int y = 0; y < heights.GetLength(1); y++)
                {
                    if (heights[x, y] > 0f)
                    {
                        return x;
                    }
                }
            }

            // Error, no positive height found
            return -1;
        }

        private int FindFirstY()
        {
            for (int y = 0; y < heights.GetLength(1); y++)
            {
                for (int x = 0; x < heights.GetLength(0); x++)
                {
                    if (heights[x, y] > 0f)
                    {
                        return y;
                    }
                }
            }

            // Error, no positive height found
            return -1;
        }

        private int FindLastX()
        {
            for (int x = heights.GetLength(0) - 1; x >= 0; x--)
            {
                for (int y = 0; y < heights.GetLength(1); y++)
                {
                    if (heights[x, y] > 0f)
                    {
                        return x;
                    }
                }
            }

            // Error, no positive height found
            return -1;
        }

        private int FindLastY()
        {
            for (int y = heights.GetLength(1) - 1; y >= 0; y--)
            {
                for (int x = 0; x < heights.GetLength(0); x++)
                {
                    if (heights[x, y] > 0f)
                    {
                        return y;
                    }
                }
            }

            // Error, no positive height found
            return -1;
        }

        public Vector2Int GetStartingCorner()
        {
            return new Vector2Int(xCenterInTerrain - xLocalCenter, yCenterInTerrain - yLocalCenter);
        }
        */
    }

}

