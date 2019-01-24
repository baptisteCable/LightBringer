using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{

    private float depth = 8f;
    private int height = 256;
    private int width = 256;

    void Start()
    {
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateData(terrain.terrainData);
    }

    private TerrainData GenerateData(TerrainData data)
    {
        data.heightmapResolution = width + 1;
        data.size = new Vector3(width, depth, height);

        float[,] heights = GenerateFlat();
        AddIsland(ref heights, 90, 64, 10);
        AddIsland(ref heights, 90, 192, 10);
        AddIsland(ref heights, 166, 64, 10);
        AddIsland(ref heights, 166, 192, 10);
        AddIsland(ref heights, 64, 128, 10);
        AddIsland(ref heights, 192, 128, 10);
        heights = Smooth(heights, 1);

        data.SetHeights(0, 0, heights);
        return data;
    }

    private float[,] GenerateFlat()
    {
        float[,] heights = new float[height, width];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                heights[i, j] = 0f;
            }
        }

        return Smooth(heights, 1);
    }

    private void AddIsland(ref float[,] heights, int x, int y, float size)
    {
        AddCircle(ref heights, x, y, 10);
        AddBranch(ref heights, x, y);
        AddBranch(ref heights, x, y);
        AddBranch(ref heights, x, y);
    }

    private void AddBranch(ref float[,] heights, int x, int y)
    {
        Vector2 direction = Vector2FromAngle(Random.value * 360f);
        Vector2 position = new Vector2(x, y);
        float radius = 6f;

        for (int i = 0; i < 30; i++)
        {

            direction = RotateVector(direction, Random.value * 80f - 40f);
            ModifyRadius(ref radius, 3, 7);
            position += direction;
            Debug.Log(radius);
            AddCircle(ref heights, (int)position.x, (int)position.y, radius);
        }
    }

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

    private void AddCircle(ref float[,] heights, int x, int y, float radius)
    {
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
    }

    private float[,] Smooth(float[,] heights, int radius)
    {
        float[,] smoothed = new float[height, width];
        int nbPoint = (2 * radius + 1) * (2 * radius + 1);

        for (int i = radius; i < width - radius; i++)
        {
            for (int j = radius; j < height - radius; j++)
            {
                smoothed[i, j] = 0f;

                for (int k = -radius; k <= radius; k++)
                {
                    for (int l = -radius; l <= radius; l++)
                    {
                        smoothed[i, j] += heights[i + k, j + l];
                    }
                }
                smoothed[i, j] /= nbPoint;
            }
        }

        return smoothed;
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
