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
        [SerializeField] private ParticleSystem abOffaSlash;
        [SerializeField] private ParticleSystem abOffbSlash;

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
            }
        }

        protected override void Init()
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

        protected override bool CallById(int methdodId)
        {
            if (base.CallById(methdodId))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_PlayAnimAb1a: PlayAnimAb1a(); return true;
                case M_PlayAnimAb1b: PlayAnimAb1b(); return true;
                case M_PlayAnimAb1c: PlayAnimAb1c(); return true;
                case M_Ab1aSlash: Ab1aSlash(); return true;
                case M_Ab1bSlash: Ab1bSlash(); return true;
                case M_AddUltiSphere: AddUltiSphere(); return true;
                case M_LoadSwordWithSpheres: LoadSwordWithSpheres(); return true;
                case M_CancelLoadSwordWithSpheres: CancelLoadSwordWithSpheres(); return true;
                case M_ConsumeAllSpheres: ConsumeAllSpheres(); return true;
                case M_PlayAnimAb2: PlayAnimAb2(); return true;
                case M_LoadSword: LoadSword(); return true;
                case M_UnloadSword: UnloadSword(); return true;
                case M_TrailEffect: TrailEffect(); return true;
                case M_PlayAbDef: PlayAbDef(); return true;
                case M_PlayAbEsc: PlayAbEsc(); return true;
                case M_EscTrails: EscTrails(); return true;
                case M_PlayAbOffaAndChangeChannelDuration: PlayAbOffaAndChangeChannelDuration(); return true;
                case M_PlayAbOffbAndChangeChannelDuration: PlayAbOffbAndChangeChannelDuration(); return true;
                case M_AbOffaSlash: AbOffaSlash(); return true;
                case M_AbOffbSlash: AbOffbSlash(); return true;
                case M_PlayAbUlt: PlayAbUlt(); return true;
                case M_UltLoadedEffectOn: UltLoadedEffectOn(); return true;
                case M_UltLoadedEffectOff: UltLoadedEffectOff(); return true;
                case M_Ab2DisplayIndicators: Ab2DisplayIndicators(); return true;
                case M_AbOffDisplayIndicators: AbOffDisplayIndicators(); return true;
                case M_AbUltDisplayIndicators: AbUltDisplayIndicators(); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_PlayAnimAb1a = 0;
        public void PlayAnimAb1a()
        {
            animator.Play("BotAb1a", -1, 0);
            animator.Play("TopAb1a", -1, 0);
        }

        // Called by id
        public const int M_PlayAnimAb1b = 1;
        private void PlayAnimAb1b()
        {
            animator.Play("BotAb1b");
            animator.Play("TopAb1b");
        }

        // Called by id
        public const int M_PlayAnimAb1c = 2;
        private void PlayAnimAb1c()
        {
            animator.Play("BotAb1c");
            animator.Play("TopAb1c");
        }

        // Called by id
        public const int M_Ab1aSlash = 3;
        private void Ab1aSlash()
        {
            ab1aSlash.Play();
        }

        // Called by id
        public const int M_Ab1bSlash = 4;
        private void Ab1bSlash()
        {
            ab1bSlash.Play();
        }

        // Called by id
        public const int M_AddUltiSphere = 5;
        private void AddUltiSphere()
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
                abilities[AB_ULT].available = true;
            }
        }

        // Called by id
        public const int M_LoadSwordWithSpheres = 6;
        private void LoadSwordWithSpheres()
        {
            foreach (GameObject sphere in sphereObjects)
            {
                sphere.transform.parent = characterContainer;
                sphere.GetComponent<Animator>().Play("UltSphereLoadingSword");
            }
        }

        // Called by id
        public const int M_CancelLoadSwordWithSpheres = 7;
        private void CancelLoadSwordWithSpheres()
        {
            foreach (GameObject sphere in sphereObjects)
            {
                sphere.transform.parent = transform;
                sphere.GetComponent<Animator>().Play("UltSphereCancelLoadingSword");
            }
        }

        // Called by id
        public const int M_ConsumeAllSpheres = 8;
        private void ConsumeAllSpheres()
        {
            foreach (GameObject sphere in sphereObjects)
            {
                Destroy(sphere);
            }

            sphereObjects = new List<GameObject>();
            ultiSphereCount = 0;
            abilities[AB_ULT].available = false;
        }

        // Called by id
        public const int M_PlayAnimAb2 = 9;
        private void PlayAnimAb2()
        {
            animator.Play("BotAb2");
            animator.Play("TopAb2");
        }

        // Called by id
        public const int M_LoadSword = 10;
        private void LoadSword()
        {
            sword.Load();
        }

        // Called by id
        public const int M_UnloadSword = 11;
        private void UnloadSword()
        {
            sword.Unload();
        }

        // Called by id
        public const int M_TrailEffect = 12;
        private void TrailEffect()
        {
            sword.transform.Find("FxTrail").GetComponent<ParticleSystem>().Play();
        }

        // Called by id
        public const int M_PlayAbDef = 13;
        private void PlayAbDef()
        {
            animator.Play("BotAbDef");
            animator.Play("TopAbDef");
        }

        // Called by id
        public const int M_PlayAbEsc = 14;
        private void PlayAbEsc()
        {
            animator.Play("BotAbEsc");
            animator.Play("TopAbEsc");
        }
        
        // Called by id
        public const int M_EscTrails = 15;
        private void EscTrails()
        {
            GameObject trailEffect1 = Instantiate(escTrailEffectPrefab, sword.transform);
            trailEffect1.transform.localPosition = new Vector3(-0.473f, 0.089f, 0f);
            Destroy(trailEffect1, abilities[AB_ESC].castDuration);
            GameObject trailEffect2 = Instantiate(escTrailEffectPrefab, sword.transform);
            trailEffect1.transform.localPosition = new Vector3(0.177f, 0.094f, 0f);
            Destroy(trailEffect2, abilities[AB_ESC].castDuration);
        }

        // Called by id
        public const int M_PlayAbOffaAndChangeChannelDuration = 16;
        private void PlayAbOffaAndChangeChannelDuration()
        {
            animator.Play("BotAbOffa");
            animator.Play("TopAbOffa");
            abilities[AB_OFF].channelDuration = AbOff.CHANNELING_DURATION_A;
        }

        // Called by id
        public const int M_PlayAbOffbAndChangeChannelDuration = 17;
        private void PlayAbOffbAndChangeChannelDuration()
        {
            animator.Play("BotAbOffb");
            animator.Play("TopAbOffb");
            abilities[AB_OFF].channelDuration = AbOff.CHANNELING_DURATION_B;
        }

        // Called by id
        public const int M_AbOffaSlash = 18;
        private void AbOffaSlash()
        {
            abOffaSlash.Play();
        }

        // Called by id
        public const int M_AbOffbSlash = 19;
        private void AbOffbSlash()
        {
            abOffbSlash.Play();
        }

        // Called by id
        public const int M_PlayAbUlt = 22;
        private void PlayAbUlt()
        {
            animator.Play("BotUlt");
            animator.Play("TopUlt");
        }

        // Called by id
        public const int M_UltLoadedEffectOn = 23;
        private void UltLoadedEffectOn()
        {
            sword.transform.Find("UltLoaded").gameObject.SetActive(true);
        }

        // Called by id
        public const int M_UltLoadedEffectOff = 24;
        private void UltLoadedEffectOff()
        {
            sword.transform.Find("UltLoaded").gameObject.SetActive(false);
        }

        // Called by id
        public const int M_Ab2DisplayIndicators = 25;
        private void Ab2DisplayIndicators()
        {
            GameObject indicator = Instantiate(ab2IndicatorPrefab, characterContainer);
            Destroy(indicator, abilities[AB_2].channelDuration);
            abilities[AB_2].indicators.Add(indicator);

            if (!isLocalPlayer)
            {
                Color col = indicator.GetComponent<SpriteRenderer>().color;
                col.a = .25f;
                indicator.GetComponent<SpriteRenderer>().color = col;
            }

        }

        // Called by id
        public const int M_AbOffDisplayIndicators = 27;
        private void AbOffDisplayIndicators()
        {
            GameObject indicator = Instantiate(abOffIndicatorPrefab, characterContainer);
            Destroy(indicator, AbOff.CHANNELING_DURATION_A);
            abilities[AB_OFF].indicators.Add(indicator);

            if (!isLocalPlayer)
            {
                Color col = indicator.GetComponent<SpriteRenderer>().color;
                col.a = .25f;
                indicator.GetComponent<SpriteRenderer>().color = col;
            }
        }

        // Called by id
        public const int M_AbUltDisplayIndicators = 28;
        private void AbUltDisplayIndicators()
        {
            GameObject indicator = Instantiate(abOffIndicatorPrefab, characterContainer);
            Destroy(indicator, abilities[AB_ULT].channelDuration);
            abilities[AB_ULT].indicators.Add(indicator);

            if (!isLocalPlayer)
            {
                Color col = indicator.GetComponent<SpriteRenderer>().color;
                col.a = .25f;
                indicator.GetComponent<SpriteRenderer>().color = col;
            }
        }



        protected override bool CallById(int methdodId, Vector3 vec)
        {
            if (base.CallById(methdodId, vec))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_LightSpawnPE: LightSpawnPE(vec); return true;
                case M_ImpactPE: ImpactPE(vec); return true;
                case M_LoadedImpactPE: LoadedImpactPE(vec); return true;
                case M_FadeOut: FadeOut(vec); return true;
                case M_FadeIn: FadeIn(vec); return true;
                case M_AbEscDisplayIndicator: AbEscDisplayIndicator(vec); return true;
                case M_AbEscMoveIndicator: AbEscMoveIndicator(vec); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_LightSpawnPE = 100;
        private void LightSpawnPE(Vector3 pos)
        {
            GameObject lightSpawn = Instantiate(lightSpawnEffetPrefab, null);
            lightSpawn.transform.position = pos;
            Destroy(lightSpawn, 1f);
        }

        // Called by id
        public const int M_ImpactPE = 101;
        private void ImpactPE(Vector3 impactPoint)
        {
            GameObject impactEffect = Instantiate(impactEffetPrefab, null);
            impactEffect.transform.position = impactPoint;
            if ((transform.position + Vector3.up - impactPoint).magnitude > .05f)
            {
                impactEffect.transform.rotation = Quaternion.LookRotation(transform.position + Vector3.up - impactPoint, Vector3.up);
            }
            Destroy(impactEffect, 1f);
        }

        // Called by id
        public const int M_LoadedImpactPE = 102;
        private void LoadedImpactPE(Vector3 impactPoint)
        {
            GameObject impactEffect = Instantiate(loadedImpactEffetPrefab, null);
            impactEffect.transform.position = impactPoint;
            if ((transform.position + Vector3.up - impactPoint).magnitude > .05f)
            {
                impactEffect.transform.rotation = Quaternion.LookRotation(transform.position + Vector3.up - impactPoint, Vector3.up);
            }
            Destroy(impactEffect, 1f);
        }

        // Called by id
        public const int M_FadeOut = 103;
        private void FadeOut(Vector3 pos)
        {
            // effect
            GameObject effect = Instantiate(fadeOutEffetPrefab);
            effect.transform.position = pos;
            Destroy(effect, .2f);
            GameObject lightColumn = GameObject.Instantiate(lightColumnPrefab);
            lightColumn.transform.position = pos;
            GameObject.Destroy(lightColumn, .5f);

            // Lock other abilities
            LockAbilitiesExcept(true, abilities[AB_OFF]);

            // short CD and set fadeIn time
            abilities[AB_OFF].coolDownDuration = AbOff.COOLDOWN_DURATION_B;
        }

        // Called by id
        public const int M_FadeIn = 104;
        private void FadeIn(Vector3 pos)
        {
            // Effect
            GameObject effect = Instantiate(fadeInEffetPrefab);
            effect.transform.position = pos;
            Destroy(effect, .3f);
            GameObject lightColumn = Instantiate(lightColumnPrefab);
            lightColumn.transform.position = pos;
            Destroy(lightColumn, .5f);

            // Long cooldown
            abilities[AB_OFF].coolDownDuration = AbOff.COOLDOWN_DURATION_A;

            // unlock other abilities
            LockAbilitiesExcept(false, abilities[AB_OFF]);
        }

        // Called by id
        public const int M_AbEscDisplayIndicator = 105;
        private void AbEscDisplayIndicator(Vector3 pos)
        {
            ((AbEsc)abilities[AB_ESC]).landingIndicator = GameObject.Instantiate(abEscLandingIndicatorPrefab);
            ((AbEsc)abilities[AB_ESC]).landingIndicator.transform.position = pos;
            Destroy(((AbEsc)abilities[AB_ESC]).landingIndicator, abilities[AB_ESC].channelDuration);

            GameObject indicator = Instantiate(abEscRangeIndicatorPrefab, characterContainer);
            Destroy(indicator, abilities[AB_ESC].channelDuration);

            if (!isLocalPlayer)
            {
                Color col = indicator.GetComponent<SpriteRenderer>().color;
                col.a = .25f;
                indicator.GetComponent<SpriteRenderer>().color = col;

                col = ((AbEsc)abilities[AB_ESC]).landingIndicator.GetComponent<SpriteRenderer>().color;
                col.a = .25f;
                ((AbEsc)abilities[AB_ESC]).landingIndicator.GetComponent<SpriteRenderer>().color = col;
            }
        }

        // Called by id
        public const int M_AbEscMoveIndicator = 106;
        private void AbEscMoveIndicator(Vector3 pos)
        {
            if (((AbEsc)abilities[AB_ESC]).landingIndicator != null)
            {
                ((AbEsc)abilities[AB_ESC]).landingIndicator.transform.position = pos;
            }
        }
    }
}


