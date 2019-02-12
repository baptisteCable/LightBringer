﻿using System.Collections;
using System.Collections.Generic;
using LightBringer.Player.Abilities;
using LightBringer.Player.Abilities.Light.LongSword;
using UnityEngine;
using UnityEngine.Networking;

namespace LightBringer.Player.Class
{
    public class LightLongSwordMotor : PlayerMotor
    {
        public const int MAX_SPHERE_COUNT = 4;
        private const float SPHERE_DURATION = 30f;

        // Prefabs
        [Header("Abilities Prefabs")]
        public GameObject lightColumnPrefab;
        public GameObject ultiDTprefab;

        [Header("Trigger Prefabs")]
        public GameObject ab1abTriggerPrefab;
        public GameObject lightSpawnTriggerPrefab;
        public GameObject ab2TriggerPrefab;
        public GameObject abOffTriggerPrefab;

        [Header("Indicator Prefabs")]
        public GameObject ab2IndicatorPrefab;
        public GameObject abOffIndicatorPrefab;
        public GameObject abEscLandingIndicatorPrefab;
        public GameObject abEscRangeIndicatorPrefab;

        [Header("Effect Prefabs")]
        public GameObject lightSpawnEffetPrefab;
        public GameObject impactEffetPrefab;
        public GameObject loadedImpactEffetPrefab;
        public GameObject escTrailEffectPrefab;
        public GameObject fadeOutEffetPrefab;
        public GameObject fadeInEffetPrefab;

        [Header("Misc Prefabs")]
        public GameObject spherePrefab;
        public GameObject lightZonePrefab;


        [Header("Effects")]
        public GameObject ab1aSlash;
        public GameObject ab1bSlash;
        public GameObject abOffSlash1;
        public GameObject abOffSlash2;

        [Header("Game Objects")]
        public GameObject swordObject;

        private int ultiSphereCount;

        private List<GameObject> sphereObjects;

        [HideInInspector]
        public LightSword sword;

        public override void Start()
        {
            // Sword
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
                abilities[PlayerController.IN_AB_ULT].available = true;
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
            abilities[PlayerController.IN_AB_ULT].available = false;
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
                abilities[PlayerController.IN_AB_ULT].available = false;
            }
        }

        protected override void Init()
        {
            base.Init();

            // Abilities
            abilities = new Ability[6];
            abilities[PlayerController.IN_AB_ESC] = new AbEsc(this);
            abilities[PlayerController.IN_AB_1] = new Ab1(this);
            abilities[PlayerController.IN_AB_2] = new Ab2(this);
            abilities[PlayerController.IN_AB_DEF] = new AbDef(this);
            abilities[PlayerController.IN_AB_OFF] = new AbOff(this);
            abilities[PlayerController.IN_AB_ULT] = new AbUlt(this);

            if (sphereObjects == null)
            {
                sphereObjects = new List<GameObject>();
                ultiSphereCount = 0;
                abilities[PlayerController.IN_AB_ULT].available = false;
            }
            else
            {
                ConsumeAllSpheres();
            }
        }
    }
}


