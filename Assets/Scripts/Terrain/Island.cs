using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    public class Island
    {
        private const float REQUIRED_FREE_SPACE_BRANCH = 14f;
        private const float REQUIRED_FREE_SPACE_ACCESS = 6f;
        private const float SLOPE_LENGTH = 6f;
        private int startSize = 192;

        private int heightPointPerUnity = 2;
        private float[,] heights;
        private int xCenterInTerrain;
        private int yCenterInTerrain;

        private int xLocalCenter;
        private int yLocalCenter;

        public List<Slope> slopes;

        public Island(int xCenterInWorld, int yCenterInWorld)
        {
            xCenterInTerrain = xCenterInWorld * heightPointPerUnity;
            yCenterInTerrain = yCenterInWorld * heightPointPerUnity;
        }

        public void GenerateIsland(ref float[,] terrainHeights)
        {
            Initialisation(ref terrainHeights);

            AddCircle(xLocalCenter, yLocalCenter, 10, REQUIRED_FREE_SPACE_BRANCH);
            AddBranch(xLocalCenter, yLocalCenter, 5, 12);
            AddBranch(xLocalCenter, yLocalCenter, 5, 12);
            AddBranch(xLocalCenter, yLocalCenter, 5, 12);

            GenerateAccesses();

            // Delete useless parts
            SimplifyHeights();
        }

        private void Initialisation(ref float[,] terrainHeights)
        {
            heights = new float[startSize, startSize];
            xLocalCenter = startSize / 2;
            yLocalCenter = startSize / 2;

            int xCornerInTerrain = xCenterInTerrain - xLocalCenter;
            int yCornerInTerrain = yCenterInTerrain - yLocalCenter;


            for (int x = 0; x < startSize; x++)
            {
                for (int y = 0; y < startSize; y++)
                {
                    if (terrainHeights[xCornerInTerrain + x, yCornerInTerrain + y] > 0)
                    {
                        // Already used by other island
                        heights[x, y] = -1;
                    }
                    else
                    {
                        // free spot
                        heights[x, y] = 0;
                    }
                }
            }
        }

        // in height unity
        private void AddBranch(int x, int y, float minRadius, float maxRadius)
        {
            Vector2 direction = TerrainGenerator.Vector2FromAngle(Random.value * 360f);
            Vector2 position = new Vector2(x, y);
            float radius = (minRadius + maxRadius) / 2f;

            bool free = true;

            for (int i = 0; i < 30; i++)
            {
                direction = TerrainGenerator.RotateVector(direction, Random.value * 80f - 40f);
                ModifyRadius(ref radius, minRadius, maxRadius);
                position += direction * heightPointPerUnity;
                free = AddCircle((int)position.x, (int)position.y, radius, REQUIRED_FREE_SPACE_BRANCH);
                if (!free)
                {
                    break;
                }
            }
        }

        // in height unity
        private void ModifyRadius(ref float radius, float min, float max)
        {
            float range = (max - min) / 2 * .25f;
            radius += Random.value * 2 * range - range;
            if (radius < min)
            {
                radius = min;
            }
            if (radius > max)
            {
                radius = max;
            }
        }

        // in height unity
        private bool AddCircle(int x, int y, float radius, float requiredFreeSpace)
        {
            if (!diskIsFree(x, y, radius + requiredFreeSpace * heightPointPerUnity))
            {
                return false;
            }

            for (int i = x - (int)radius - 1; i < x + (int)radius + 1; i++)
            {
                for (int j = y - (int)radius - 1; j < y + (int)radius + 1; j++)
                {
                    if ((i - x) * (i - x) + (j - y) * (j - y) < radius * radius)
                    {
                        heights[i, j] = .5f;
                    }
                }
            }

            return true;
        }

        // in height unity
        private bool diskIsFree(int x, int y, float radius)
        {
            for (int i = x - (int)radius - 1; i < x + (int)radius + 1; i++)
            {
                for (int j = y - (int)radius - 1; j < y + (int)radius + 1; j++)
                {
                    if ((i - x) * (i - x) + (j - y) * (j - y) < radius * radius)
                    {
                        if (heights[i, j] < 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

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

        private void SimplifyHeights()
        {
            int xFirst = FindFirstX();
            int xLast = FindLastX();
            int yFirst = FindFirstY();
            int yLast = FindLastY();

            float[,] newHeights = new float[xLast + 1 - xFirst, yLast + 1 - yFirst];

            for (int x = 0; x <= xLast - xFirst; x++)
            {
                for (int y = 0; y <= yLast - yFirst; y++)
                {
                    newHeights[x, y] = heights[xFirst + x, yFirst + y];
                }
            }

            heights = newHeights;
            xLocalCenter -= xFirst;
            yLocalCenter -= yFirst;

            foreach(Slope slope in slopes)
            {
                slope.topPoint -= new Vector2Int(xFirst, yFirst);
                slope.botPoint -= new Vector2Int(xFirst, yFirst);
            }
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

        public void AddIslandToMap(ref float[,] terrainHeights)
        {
            int xCornerInTerrain = xCenterInTerrain - xLocalCenter;
            int yCornerInTerrain = yCenterInTerrain - yLocalCenter;

            for (int x = 0; x < heights.GetLength(0); x++)
            {
                for (int y = 0; y < heights.GetLength(1); y++)
                {
                    // Add the positive heights (terrain heights should be 0 at this point, because of island building method)
                    if (heights[x, y] > 0)
                    {
                        terrainHeights[xCornerInTerrain + x, yCornerInTerrain + y] += heights[x, y];
                    }
                }
            }
        }

        public Vector2Int GetStartingCorner()
        {
            return new Vector2Int(xCenterInTerrain - xLocalCenter, yCenterInTerrain - yLocalCenter);
        }
    }

}

