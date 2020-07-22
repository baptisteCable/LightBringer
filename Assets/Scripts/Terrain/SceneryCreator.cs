using UnityEngine;
using System.Collections.Generic;
using LightBringer.Scenery;
using System;
using UnityEngine.SceneManagement;

namespace LightBringer.TerrainGeneration
{
    public class SceneryCreator
    {
        // Scenery density
        private const int NB_BEACON_PER_TURN = 12;

        private int terrainWidth;

        private SpatialDictionary<SceneryElement> sceneryElements;

        // locks
        object biomeIslandLock;

        // Tiles dictionary
        private Dictionary<Dic2DKey, GameObject> loadedTiles;

        // Biomes, islands
        private SpatialDictionary<Biome> biomes;
        private SpatialDictionary<Island> islands;

        // Thread messages
        private Dictionary<Dic2DKey, List<SceneryElement>> sceneryElementsToAdd;

        public SceneryCreator(
            int terrainWidth,
            object biomeIslandLock,
            SpatialDictionary<Island> islands,
            SpatialDictionary<Biome> biomes,
            SpatialDictionary<SceneryElement> sceneryElements,
            Dictionary<Dic2DKey, List<SceneryElement>> sceneryElementsToAdd,
            Dictionary<Dic2DKey, GameObject> loadedTiles
            )
        {
            this.terrainWidth = terrainWidth;
            this.biomeIslandLock = biomeIslandLock;
            this.islands = islands;
            this.biomes = biomes;
            this.sceneryElements = sceneryElements;
            this.sceneryElementsToAdd = sceneryElementsToAdd;
            this.loadedTiles = loadedTiles;
        }

        public List<SceneryElement> GenerateTileScenery(int xBase, int zBase)
        {
            xBase *= terrainWidth;
            zBase *= terrainWidth;

            List<SceneryElement> elements;

            lock (sceneryElements)
            {
                // if not in memory, generate it
                if (sceneryElements.IsEmpty(xBase + terrainWidth / 2, zBase + terrainWidth / 2, terrainWidth / 4))
                {
                    Debug.Log("Generate for " + xBase + " - " + zBase);
                    elements = new List<SceneryElement>();

                    // Look for islands to be added
                    List<Island> islandList;
                    lock (biomeIslandLock)
                    {
                        islandList = islands.GetAround(
                            xBase + terrainWidth / 2,
                            zBase + terrainWidth / 2,
                            terrainWidth / 2 + (int)((Island.MAX_RADIUS + 1) * Island.SCALE * 3)
                        );
                    }

                    Vector2 terrainPosition = new Vector2(xBase, zBase);

                    // Beacons
                    elements.AddRange (GenerateBeacons(islandList, xBase, zBase));

                    // Trees
                    elements.AddRange(GenerateTrees(islandList, xBase, zBase));

                }

                // If scenery exists, load it
                else
                {
                    Debug.Log("Load for " + xBase + " - " + zBase);
                    elements = sceneryElements.GetAround(
                        xBase + terrainWidth / 2,
                        zBase + terrainWidth / 2,
                        terrainWidth / 2
                    );
                }
            }

            return elements;
        }

        private List<SceneryElement> GenerateBeacons(List<Island> islandList, int xBase, int zBase)
        {
            List<SceneryElement> elements = new List<SceneryElement>();

            foreach (Island island in islandList)
            {
                Vector3 islandPosition = new Vector3(island.centerInWorld.x + 3, 0, island.centerInWorld.y + 3);

                // Generate beacons
                for (float angle = 0; angle < 2 * (float)Math.PI; angle += 2 * (float)Math.PI / NB_BEACON_PER_TURN)
                {
                    float dist = island.Ground1Dist(angle);

                    float convertedAngle = -angle / 2f / (float)Math.PI * 360f - 90;

                    Vector3 position = islandPosition +
                        Quaternion.AngleAxis(convertedAngle, Vector3.up) * Vector3.forward * dist * Island.SCALE;

                    if (position.x >= xBase && position.x < xBase + terrainWidth
                        && position.z >= zBase && position.z < zBase + terrainWidth)
                    {
                        bool outsideEveryG1 = true;
                        foreach (Island isl in islandList)
                        {
                            if (isl == island)
                            {
                                continue;
                            }

                            outsideEveryG1 = !isl.WorldIsInGround1(position);
                            if (!outsideEveryG1)
                            {
                                break;
                            }
                        }

                        if (outsideEveryG1)
                        {
                            SceneryElement element = new SceneryElement(position, island.biomeType, SceneryElement.type.beacon);
                            elements.Add(element);
                            sceneryElements.Add((int)position.x, (int)position.z, element);
                        }
                    }
                }
            }

            return elements;
        }

        private List<SceneryElement> GenerateTrees(List<Island> islandList, int xBase, int zBase)
        {
            List<SceneryElement> elements = new List<SceneryElement>();

            Vector3 position = new Vector3(xBase + terrainWidth / 2, 0, zBase + terrainWidth / 2);
            SceneryElement element = new SceneryElement(position, Biome.Type.Light , SceneryElement.type.tree);
            elements.Add(element);
            sceneryElements.Add((int)position.x, (int)position.z, element);

            return elements;
        }

        public void AddSceneryElements()
        {
            int counter = 0;
            lock (sceneryElementsToAdd)
            {
                List<Dic2DKey> keys = new List<Dic2DKey>(sceneryElementsToAdd.Keys);

                // load the scenery element in the terrain GO (if already created, else wait for it)
                // max 5 objects per frame
                while (keys.Count > 0 && counter < 5 && sceneryElementsToAdd[keys[0]].Count > 0 && loadedTiles.ContainsKey(keys[0]))
                {
                    SceneryElement element = sceneryElementsToAdd[keys[0]][0];
                    GameObject prefab = Resources.Load(element.PrefabPath()) as GameObject;
                    GameObject go = GameObject.Instantiate(prefab, loadedTiles[keys[0]].transform);
                    go.transform.position = element.position;

                    sceneryElementsToAdd[keys[0]].RemoveAt(0);
                    if (sceneryElementsToAdd[keys[0]].Count == 0)
                    {
                        sceneryElementsToAdd.Remove(keys[0]);
                        keys = new List<Dic2DKey>(sceneryElementsToAdd.Keys);
                    }
                    counter += 1;
                }
            }
        }
    }
}