using UnityEngine;

public class Island
{
    private const float REQUIRED_FREE_SPACE = 10f;
    private const float SLOPE_LENGTH = 6f;
    private int startSize = 192;

    public int heightPointPerUnity = 2;
    public float[,] heights;
    public int xCenterInTerrain;
    public int yCenterInTerrain;

    public int xLocalCenter;
    public int yLocalCenter;

    public Island(int xCenterInWorld, int yCenterInWorld)
    {
        xCenterInTerrain = xCenterInWorld * heightPointPerUnity;
        yCenterInTerrain = yCenterInWorld * heightPointPerUnity;
    }

    public void GenerateIsland(ref float[,] terrainHeights)
    {
        Initialisation(ref terrainHeights);

        AddCircle(xLocalCenter, yLocalCenter, 10);
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
        Vector2 direction = Vector2FromAngle(Random.value * 360f);
        Vector2 position = new Vector2(x, y);
        float radius = (minRadius + maxRadius) / 2f;

        bool free = true;

        for (int i = 0; i < 30; i++)
        {
            direction = RotateVector(direction, Random.value * 80f - 40f);
            ModifyRadius(ref radius, minRadius, maxRadius);
            position += direction * heightPointPerUnity;
            free = AddCircle((int)position.x, (int)position.y, radius);
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
    private bool AddCircle(int x, int y, float radius)
    {
        if (!diskIsFree(x, y, radius + REQUIRED_FREE_SPACE * heightPointPerUnity))
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
                        Debug.Log("Spot not free");
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private void GenerateAccesses()
    {
        int[] xBorder = new int[6];
        int[] yBorder = new int[6];
        Vector2 direction = new Vector2(0, 1);

        // find 6 border points and find the farthest from the center
        float maxDist = 0;
        int maxI = 0;
        for (int i = 0; i < 6; i++)
        {
            FindBorderPoint(xLocalCenter, yLocalCenter, RotateVector(direction, i * 60), out xBorder[i], out yBorder[i]);
            float dist = (xLocalCenter - xBorder[i]) * (xLocalCenter - xBorder[i]) + (yLocalCenter - yBorder[i]) * (yLocalCenter - yBorder[i]);
            if (dist > maxDist)
            {
                maxDist = dist;
                maxI = i;
            }
        }
        CreateAccess(xBorder[maxI], yBorder[maxI], RotateVector(direction, maxI * 60), 4f);
        

        // find farthest between the 3 opposite border points
        maxDist = 0;
        int newIStart = maxI + 2;
        for (int i =newIStart; i < newIStart + 3; i++)
        {
            float dist = (xLocalCenter - xBorder[i % 6]) * (xLocalCenter - xBorder[i % 6]) + (yLocalCenter - yBorder[i % 6]) * (yLocalCenter - yBorder[i % 6]);
            if (dist > maxDist)
            {
                maxDist = dist;
                maxI = i % 6;
            }
        }
        CreateAccess(xBorder[maxI], yBorder[maxI], RotateVector(direction, maxI * 60), 4f);
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
            AddCircle((int)x, (int)y, radius * 2);
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

        direction = RotateVector(direction, angle);
        while (Mathf.Abs(angle) <= 270)
        {
            direction = RotateVector(direction, angleDiff);
            angle += angleDiff;
            Vector2 normal = RotateVector(direction, 90);
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
        Vector2 normal = RotateVector(direction, 90);
        Vector2 topL = -normal * radius + new Vector2(x, y);
        Vector2 topR = normal * radius + new Vector2(x, y);
        Vector2 botL = -normal * radius + new Vector2(x, y) + direction * length;
        Vector2 botR = normal * radius + new Vector2(x, y) + direction * length;

        int minX = (int)Mathf.Min(topL.x, topR.x, botL.x, botR.x);
        int minY = (int)Mathf.Min(topL.y, topR.y, botL.y, botR.y);
        int maxX = (int)Mathf.Max(topL.x, topR.x, botL.x, botR.x) + 1;
        int maxY = (int)Mathf.Max(topL.y, topR.y, botL.y, botR.y) + 1;

        // for each point in the area, if in slope, adjust height.
        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
            {
                Vector2 pointFromOrigin = new Vector2(i - x, j - y);
                float downDist = Vector2.Dot(pointFromOrigin, direction);
                float sideDist = Vector2.Dot(pointFromOrigin, normal);

                // if in slope
                if (downDist > 0 && downDist < length && Mathf.Abs(sideDist) <= radius)
                {
                    heights[i, j] = (length - downDist) / length * .5f;
                }
            }
        }
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

    private Vector2 Vector2FromAngle(float a)
    {
        a *= Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
    }

    public Vector2 RotateVector(Vector2 v, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float x = v.x * Mathf.Cos(radian) - v.y * Mathf.Sin(radian);
        float y = v.x * Mathf.Sin(radian) + v.y * Mathf.Cos(radian);
        return new Vector2(x, y);
    }
}
