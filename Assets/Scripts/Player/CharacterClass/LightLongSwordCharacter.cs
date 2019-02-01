using System.Collections;
using System.Collections.Generic;
using LightBringer.Player.Abilities.Light.LongSword;
using UnityEngine;

namespace LightBringer.Player.Class
{
    public class LightLongSwordCharacter : Character
    {
        public const int MAX_SPHERE_COUNT = 4;
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
            abilities.Add("SkillEsc", new AbEsc(this, sword));
            abilities.Add("Skill1", new Ab1(this, sword));
            abilities.Add("Skill2", new Ab2(this, sword));
            abilities.Add("SkillDef", new AbDef(this, sword));
            abilities.Add("SkillOff", new AbOff(this, sword));
            abilities.Add("SkillUlt", new AbUlt(this, sword));

            // Spheres
            sphereObjects = new List<GameObject>();
            ultiSphereCount = 0;
            spherePrefab = Resources.Load("Player/Light/LongSword/AbUlt/UltSphere") as GameObject;
            abilities["SkillUlt"].available = false;
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
                abilities["SkillUlt"].available = true;
            }
        }

        public void LoadSwordWithSpheres()
        {
            foreach (GameObject sphere in sphereObjects)
            {
                sphere.transform.parent = characterContainer;
                sphere.GetComponent<Animator>().Play("UltSphereLoadingSword");
            }
        }

        public void CancelLoadSwordWithSpheres()
        {
            foreach (GameObject sphere in sphereObjects)
            {
                sphere.transform.parent = transform;
                sphere.GetComponent<Animator>().Play("UltSphereCancelLoadingSword");
            }
        }

        public void ConsumeAllSpheres()
        {
            foreach (GameObject sphere in sphereObjects)
            {
                Destroy(sphere);
            }

            sphereObjects = new List<GameObject>();
            ultiSphereCount = 0;
            abilities["SkillUlt"].available = false;
        }

        public int GetUltiShpereCount()
        {
            return ultiSphereCount;
        }

        private IEnumerator DestroySphere(GameObject sphere)
        {
            yield return new WaitForSeconds(SPHERE_DURATION);

            if (sphere != null)
            {
                sphereObjects.Remove(sphere);
                Destroy(sphere);
                ultiSphereCount = sphereObjects.Count;
                abilities["SkillUlt"].available = false;
            }
        }
    }
}


