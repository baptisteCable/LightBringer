using LightBringer.TerrainGeneration;
using System.Drawing;
using UnityEngine;
using SColor = System.Drawing.Color;

public class MapPainter
{
    private static Bitmap bmp;

    private SColor[] biomeColors = { SColor.Pink, SColor.Yellow, SColor.Purple, SColor.Red, SColor.Blue, SColor.Green, SColor.LightGray };

    public void DrawIslands (ref SpatialDictionary<Biome> biomes,
        ref SpatialDictionary<Island> islands, int xCenter, int yCenter, int mapRadius, int meterPerPix, int nbOfSquares = 0)
    {
        bmp = new Bitmap (mapRadius * 2 / meterPerPix, mapRadius * 2 / meterPerPix);
        BiomeBmp (ref bmp, ref biomes, xCenter, yCenter, mapRadius, meterPerPix);

        foreach (Island island in islands.GetAround (xCenter, yCenter, mapRadius + (int)(Island.MAX_RADIUS * Island.SCALE * 2)))
        {
            DrawIsland (island, ref bmp, xCenter, yCenter, mapRadius, meterPerPix);
        }

        if (nbOfSquares > 1)
        {
            DrawSquares (ref bmp, nbOfSquares);
        }

        Debug.Log ("Save to: " + Application.persistentDataPath + "/WorldMap.png");
        bmp.Save (Application.persistentDataPath + "/WorldMap.png");
    }

    private void DrawSquares (ref Bitmap bmp, int number)
    {
        Pen whitePen = new Pen (SColor.White, 1);

        for (int i = 1; i < number; i++)
        {
            int linePosition = i * bmp.Width / number;

            using (var graphics = System.Drawing.Graphics.FromImage (bmp))
            {
                graphics.DrawLine (whitePen, 0, linePosition, bmp.Width, linePosition);
                graphics.DrawLine (whitePen, linePosition, 0, linePosition, bmp.Width);
            }
        }
    }

    void DrawIsland (Island island, ref Bitmap bmp, int xCenter, int yCenter, int mapRadius, int meterPerPix)
    {
        float sqRadius = Island.MAX_RADIUS * Island.SCALE * 2;
        int iMin = Mathf.Max (0, (int)(island.centerInWorld.x - xCenter + mapRadius - sqRadius)) / meterPerPix;
        int iMax = Mathf.Min (2 * mapRadius - 1, (int)(island.centerInWorld.x - xCenter + mapRadius + sqRadius)) / meterPerPix;
        int jMin = Mathf.Max (0, (int)(-island.centerInWorld.y - yCenter + mapRadius - sqRadius)) / meterPerPix;
        int jMax = Mathf.Min (2 * mapRadius - 1, (int)(-island.centerInWorld.y - yCenter + mapRadius + sqRadius)) / meterPerPix;

        for (int i = iMin; i <= iMax; i++)
        {
            for (int j = jMin; j <= jMax; j++)
            {
                float x = (i * meterPerPix - mapRadius + xCenter - island.centerInWorld.x) / Island.SCALE;
                float y = (j * meterPerPix - mapRadius + yCenter + island.centerInWorld.y) / Island.SCALE;

                float dist = island.DistanceFromIslandInIslandUnit (new Vector2 (x, y));
                if (dist == 0)
                {
                    // Color depends on island biome type
                    bmp.SetPixel (i, j, biomeColors[(int)island.biomeType]);
                }
                else if (dist < .2f * meterPerPix)
                {
                    bmp.SetPixel (i, j, SColor.Black);
                }
            }
        }
    }

    private void BiomeBmp (ref Bitmap bmp, ref SpatialDictionary<Biome> biomes, int xCenter, int yCenter, int mapRadius, int meterPerPix)
    {
        for (int i = 0; i < mapRadius * 2 / meterPerPix; i++)
        {
            for (int j = 0; j < mapRadius * 2 / meterPerPix; j++)
            {
                Vector2 point = new Vector2 (i * meterPerPix + xCenter - mapRadius, -(j * meterPerPix + yCenter - mapRadius));
                Biome biome = Biome.GetBiome (biomes, point);
                bmp.SetPixel (i, j, biomeColors[(int)biome.type]);
            }
        }
    }
}
