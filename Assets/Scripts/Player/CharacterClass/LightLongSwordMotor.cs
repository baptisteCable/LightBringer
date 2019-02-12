using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LightBringer.Networking;
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
        [SerializeField] private ParticleSystem ab1aSlash;
        [SerializeField] private ParticleSystem ab1bSlash;
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

        public void CallByName(string methodName)
        {
            if (isServer)
            {
                RpcCallByName(methodName, Time.time);

                MethodInfo mi = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                mi.Invoke(this, null);
            }
        }

        [ClientRpc]
        private void RpcCallByName(string methodName, float time)
        {
            if (!isServer)
            {
                StartCoroutine(CallByNameWithDelay(methodName, NetworkSynchronization.singleton.GetLocalTimeFromServerTime(time) - Time.time));
            }
        }

        private IEnumerator CallByNameWithDelay(string methodName, float delay)
        {
            yield return new WaitForSeconds(delay);

            MethodInfo mi = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(this, null);
        }

        private void PlayAnimAb1a()
        {
            animator.Play("BotAb1a", -1, 0);
            animator.Play("TopAb1a", -1, 0);
        }

        private void PlayAnimAb1b()
        {
            animator.Play("BotAb1b");
            animator.Play("TopAb1b");
        }

        private void PlayAnimAb1c()
        {
            animator.Play("BotAb1c");
            animator.Play("TopAb1c");
        }

        private void Ab1aSlash()
        {
            ab1aSlash.Play();
        }

        private void Ab1bSlash()
        {
            ab1bSlash.Play();
        }
    }
}


