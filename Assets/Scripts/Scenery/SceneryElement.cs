using System;
using LightBringer.TerrainGeneration;
using UnityEngine;

namespace LightBringer.Scenery
{
    [Serializable]
    public struct SceneryElement
    {
        [NonSerialized]
        public static readonly string[] types = { "Beacons", "Trees", "Grass", "Rocks" };

        [Serializable]
        public enum type
        {
            beacon = 0,
            tree = 1,
            grassZone = 2,
            rock = 3
        }

        public Vector3 position;
        public Biome.Type biome;
        public type elementType;
        public int version;

        public SceneryElement (Vector3 position, Biome.Type biome, type elementType, int version = 0)
        {
            this.position = position;
            this.biome = biome;
            this.elementType = elementType;
            this.version = version;
        }

        public string PrefabPath()
        {
            return "Scenery/" + types[(int)elementType] + "/" + Biome.GetName(biome) + version;
        }
    }
}