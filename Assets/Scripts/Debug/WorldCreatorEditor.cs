using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    [ExecuteInEditMode]
    public class WorldCreatorEditor : MonoBehaviour
    {
        // General data
        public int GEN_SQUARE_RADIUS = 1024;
        public int NB_SQUARE_RADIUS = 0;
        public int UPP = 4;

        // Debug checkBoxed
        public bool createMapAndSaveBin = true;
        public bool loadAndPrintMap = true;

        // random
        static System.Random rnd;

        WorldCreator wc;

        // Update is called once per frame
        void Update()
        {
            if (!createMapAndSaveBin)
            {
                SetWC();
                createMapAndSaveBin = true;
                CreateMapAndSaveToBinary();
            }

            if (!loadAndPrintMap)
            {
                SetWC();
                loadAndPrintMap = true;
                LoadAndPrintMap();
            }
        }

        private void SetWC()
        {
            wc = new WorldCreator(Application.persistentDataPath + "/", GEN_SQUARE_RADIUS, .00003f, .0006f);
        }

        private void CreateMapAndSaveToBinary()
        {
            SpatialDictionary<Biome> biomes = new SpatialDictionary<Biome>();
            SpatialDictionary<Island> islands = new SpatialDictionary<Island>();

            if (NB_SQUARE_RADIUS == 0)
            {
                wc.GenerateBiomesInSquareAndNeighbourSquares(ref biomes, 0, 0);
                wc.GenerateIslandsInSquare(ref biomes, ref islands, 0, 0);
            }
            else
            {
                for (int i = -NB_SQUARE_RADIUS * GEN_SQUARE_RADIUS * 2; i <= NB_SQUARE_RADIUS * GEN_SQUARE_RADIUS * 2; i += GEN_SQUARE_RADIUS * 2)
                {
                    for (int j = -NB_SQUARE_RADIUS * GEN_SQUARE_RADIUS * 2; j <= NB_SQUARE_RADIUS * GEN_SQUARE_RADIUS * 2; j += GEN_SQUARE_RADIUS * 2)
                    {
                        wc.GenerateBiomesInSquareAndNeighbourSquares(ref biomes, i, j);
                        wc.GenerateIslandsInSquare(ref biomes, ref islands, i, j);
                    }
                }
            }
            
            wc.SaveSpDic(biomes, "biomes.dat");
            wc.SaveSpDic(islands, "islands.dat");
        }

        private void LoadAndPrintMap()
        {
            wc.LoadData(out SpatialDictionary<Biome> biomes, out SpatialDictionary<Island> islands);

            int squareRadius = (NB_SQUARE_RADIUS * 2 + 1) * GEN_SQUARE_RADIUS;

            MapPainter mp = new MapPainter();
            mp.DrawIslands(ref biomes, ref islands, 0, 0, squareRadius, UPP/*, (NB_SQUARE_RADIUS * 2 + 1)*/);
        }
    }
}
