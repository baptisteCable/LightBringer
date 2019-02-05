using System.Collections;
using System.Collections.Generic;
using LightBringer.Player.Abilities.Light.LongSword;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Player.Class
{
    public class LightLongSwordMotor : PlayerMotor
    {
        public const int MAX_SPHERE_COUNT = 4;
        private const float SPHERE_DURATION = 30f;

        private int ultiSphereCount;

        // Prefabs
        public GameObject spherePrefab;
        public GameObject swordPrefab;

        private List<GameObject> sphereObjects;

        private LightSword sword;

        public override void Start()
        {
            // Sword
            swordObject = Instantiate(swordPrefab, weaponSlotR);
            sword = swordObject.GetComponent<LightSword>();

            base.Start();
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

        [ClientRpc]
        public override void RpcClientInit()
        {
            Debug.Log("RpcClientInitLLS");
            ClientInit();
            
            // Abilities
            abilities.Add("SkillEsc", new AbEsc(this, sword));
            abilities.Add("Skill1", new Ab1(this, sword));
            abilities.Add("Skill2", new Ab2(this, sword));
            abilities.Add("SkillDef", new AbDef(this, sword));
            abilities.Add("SkillOff", new AbOff(this, sword));
            abilities.Add("SkillUlt", new AbUlt(this, sword));

            Debug.Log(abilities.Keys);

            if (sphereObjects == null)
            {
                sphereObjects = new List<GameObject>();
                ultiSphereCount = 0;
                abilities["SkillUlt"].available = false;
            }
            else
            {
                ConsumeAllSpheres();
            }
        }
    }
}


