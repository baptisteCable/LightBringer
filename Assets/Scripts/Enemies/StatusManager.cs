using System.Collections;
using System.Collections.Generic;
using LightBringer.Enemies.Knight;
using LightBringer.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LightBringer.Enemies
{
    [RequireComponent(typeof(Motor))]
    public class StatusManager : MonoBehaviour
    {
        private const float FLASH_DURATION = .1f;
        private const float DISPLAY_GAP = .1f;
        private const float DISPLAY_DURATION = 1f;

        // status
        public float maxHP;
        public float currentHP;
        public GameObject statusBarGO;
        public float displayHeight;
        private Motor motor;
        public bool isDead = false;

        // Damage
        private Dictionary<int, Damage> frameDamage;
        private Dictionary<int, float> frameDamageDistance;

        // Damage display
        private Dictionary<DamageElement, float> damageToDisplay;
        private bool dmgWaitingForDisplay = false;
        public Transform lostHpPoint;

        // UI object
        private GameObject lostHPPrefab;

        void Start()
        {
            EnemyStatusBar esb = (EnemyStatusBar)(statusBarGO.GetComponent("EnemyStatusBar"));
            motor = GetComponent<Motor>();
            esb.damageController = this;

            frameDamage = new Dictionary<int, Damage>();
            frameDamageDistance = new Dictionary<int, float>();

            damageToDisplay = new Dictionary<DamageElement, float>();

            lostHPPrefab = Resources.Load("Fight/LostHP") as GameObject;
        }

        private void Update()
        {
            ApplyAllDamages();
        }

        public void TakeDamage(Damage dmg, PlayerMotor dealer, int id, float distance)
        {
            // If this damage id is already registered
            if (frameDamage.ContainsKey(id))
            {
                // If AoE, take the highest.
                if (dmg.type == DamageType.AreaOfEffect)
                {
                    if (dmg.amount > frameDamage[id].amount)
                    {
                        frameDamage[id] = dmg;
                        frameDamageDistance[id] = distance;
                    }
                }
                // Else, take the closest
                else
                {
                    if (distance < frameDamageDistance[id])
                    {
                        frameDamage[id] = dmg;
                        frameDamageDistance[id] = distance;
                    }
                }
            }
            // Else, new damage id
            else
            {
                frameDamage.Add(id, dmg);
                frameDamageDistance.Add(id, distance);
            }
        }

        private void ApplyAllDamages()
        {
            if (frameDamage.Count > 0)
            {
                bool flash = false;

                foreach (KeyValuePair<int, Damage> pair in frameDamage)
                {
                    if (pair.Value.amount > 0)
                    {
                        flash = true;
                        currentHP -= pair.Value.amount;
                        AddDamageToDisplay(pair.Value);
                    }
                }

                frameDamage.Clear();

                if (flash)
                {
                    StopCoroutine("Flash");
                    StartCoroutine("Flash");
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
                StartCoroutine(DisplayGapDamage());
                dmgWaitingForDisplay = true;
            }

            if (!damageToDisplay.ContainsKey(dmg.element))
            {
                damageToDisplay.Add(dmg.element, 0f);
            }

            damageToDisplay[dmg.element] += dmg.amount;
        }

        private IEnumerator DisplayGapDamage()
        {
            yield return new WaitForSeconds(DISPLAY_GAP);

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
            switch (element)
            {
                case DamageElement.Energy:
                    txt.material = Resources.Load("Fight/EnergyDmg") as Material; break;
                case DamageElement.Fire:
                    txt.material = Resources.Load("Fight/FireDmg") as Material; break;
                case DamageElement.Ice:
                    txt.material = Resources.Load("Fight/IceDmg") as Material; break;
                case DamageElement.Light:
                    txt.material = Resources.Load("Fight/LightDmg") as Material; break;
                case DamageElement.Pure:
                    txt.material = Resources.Load("Fight/PureDmg") as Material; break;
                default:
                    txt.material = Resources.Load("Fight/PhysicalDmg") as Material; break;
            }
        }

        private IEnumerator Flash()
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
