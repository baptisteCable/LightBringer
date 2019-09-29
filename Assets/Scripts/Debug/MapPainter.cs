using LightBringer.TerrainGeneration;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using SColor = System.Drawing.Color;

public class MapPainter
{
    private static Bitmap bmp;

    private SColor[] biomeColors = { SColor.Pink, SColor.Yellow, SColor.Purple, SColor.Red, SColor.Blue, SColor.Green, SColor.LightGray };

    public void DrawIslands(ref SpatialDictionary<Biome> biomes, 
        ref SpatialDictionary<Island> islands, int xCenter, int yCenter, int mapRadius, int meterPerPix)
    {
        bmp = new Bitmap(mapRadius * 2 / meterPerPix, mapRadius * 2 / meterPerPix);
        BiomeBmp(ref bmp, ref biomes, xCenter, yCenter, mapRadius, meterPerPix);

        foreach (Island island in islands.GetAround(xCenter, yCenter, mapRadius + (int)(Island.MAX_POSSIBLE_RADIUS * Island.SCALE * 2)))
        {
            DrawIsland(island, ref bmp, xCenter, yCenter, mapRadius, meterPerPix);
        }

        Debug.Log("Save to: " + Application.persistentDataPath + "/WorldMap.png");
        bmp.Save(Application.persistentDataPath + "/WorldMap.png");
    }

    void DrawIsland(Island island, ref Bitmap bmp, int xCenter, int yCenter, int mapRadius, int meterPerPix)
    {
        float sqRadius = Island.MAX_POSSIBLE_RADIUS * Island.SCALE * 2;
        int iMin = Mathf.Max(0, (int)(island.centerInWorld.x - xCenter + mapRadius - sqRadius)) / meterPerPix;
        int iMax = Mathf.Min(2 * mapRadius - 1, (int)(island.centerInWorld.x - xCenter + mapRadius + sqRadius)) / meterPerPix;
        int jMin = Mathf.Max(0, (int)(-island.centerInWorld.y - yCenter + mapRadius - sqRadius)) / meterPerPix;
        int jMax = Mathf.Min(2 * mapRadius - 1, (int)(-island.centerInWorld.y - yCenter + mapRadius + sqRadius)) / meterPerPix;

        for (int i = iMin; i <= iMax; i++)
        {
            for (int j = jMin; j <= jMax; j++)
            {
                float x = (i * meterPerPix - mapRadius + xCenter - island.centerInWorld.x) / Island.SCALE;
                float y = (j * meterPerPix - mapRadius + yCenter + island.centerInWorld.y) / Island.SCALE;

                float dist = island.DistanceFromIsland(new Vector2(x, y));
                if (dist == 0)
                {
                    // Color depends on island biome type
                    bmp.SetPixel(i, j, biomeColors[(int)island.biomeType]);
                }
                else if (dist < .2f * meterPerPix)
                {
                    bmp.SetPixel(i, j, SColor.Black);
                }
            }
        }
    }

    public void DrawBiomes(ref SpatialDictionary<Biome> biomes, int xCenter, int yCenter, int mapRadius, int meterPerPix)
    {
        bmp = new Bitmap(mapRadius * 2 / meterPerPix, mapRadius * 2 / meterPerPix);
        BiomeBmp(ref bmp, ref biomes, xCenter, yCenter, mapRadius, meterPerPix);
        string path = Application.persistentDataPath + "/BiomeMap.png";
        Debug.Log("Save to: " + path);
        bmp.Save(path);
    }

    private void BiomeBmp(ref Bitmap bmp, ref SpatialDictionary<Biome> biomes, int xCenter, int yCenter, int mapRadius, int meterPerPix)
    {
        for (int i = 0; i < mapRadius * 2 / meterPerPix; i++)
        {
            for (int j = 0; j < mapRadius * 2 / meterPerPix; j++)
            {
                Vector2 point = new Vector2(i * meterPerPix + xCenter - mapRadius, -(j * meterPerPix + yCenter - mapRadius));
                Biome biome = Biome.GetBiome(biomes, point);
                bmp.SetPixel(i, j, biomeColors[(int)biome.type]);
            }
        }
    }

    public void DrawBiomesPoly(ref SpatialDictionary<Biome> biomes, int xCenter, int yCenter, int mapRadius)
    {
        // load the existing picture
        string imageToLoad = Application.persistentDataPath + "/BiomeMap.png";
        Bitmap bmp = new Bitmap(imageToLoad);

        // scale
        int meterPerPix = mapRadius / bmp.Width * 2;

        Pen blackPen = new Pen(SColor.Black, System.Math.Max(1, 4 / meterPerPix));

        foreach (Biome biome in biomes.GetAround(xCenter, yCenter, mapRadius))
        {
            for (int i = 0; i < 4; i++)
            {
                int x1 = ((int)biome.vertices[i].x + mapRadius - xCenter) / meterPerPix;
                int y1 = (-(int)biome.vertices[i].y + mapRadius - yCenter) / 4;
                int x2 = ((int)biome.vertices[(i + 1) % 4].x + mapRadius - xCenter) / 4;
                int y2 = (-(int)biome.vertices[(i + 1) % 4].y + mapRadius - yCenter) / 4;

                // Draw line to bitmap.
                using (var graphics = System.Drawing.Graphics.FromImage(bmp))
                {
                    graphics.DrawLine(blackPen, x1, y1, x2, y2);
                }
            }
        }

        string path = Application.persistentDataPath + "/BiomeMapPoly.png";
        Debug.Log("Save to: " + path);
        bmp.Save(path);
    }

    public void DrawNeighbourhoodLines(ref Dictionary<Dic2DKey, List<Dic2DKey>> neighbours,
        int xCenter, int yCenter, int mapRadius)
    {
        // load the existing picture
        string imageToLoad = Application.persistentDataPath + "/BiomeMap.png";
        Bitmap bmp = new Bitmap(imageToLoad);

        // scale
        int meterPerPix = mapRadius / bmp.Width * 2;

        // draw all the neighbourhood lines
        Pen pen = new Pen(SColor.LightCyan, System.Math.Max(1, 4 / meterPerPix));

        foreach (KeyValuePair<Dic2DKey, List<Dic2DKey>> biomePair in neighbours)
        {
            foreach (Dic2DKey neighbour in biomePair.Value)
            {
                int x1 = (biomePair.Key.x + mapRadius - xCenter) / meterPerPix;
                int y1 = (-biomePair.Key.y + mapRadius - yCenter) / meterPerPix;
                int x2 = (neighbour.x + mapRadius - xCenter) / meterPerPix;
                int y2 = (-neighbour.y + mapRadius - yCenter) / meterPerPix;

                // Draw line to bitmap.
                using (var graphics = System.Drawing.Graphics.FromImage(bmp))
                {
                    graphics.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        // save in a new picture
        string path = Application.persistentDataPath + "/BiomeMapNeighbours.png";
        Debug.Log("Save to: " + path);
        bmp.Save(path);
    }

    public void DrawBiomeOrder(List<List<Biome>> orderList, int xCenter, int yCenter, int mapRadius)
    {
        // load the existing picture
        string imageToLoad = Application.persistentDataPath + "/BiomeMap.png";
        Bitmap bmp = new Bitmap(imageToLoad);

        // scale
        int meterPerPix = mapRadius / bmp.Width * 2;

        for (int i = 0; i < orderList.Count; i++)
        {
            foreach (Biome biome in orderList[i])
            {
                int x = (biome.coord.x + mapRadius - xCenter - 40) / meterPerPix;
                int y = (-biome.coord.y + mapRadius - yCenter - 40) / meterPerPix;
                RectangleF rectf = new RectangleF(x, y, 120 / meterPerPix, 160 / meterPerPix);

                // Draw line to bitmap.
                using (var graphics = System.Drawing.Graphics.FromImage(bmp))
                {
                    graphics.DrawString(i.ToString(), new System.Drawing.Font("Tahoma", 40 / meterPerPix), Brushes.Black, rectf);
                }
            }
        }

        // save in a new picture
        string path = Application.persistentDataPath + "/BiomeMapOrder.png";
        Debug.Log("Save to: " + path);
        bmp.Save(path);
    }

    public void DrawBiomeTypingOrder(List<Biome> typingBiomeList, int xCenter, int yCenter, int mapRadius)
    {
        // load the existing picture
        string imageToLoad = Application.persistentDataPath + "/BiomeMap.png";
        Bitmap bmp = new Bitmap(imageToLoad);

        // scale
        int meterPerPix = mapRadius / bmp.Width * 2;

        for (int i = 0; i < typingBiomeList.Count; i++)
        {
            int x = (typingBiomeList[i].coord.x + mapRadius - xCenter - 40) / meterPerPix;
            int y = (-typingBiomeList[i].coord.y + mapRadius - yCenter - 40) / meterPerPix;
            RectangleF rectf = new RectangleF(x, y, 120 / meterPerPix, 160 / meterPerPix);

            // Draw line to bitmap.
            using (var graphics = System.Drawing.Graphics.FromImage(bmp))
            {
                graphics.DrawString(i.ToString(), new System.Drawing.Font("Tahoma", 40 / meterPerPix), Brushes.Black, rectf);
            }
        }

        // save in a new picture
        string path = Application.persistentDataPath + "/BiomeMapTypingOrder.png";
        Debug.Log("Save to: " + path);
        bmp.Save(path);
    }
}
