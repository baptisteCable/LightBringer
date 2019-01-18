using System.Collections;
using System.Collections.Generic;
using LightBringer.Player;
using UnityEngine;

namespace LightBringer.Enemies
{
    [RequireComponent(typeof(Motor))]
    public class StatusManager : MonoBehaviour
    {
        private const float FLASH_DURATION = .1f;

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


        void Start()
        {
            EnemyStatusBar esb = (EnemyStatusBar)(statusBarGO.GetComponent("EnemyStatusBar"));
            motor = GetComponent<Motor>();
            esb.damageController = this;

            frameDamage = new Dictionary<int, Damage>();
            frameDamageDistance = new Dictionary<int, float>();
        }

        private void Update()
        {
            ApplyAllDamages();
        }

        public void TakeDamage(Damage dmg, Character dealer, int id, float distance)
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
                    }
                    currentHP -= pair.Value.amount;
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
