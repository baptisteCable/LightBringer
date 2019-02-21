using System.Collections;
using System.Collections.Generic;
using LightBringer.Effects;
using LightBringer.Networking;
using LightBringer.Player;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LightBringer.Enemies
{
    [RequireComponent(typeof(Motor)), RequireComponent(typeof(FlashEffect))]
    public class StatusManager : DelayedNetworkBehaviour2
    {
        private const float DISPLAY_INTERVAL = .1f;
        private const float DISPLAY_DURATION = 1f;

        // status
        public float maxHP;
        public float currentHP;
        public GameObject statusBarGO;
        public float displayHeight;
        public bool isDead = false;

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
            float newHP = currentHP;

            if (frameDamage.Count > 0)
            {
                foreach (KeyValuePair<int, DamageDealer> pair in frameDamage)
                {
                    if (pair.Value.dmg.amount > 0)
                    {
                        newHP -= pair.Value.dmg.amount;

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

                if (newHP < currentHP)
                {
                    CallForAll(M_SetHP, newHP);
                }
                
                if (currentHP <= 0)
                {
                    isDead = true;
                    motor.Die();
                    Destroy(statusBarGO);
                }
            }
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

        protected override bool CallById(int methdodId)
        {
            if (base.CallById(methdodId))
            {
                return true;
            }
            switch (methdodId)
            {
                case M_ShieldFlash: ShieldFlash(); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_ShieldFlash = 0;
        private void ShieldFlash()
        {
            shieldFlashEffect.Flash();
        }

        protected override bool CallById(int methdodId, float value)
        {
            if (base.CallById(methdodId, value))
            {
                return true;
            }
            switch (methdodId)
            {
                case M_SetHP: SetHP(value); return true;
            }

            Debug.LogError("No such method Id: " + methdodId);
            return false;
        }

        // Called by id
        public const int M_SetHP = 200;
        private void SetHP(float hp)
        {
            if (hp < currentHP)
            {
                flashEffect.Flash();
            }

            currentHP = hp;
        }
    }
}
