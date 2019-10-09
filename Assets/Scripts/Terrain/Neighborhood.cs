using System.Collections.Generic;

namespace LightBringer.TerrainGeneration
{
    public class Neighborhood
    {
        public List<Biome> neighbors;
        public int typedNeighborCount;

        public Neighborhood(SpatialDictionary<Biome> biomes, List<Dic2DKey> neighborList)
        {
            neighbors = new List<Biome>();
            typedNeighborCount = 0;

            foreach (Dic2DKey neighborKey in neighborList)
            {
                Biome biome = biomes.Get(neighborKey);

                if (biome.type != Biome.Type.Undefined)
                {
                    typedNeighborCount++;
                }

                neighbors.Add(biome);
            }
        }
    }
}
