using System.Collections;
using System.Collections.Generic;
using LightBringer.Networking;
using LightBringer.Player;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LightBringer.Enemies
{
    [RequireComponent(typeof(Motor))]
    public class StatusManager : DelayedNetworkBehaviour
    {
        private const float FLASH_DURATION = .1f;
        private const float DISPLAY_INTERVAL = .1f;
        private const float DISPLAY_DURATION = 1f;

        // status
        public float maxHP;
        public float currentHP;
        public GameObject statusBarGO;
        public float displayHeight;
        private Motor motor;
        public bool isDead = false;

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

            frameDamage = new Dictionary<int, DamageDealer>();
            frameDamageDistance = new Dictionary<int, float>();

            damageToDisplay = new Dictionary<DamageElement, float>();
        }

        private void Update()
        {
            if (isServer)
            {
                ApplyAllDamages();
            }
        }

        public void TakeDamage(Damage dmg, PlayerMotor dealer, int id, float distance)
        {
            if (!isServer)
            {
                return;
            }

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
            if (frameDamage.Count > 0)
            {
                bool flash = false;

                foreach (KeyValuePair<int, DamageDealer> pair in frameDamage)
                {
                    if (pair.Value.dmg.amount > 0)
                    {
                        flash = true;
                        currentHP -= pair.Value.dmg.amount;

                        // add display only if local player, else send by message to client
                        if (pair.Value.dealer.isLocalPlayer)
                        {
                            AddDamageToDisplay(pair.Value.dmg);
                        }
                        else
                        {
                            TargetAddDamageToDisplay(pair.Value.dealer.connectionToClient, pair.Value.dmg.ToMessage());
                        }

                    }
                }

                frameDamage.Clear();

                if (flash)
                {
                    CallByName("Flash");
                }

                if (currentHP <= 0)
                {
                    isDead = true;
                    motor.Die();
                    Destroy(statusBarGO);
                }
            }
        }

        // Called by name
        private void Flash()
        {
            StopCoroutine(FlashCoroutine());
            StartCoroutine(FlashCoroutine());
        }

        [TargetRpc]
        private void TargetAddDamageToDisplay(NetworkConnection target, DamageMessage message)
        {
            AddDamageToDisplay(Damage.FromMessage(message));
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

        private IEnumerator FlashCoroutine()
        {
            RecFlashOn(transform);
            yield return new WaitForSeconds(FLASH_DURATION);
            RecFlashOff(transform);
        }

        private void RecFlashOn(Transform tr)
        {
            if (tr.tag != "Shield" && tr.tag != "UI")
            {
                Renderer renderer = tr.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material mat = tr.GetComponent<Renderer>().material;

                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", new Color(.2f, .1f, .1f));
                }
            }

            foreach (Transform child in tr)
            {
                RecFlashOn(child);
            }
        }

        private void RecFlashOff(Transform tr)
        {
            if (tr.tag != "Shield" && tr.tag != "UI")
            {
                Renderer renderer = tr.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material mat = tr.GetComponent<Renderer>().material;

                    mat.DisableKeyword("_EMISSION");
                }
            }

            foreach (Transform child in tr)
            {
                RecFlashOff(child);
            }
        }
    }
}
