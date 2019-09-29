using System.Collections.Generic;

namespace LightBringer.TerrainGeneration
{
    public class Neighbourhood
    {
        public List<Biome> neighbours;
        public int typedNeighbourCount;

        public Neighbourhood(SpatialDictionary<Biome> biomes, List<Dic2DKey> neighbourList)
        {
            neighbours = new List<Biome>();
            typedNeighbourCount = 0;

            foreach (Dic2DKey neighbourKey in neighbourList)
            {
                Biome biome = biomes.Get(neighbourKey);

                if (biome.type != Biome.Type.Undefined)
                {
                    typedNeighbourCount++;
                }

                neighbours.Add(biome);
            }
        }
    }
}
