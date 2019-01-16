using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using LightBringer.Player.Abilities;
using LightBringer.Player.Abilities.Light.LongSword;

namespace LightBringer.Player.Class
{
    public class LightLongSwordCharacter : Character
    {
        private const int MAX_SPHERE_COUNT = 4;
        private const float SPHERE_DURATION = 30f;

        private int ultiSphereCount;
        private GameObject spherePrefab;

        private List<GameObject> sphereObjects;

        public override void Start()
        {
            base.Start();

            // Sword
            GameObject swordPrefab = Resources.Load("Player/Light/LongSword/Sword/LightLongSword") as GameObject;
            swordObject = Instantiate(swordPrefab, weaponSlotR);
            LightSword sword = swordObject.GetComponent<LightSword>();

            // Abilities
            abilities = new Ability[6];
            abilities[0] = new AbEsc(this, sword);
            abilities[1] = new Ab1(this, sword);
            abilities[2] = new Ab2(this, sword);
            abilities[3] = new AbOff(this, sword);
            abilities[4] = new AbDef(this, sword);
            abilities[5] = new AbUlt(this, sword);

            // Spheres
            sphereObjects = new List<GameObject>();
            ultiSphereCount = 0;
            spherePrefab = Resources.Load("Player/Light/LongSword/AbUlt/UltSphere") as GameObject;
            abilities[5].available = false;
        }

        public void AddUltiSphere()
        {
            if (sphereObjects.Count == MAX_SPHERE_COUNT)
            {
                Destroy(sphereObjects[0]);
                sphereObjects.RemoveAt(0);
            }

            GameObject sphere = Instantiate(spherePrefab, transform);
            sphereObjects.Add(sphere);
            StartCoroutine(DestroySphere(sphere));

            ultiSphereCount = sphereObjects.Count;

            if (ultiSphereCount == MAX_SPHERE_COUNT)
            {
                abilities[5].available = true;
            }
        }

        public bool UltiReady()
        {
            return ultiSphereCount == MAX_SPHERE_COUNT;
        }

        public void ConsumeAllSpheres()
        {
            foreach (GameObject sphere in sphereObjects)
            {
                Destroy(sphere);
            }

            sphereObjects = new List<GameObject>();
            ultiSphereCount = 0;
            abilities[5].available = false;
        }

        public int GetUltiShpereCount()
        {
            return ultiSphereCount;
        }

        private IEnumerator DestroySphere(GameObject sphere)
        {
            yield return new WaitForSeconds(SPHERE_DURATION);

            Debug.Log("Avant objet:" + sphere);
            if (sphere != null)
            {
                Debug.Log("Avant :" + sphereObjects.Count);
                sphereObjects.Remove(sphere);
                Destroy(sphere);
            }

            ultiSphereCount = sphereObjects.Count;
            abilities[5].available = false;
            Debug.Log("Après :" + sphereObjects.Count);
        }
    }
}


