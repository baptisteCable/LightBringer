using System.Collections;
using System.Collections.Generic;
using LightBringer.Effects;
using LightBringer.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LightBringer.Enemies
{
    [RequireComponent(typeof(Motor)), RequireComponent(typeof(FlashEffect))]
    public class StatusManager : MonoBehaviour
    {
        private const float DISPLAY_INTERVAL = .1f;
        private const float DISPLAY_DURATION = 1f;

        private const float RAGE_INCREASE_WITH_MISSED = .1f;
        private const float RAGE_INCREASE_WITH_INTERRUPTION = .2f;
        private const float RAGE_RATIO_WITH_DAMAGE = 1.5f; // ratio applied to % of max HP that the taken damages represent
        private const float RAGE_DURATION = 10; // DEBUG
        private const float EXHAUSTION_DURATION = 10; // DEBUG

        // DEBUG
        private GUIStyle frontStyle = null;
        private GUIStyle backStyle = null;

        // status
        public float maxHP;
        public float currentHP;
        public GameObject statusBarGO;
        public float displayHeight;
        public bool isDead;
        private float rageAmount;
        public Mode mode;

        // Rage
        public Mode nextMode;
        private float rageEnd;
        private float exhaustionEnd;
        public bool rageToBeStarted = false;
        public bool exhaustionToBeStarted = false;
        public bool exhaustionToBeEnded = false;

        // Components
        private Motor motor;
        private FlashEffect flashEffect;
        [SerializeField] private FlashEffect shieldFlashEffect;

        // Damage
        private Dictionary<int, DamageDealer> frameDamage;
        private Dictionary<int, float> frameDamageDistance;

        // Damage display
        private Dictionary<DamageElement, float> damageToDisplay;
        private bool dmgWaitingForDisplay = false;
        public Transform lostHpPoint;

        // UI object
        [SerializeField]
        private GameObject lostHPPrefab;

        private struct DamageDealer
        {
            public Damage dmg;
            public PlayerMotor dealer;

            public DamageDealer(Damage dmg, PlayerMotor dealer)
            {
                this.dmg = dmg;
                this.dealer = dealer;
            }
        }

        void Start()
        {
            motor = GetComponent<Motor>();
            flashEffect = GetComponent<FlashEffect>();

            frameDamage = new Dictionary<int, DamageDealer>();
            frameDamageDistance = new Dictionary<int, float>();

            damageToDisplay = new Dictionary<DamageElement, float>();
        }

        private void Update()
        {
            ApplyAllDamages();
            UpdateNextMode();
        }

        public void Init()
        {
            currentHP = maxHP;
            isDead = false;
            rageAmount = .95f; // DEBUG
        }

        private void UpdateNextMode()
        {
            if (mode == Mode.Rage && Time.time >= rageEnd)
            {
                nextMode = Mode.Exhaustion;
            }
            else if (mode == Mode.Exhaustion && Time.time >= exhaustionEnd)
            {
                nextMode = Mode.Fight;
            }
        }

        public void IncreaseRageMissedAttack()
        {
            RageIncrease(RAGE_INCREASE_WITH_MISSED);
        }

        public void IncreaseRageInterruption()
        {
            RageIncrease(RAGE_INCREASE_WITH_INTERRUPTION);
        }

        public void IncreaseRageDamageTaken(float damage)
        {
            RageIncrease(damage / maxHP * RAGE_RATIO_WITH_DAMAGE);
        }

        private void RageIncrease(float amount)
        {
            if (mode == Mode.Exhaustion || mode == Mode.Rage)
            {
                return;
            }

            rageAmount += amount;

            if (rageAmount >= 1f)
            {
                nextMode = Mode.Rage;
            }
        }

        public void RageStart()
        {
            motor.SetMode(Mode.Rage);
            rageEnd = Time.time + RAGE_DURATION;
            rageToBeStarted = true;
        }

        public void ExhaustionStart()
        {
            motor.SetMode(Mode.Exhaustion);
            exhaustionEnd = Time.time + EXHAUSTION_DURATION;
            exhaustionToBeStarted = true;
            motor.StartExhaustion();
        }

        public void ExhaustionEnd()
        {
            motor.SetMode(Mode.Fight);
            rageAmount = 0;
            exhaustionToBeEnded = true;
            motor.StopExhaustion();
        }

        public void TakeDamage(Damage dmg, PlayerMotor dealer, int id, float distance)
        {
            // If this damage id is already registered
            if (frameDamage.ContainsKey(id))
            {
                // If AoE, take the highest.
                if (dmg.type == DamageType.AreaOfEffect)
                {
                    if (dmg.amount > frameDamage[id].dmg.amount)
                    {
                        frameDamage[id] = new DamageDealer(dmg, dealer);
                        frameDamageDistance[id] = distance;
                    }
                }
                // Else, take the closest
                else
                {
                    if (distance < frameDamageDistance[id])
                    {
                        frameDamage[id] = new DamageDealer(dmg, dealer);
                        frameDamageDistance[id] = distance;
                    }
                }
            }
            // Else, new damage id
            else
            {
                frameDamage.Add(id, new DamageDealer(dmg, dealer));
                frameDamageDistance.Add(id, distance);
            }
        }

        private void ApplyAllDamages()
        {
            float newHP = currentHP;

            if (frameDamage.Count > 0)
            {
                foreach (KeyValuePair<int, DamageDealer> pair in frameDamage)
                {
                    if (pair.Value.dmg.amount > 0)
                    {
                        newHP -= pair.Value.dmg.amount;

                        // add display only if local player, else send by message to client
                        AddDamageToDisplay(pair.Value.dmg);
                    }
                }

                frameDamage.Clear();

                if (newHP < currentHP)
                {
                    IncreaseRageDamageTaken(currentHP - newHP);

                    flashEffect.Flash();

                    currentHP = newHP;
                }

                if (currentHP <= 0)
                {
                    isDead = true;
                    motor.Die();
                    Destroy(statusBarGO);
                }
            }
        }

        private void AddDamageToDisplay(Damage dmg)
        {
            if (!dmgWaitingForDisplay)
            {
                StartCoroutine(DisplayIntervalDamage());
                dmgWaitingForDisplay = true;
            }

            if (!damageToDisplay.ContainsKey(dmg.element))
            {
                damageToDisplay.Add(dmg.element, 0f);
            }

            damageToDisplay[dmg.element] += dmg.amount;
        }

        private IEnumerator DisplayIntervalDamage()
        {
            yield return new WaitForSeconds(DISPLAY_INTERVAL);

            dmgWaitingForDisplay = false;

            foreach (KeyValuePair<DamageElement, float> pair in damageToDisplay)
            {
                DisplayDamage(pair.Key, pair.Value);
            }

            damageToDisplay.Clear();
        }

        private void DisplayDamage(DamageElement element, float amount)
        {
            GameObject lostHP = Instantiate(lostHPPrefab, lostHpPoint);
            Destroy(lostHP, DISPLAY_DURATION);
            Text txt = lostHP.GetComponent<Text>();

            // amount
            txt.text = Mathf.Round(amount).ToString();

            // Color
            txt.material = DamageManager.dm.ElementMaterial(element);
        }

        public void ShieldFlash()
        {
            shieldFlashEffect.Flash();
        }

        void OnGUI()
        {
            int length = (int)(400 * rageAmount);

            InitStyles();
            GUI.Box(new Rect(20, 10, 400, 30), "", backStyle);
            GUI.Box(new Rect(20, 10, length, 30), "", frontStyle);
            GUILayout.BeginArea(new Rect(20, 45, 250, 120));
            GUILayout.Label("Rage: " + rageAmount);
            GUILayout.Label("Mode: " + mode);
            GUILayout.EndArea();
        }
        private void InitStyles()
        {
            if (frontStyle == null)
            {
                frontStyle = new GUIStyle(GUI.skin.box);
                frontStyle.normal.background = MakeTex(2, 2, new Color(.6f, .6f, 0f, 1f));
                backStyle = new GUIStyle(GUI.skin.box);
                backStyle.normal.background = MakeTex(2, 2, new Color(.3f, .3f, .3f, 1f));
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
