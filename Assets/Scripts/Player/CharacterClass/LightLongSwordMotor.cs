using System.Collections;
using System.Collections.Generic;
using LightBringer.Player.Abilities;
using LightBringer.Player.Abilities.Light.LongSword;
using UnityEngine;

namespace LightBringer.Player.Class
{
    public delegate void Fonc();

    public class LightLongSwordMotor : PlayerMotor
    {
        private const int AB_ESC = 0;
        private const int AB_1 = 1;
        private const int AB_2 = 2;
        private const int AB_DEF = 3;
        private const int AB_OFF = 4;
        private const int AB_ULT = 5;

        public const int MAX_SPHERE_COUNT = 4;
        private const float SPHERE_DURATION = 35f;

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
        public GameObject fadeOutEffetPrefab;
        public GameObject fadeInEffetPrefab;

        [Header("Misc Prefabs")]
        public GameObject spherePrefab;
        public GameObject lightZonePrefab;


        [Header("Effects")]
        public ParticleSystem ab1aSlash;
        public ParticleSystem ab1bSlash;
        public ParticleSystem abOffaSlash;
        public ParticleSystem abOffbSlash;
        public ParticleSystem UltiReadyEffect;
        public ParticleSystem jumpTrails;

        [Header("Game Objects")]
        public GameObject swordObject;

        private int ultiSphereCount;

        private List<GameObject> sphereObjects;

        [HideInInspector]
        public LightSword sword;

        private void Start()
        {
            // Sword
            sword = swordObject.GetComponent<LightSword>();

            BaseStart();
        }

        private void Update()
        {
            BaseUpdate();
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
                abilities[AB_ULT].available = false;
                UltiReadyEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public override void Init()
        {
            base.Init();

            // If Abilities already exist
            if (abilities != null)
            {
                AbortAllAbilities();
            }

            // Abilities
            abilities = new Ability[6];
            abilities[AB_ESC] = new AbEsc(this, AB_ESC);
            abilities[AB_1] = new Ab1(this, AB_1);
            abilities[AB_2] = new Ab2(this, AB_2);
            abilities[AB_DEF] = new AbDef(this, AB_DEF);
            abilities[AB_OFF] = new AbOff(this, AB_OFF);
            abilities[AB_ULT] = new AbUlt(this, AB_ULT);

            if (sphereObjects == null)
            {
                sphereObjects = new List<GameObject>();
                ultiSphereCount = 0;
                abilities[AB_ULT].available = false;
            }
            else
            {
                ConsumeAllSpheres();
            }
        }

        public void AddUltiSphere()
        {
            if (sphereObjects.Count == MAX_SPHERE_COUNT)
            {
                Destroy(sphereObjects[0]);
                sphereObjects.RemoveAt(0);
            }

            GameObject sphere = Instantiate(spherePrefab, transform);

            // Set sphere effect duration and play effect
            ParticleSystem ps = sphere.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule psMain = ps.main;
            psMain.duration = SPHERE_DURATION;
            psMain.startLifetime = SPHERE_DURATION;
            ps.Play();

            // Add to sphere list
            sphereObjects.Add(sphere);

            // Plan destruction
            StartCoroutine(DestroySphere(sphere));

            ultiSphereCount = sphereObjects.Count;

            if (ultiSphereCount == MAX_SPHERE_COUNT)
            {
                abilities[AB_ULT].available = true;
                UltiReadyEffect.Play();
            }
        }

        // Light zone spawning effect
        public void LightSpawnPE(Vector3 pos)
        {
            GameObject lightSpawn = Instantiate(lightSpawnEffetPrefab, null);
            lightSpawn.transform.position = pos;
            Destroy(lightSpawn, 1f);
        }

        public void ImpactPE(Vector3 impactPoint)
        {
            GameObject impactEffect = Instantiate(impactEffetPrefab, null);
            impactEffect.transform.position = impactPoint;
            if ((transform.position + Vector3.up - impactPoint).magnitude > .05f)
            {
                impactEffect.transform.rotation = Quaternion.LookRotation(transform.position + Vector3.up - impactPoint, Vector3.up);
            }
            Destroy(impactEffect, 1f);
        }

        public void LoadedImpactPE(Vector3 impactPoint)
        {
            GameObject impactEffect = Instantiate(loadedImpactEffetPrefab, null);
            impactEffect.transform.position = impactPoint;
            if ((transform.position + Vector3.up - impactPoint).magnitude > .05f)
            {
                impactEffect.transform.rotation = Quaternion.LookRotation(transform.position + Vector3.up - impactPoint, Vector3.up);
            }
            Destroy(impactEffect, 1f);
        }

        public void LoadSwordWithSpheres()
        {
            foreach (GameObject sphere in sphereObjects)
            {
                sphere.transform.parent = characterContainer;
                sphere.GetComponent<Animator>().Play("UltSphereLoadingSword");
            }
            UltiReadyEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        public void UltLoadedEffectOn()
        {
            sword.transform.Find("UltLoaded").gameObject.SetActive(true);
        }

        public void UltLoadedEffectOff()
        {
            sword.transform.Find("UltLoaded").gameObject.SetActive(false);
        }

        public void CancelLoadSwordWithSpheres()
        {
            foreach (GameObject sphere in sphereObjects)
            {
                sphere.transform.parent = transform;
                sphere.GetComponent<Animator>().Play("UltSphereCancelLoadingSword");
            }
            UltiReadyEffect.Play();
        }

        public void ConsumeAllSpheres()
        {
            foreach (GameObject sphere in sphereObjects)
            {
                Destroy(sphere);
            }

            sphereObjects = new List<GameObject>();
            ultiSphereCount = 0;
            abilities[AB_ULT].available = false;
        }
    }
}


