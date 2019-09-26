using LightBringer.TerrainGeneration;
using System.Drawing;
using UnityEngine;
using SColor = System.Drawing.Color;

public class MapPainter
{
    private static Bitmap bmp;


    public void DrawIslands(ref SpatialDictionary<Island> islands, int xCenter, int yCenter, int mapRadius)
    {
        bmp = new Bitmap(mapRadius * 2, mapRadius * 2);
        for (int i = 0; i < mapRadius * 2; i++)
        {
            for (int j = 0; j < mapRadius * 2; j++)
            {
                bmp.SetPixel(i, j, SColor.Green);
            }
        }

        foreach (Island island in islands.GetAround(xCenter, yCenter, mapRadius + (int)(Island.MAX_POSSIBLE_RADIUS * Island.SCALE * 2)))
        {
            DrawIsland(island, ref bmp, xCenter, yCenter, mapRadius);
        }

        Debug.Log("Save to: " + Application.persistentDataPath + "/WorldMap.png");
        bmp.Save(Application.persistentDataPath + "/WorldMap.png");
    }

    void DrawIsland(Island island, ref Bitmap bmp, int xCenter, int yCenter, int mapRadius)
    {
        float sqRadius = Island.MAX_POSSIBLE_RADIUS * Island.SCALE * 2;
        int iMin = Mathf.Max(0, (int)(island.centerInWorld.x - xCenter + mapRadius - sqRadius));
        int iMax = Mathf.Min(2 * mapRadius - 1, (int)(island.centerInWorld.x - xCenter + mapRadius + sqRadius));
        int jMin = Mathf.Max(0, (int)(island.centerInWorld.y - yCenter + mapRadius - sqRadius));
        int jMax = Mathf.Min(2 * mapRadius - 1, (int)(island.centerInWorld.y - yCenter + mapRadius + sqRadius));

        for (int i = iMin; i <= iMax; i++)
        {
            for (int j = jMin; j <= jMax; j++)
            {
                float x = (i - mapRadius + xCenter - island.centerInWorld.x) / Island.SCALE;
                float y = (j - mapRadius + yCenter - island.centerInWorld.y) / Island.SCALE;

                float dist = island.DistanceFromIsland(new Vector2(x, y));
                if (dist == 0)
                {
                    bmp.SetPixel(i, j, SColor.Black);
                }
                else if (dist < .2f)
                {
                    bmp.SetPixel(i, j, SColor.Gray);
                }
            }
        }
    }

    public void DrawBiomes(ref SpatialDictionary<Biome> biomes, int xCenter, int yCenter, int mapRadius)
    {
        bmp = new Bitmap(mapRadius * 2, mapRadius * 2);
        SColor[] biomeColors = { SColor.Yellow, SColor.Purple, SColor.Red, SColor.LightBlue, SColor.Green, SColor.LightGray };

        for (int i = 0; i < mapRadius * 2; i++)
        {
            for (int j = 0; j < mapRadius * 2; j++)
            {
                Vector2 point = new Vector2(i + xCenter - mapRadius, -(j + yCenter - mapRadius));
                Biome biome = Biome.GetBiome(biomes, point);
                bmp.SetPixel(i, j, biomeColors[(int)biome.type]);
            }
        }

        foreach (Biome biome in biomes.GetAround(xCenter, yCenter, mapRadius))
        {
            DrawBiome(biome, ref bmp, xCenter, yCenter, mapRadius);
        }

        string path = Application.persistentDataPath + "/BiomeMap.png";
        Debug.Log("Save to: " + path);
        bmp.Save(path);
    }

    void DrawBiome(Biome biome, ref Bitmap bmp, int xCenter, int yCenter, int mapRadius)
    {
        Pen blackPen = new Pen(System.Drawing.Color.Black, 4);

        for (int i = 0; i < 4; i++)
        {
            int x1 = (int)biome.vertices[i].x + mapRadius - xCenter;
            int y1 = -(int)biome.vertices[i].y + mapRadius - yCenter;
            int x2 = (int)biome.vertices[(i + 1) % 4].x + mapRadius - xCenter;
            int y2 = -(int)biome.vertices[(i + 1) % 4].y + mapRadius - yCenter;

            // Draw line to bitmap.
            using (var graphics = System.Drawing.Graphics.FromImage(bmp))
            {
                graphics.DrawLine(blackPen, x1, y1, x2, y2);
            }
        }
    }
}
