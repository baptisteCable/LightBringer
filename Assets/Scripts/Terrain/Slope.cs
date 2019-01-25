using System.Collections.Generic;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    public class Slope
    {
        public float radius;
        public Vector2 topPoint;
        public Vector2 botPoint;

        public Slope(Vector2Int top, Vector2Int bot, float radius)
        {
            topPoint = top;
            botPoint = bot;
            this.radius = radius;
        }

        // returns every point in the slope. The second vector2 contains downPosition (1 is top, 0 is bot)
        // and sidePosition (-1 is max right when going down, 0 center and 1 left)
        public Dictionary<Vector2Int, Vector2> GetPointList(float radiusExtension = 0, float topExtension = 0, float botExtension = 0)
        {
            float length = (botPoint - topPoint).magnitude;
            Vector2 direction = (botPoint - topPoint).normalized;
            Vector2 normal = TerrainGenerator.RotateVector(direction, 90);
            Vector2 topL = -normal * (radius + radiusExtension) + topPoint - direction * topExtension;
            Vector2 topR = normal * (radius + radiusExtension) + topPoint - direction * topExtension;
            Vector2 botL = -normal * (radius + radiusExtension) + botPoint + direction * botExtension;
            Vector2 botR = normal * (radius + radiusExtension) + botPoint + direction * botExtension;

            int minX = (int)Mathf.Min(topL.x, topR.x, botL.x, botR.x);
            int minY = (int)Mathf.Min(topL.y, topR.y, botL.y, botR.y);
            int maxX = (int)Mathf.Max(topL.x, topR.x, botL.x, botR.x) + 1;
            int maxY = (int)Mathf.Max(topL.y, topR.y, botL.y, botR.y) + 1;


            Dictionary<Vector2Int, Vector2> list = new Dictionary<Vector2Int, Vector2>();
            // for each point in the area, if in slope, adjust height.
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    Vector2 pointFromTop = new Vector2(i, j) - topPoint;
                    float downDist = Vector2.Dot(pointFromTop, direction);
                    float sideDist = Vector2.Dot(pointFromTop, normal);

                    // if in slope
                    if (downDist > -topExtension && downDist < length + botExtension && Mathf.Abs(sideDist) <= radius + radiusExtension)
                    {
                        list.Add(new Vector2Int(i, j), new Vector2((length - downDist) / length, sideDist));
                    }
                }
            }

            return list;
        }

    }
}

