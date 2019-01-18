using UnityEngine;
using System.Collections.Generic;
using LightBringer.Abilities;
using LightBringer.Enemies;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class AbOff : CollisionPlayerAbility
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = false;

        // const
        private const float COOLDOWN_DURATION_A = 10f;
        private const float COOLDOWN_DURATION_B = .1f;
        private const float CHANNELING_DURATION_A = 12f / 60f;
        private const float CHANNELING_DURATION_B = 12f / 60f;
        private const float ABILITY_DURATION_A = 6f / 60f;
        private const float ABILITY_DURATION_B = 6f / 60f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = .7f;
        private const float CASTING_MOVE_MULTIPLICATOR = .3f;
        private const float DAMAGE = 6f;

        private const float VANISH_DURATION = 1f;

        private const float INTERRUPT_DURATION = .6f;

        // Colliders
        private Dictionary<Collider, Vector3> encounteredCols;

        // Prefabs
        private GameObject triggerPrefab;
        private GameObject impactEffetPrefab;
        private GameObject fadeOutEffetPrefab;
        private GameObject fadeInEffetPrefab;
        private GameObject lightColumnPrefab;

        // GameObjects
        private LightSword sword;
        private GameObject trigger;
        private Transform characterContainer;

        // Status
        private int currentAttack = 1;
        private bool vanished = false;
        private float forcedFadeInTime;

        // Indicator
        private GameObject indicatorPrefab;

        // Respawn point relatively to enemy center
        Transform spawnPoint;

        public AbOff(Character character, LightSword sword) :
            base(COOLDOWN_DURATION_A, CHANNELING_DURATION_A, ABILITY_DURATION_A, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            this.sword = sword;
            triggerPrefab = Resources.Load("Player/Light/LongSword/AbOff/Trigger") as GameObject;
            impactEffetPrefab = Resources.Load("Player/Light/LongSword/ImpactEffect") as GameObject;
            fadeOutEffetPrefab = Resources.Load("Player/Light/LongSword/AbOff/FadeOutEffect") as GameObject;
            fadeInEffetPrefab = Resources.Load("Player/Light/LongSword/AbOff/FadeInEffect") as GameObject;
            lightColumnPrefab = Resources.Load("Player/Light/LongSword/AbOff/LightColumn") as GameObject;
            indicatorPrefab = Resources.Load("Player/Light/LongSword/AbOff/AbOffIndicator") as GameObject;

            characterContainer = character.gameObject.transform.Find("CharacterContainer");
        }


        public override void StartChanneling()
        {
            if (CannotStartStandard())
            {
                return;
            }

            base.StartChanneling();
            character.abilityMoveMultiplicator = CHANNELING_MOVE_MULTIPLICATOR;
            encounteredCols = new Dictionary<Collider, Vector3>();

            if (!vanished)
            {
                currentAttack = 1;
                character.animator.Play("BotAbOffa");
                character.animator.Play("TopAbOffa");
                channelDuration = CHANNELING_DURATION_A;

            }
            else
            {

                FadeInAnimation();

                currentAttack = 2;
                channelDuration = CHANNELING_DURATION_B;
            }

            // Indicator
            DisplayIndicator();
        }

        private void DisplayIndicator()
        {
            GameObject indicator = GameObject.Instantiate(indicatorPrefab, characterContainer);
            GameObject.Destroy(indicator, CHANNELING_DURATION_A);
        }

        public override void StartAbility()
        {
            base.StartAbility();

            // No more rotation
            character.abilityMaxRotation = 0f;

            if (currentAttack == 2)
            {
                FadeIn();
                character.animator.Play("BotAbOffb");
                character.animator.Play("TopAbOffb");
            }

            // Trail effect
            sword.transform.Find("FxTrail").GetComponent<ParticleSystem>().Play();

            CreateTrigger();

        }

        private void FadeInAnimation()
        {
            // Effect
            GameObject effect = GameObject.Instantiate(fadeInEffetPrefab);
            effect.transform.position = spawnPoint.position;
            GameObject.Destroy(effect, .3f);
            GameObject lightColumn = GameObject.Instantiate(lightColumnPrefab);
            lightColumn.transform.position = spawnPoint.position;
            GameObject.Destroy(lightColumn, .5f);

            // Anchor with column
            character.MergeWith(lightColumn.transform);
        }

        private void FadeIn()
        {
            // Positionning character
            character.SetMovementMode(MovementMode.Player);
            character.transform.position = spawnPoint.position;
            characterContainer.rotation = spawnPoint.rotation;

            // Reset state
            character.psm.isTargetable = true;
            vanished = false;

            // Long cooldown
            coolDownDuration = COOLDOWN_DURATION_A;

            // unlock other abilities
            character.LockAbilitiesExcept(false, this);

            // Special cancel
            character.specialCancelAbility = null;
        }

        private void FadeOut(Collider col)
        {
            // effect
            GameObject effect = GameObject.Instantiate(fadeOutEffetPrefab);
            effect.transform.position = character.transform.position;
            GameObject.Destroy(effect, .2f);
            GameObject lightColumn = GameObject.Instantiate(lightColumnPrefab);
            lightColumn.transform.position = character.transform.position;
            GameObject.Destroy(lightColumn, .5f);

            // Fade in position and rotation
            spawnPoint = col.GetComponent<BackSpawn>().backSpawPoint;

            // Anchor character
            character.MergeWith(col.transform);
            character.psm.isTargetable = false;
            vanished = true;

            // short CD and set fadeIn time
            coolDownDuration = COOLDOWN_DURATION_B;
            forcedFadeInTime = Time.time + VANISH_DURATION;

            // Lock other abilities
            character.LockAbilitiesExcept(true, this);

            // Special cancel
            character.specialCancelAbility = this;
        }

        private void CreateTrigger()
        {
            trigger = GameObject.Instantiate(triggerPrefab);
            trigger.transform.SetParent(characterContainer);
            trigger.transform.localPosition = new Vector3(0f, .1f, 0f);
            trigger.transform.localRotation = Quaternion.identity;
            AbilityColliderTrigger act = trigger.GetComponent<AbilityColliderTrigger>();
            act.SetAbility(this);
        }

        public override void End()
        {
            if (currentAttack == 1)
            {
                Collider col = ApplyAllDamage();
                if (col)
                {
                    FadeOut(col);
                }
            }
            else
            {
                ApplyAllDamage();
            }

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            base.End();
        }

        private Collider ApplyAllDamage()
        {
            Collider closestCol = null;
            float minDist = float.PositiveInfinity;
            float dist;

            int id = Random.Range(int.MinValue, int.MaxValue);

            // find the closest collider (and the extraDamage ones)
            foreach (KeyValuePair<Collider, Vector3> pair in encounteredCols)
            {
                // Extra damage: deal damage
                DamageTaker dt = pair.Key.GetComponent<DamageTaker>();
                if (dt != null && dt.extraDmg)
                {
                    ApplyDamage(pair.Key, pair.Value, id);
                }
                else
                {
                    dist = (pair.Key.ClosestPoint(pair.Value) - pair.Value).magnitude;
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestCol = pair.Key;
                    }
                }

            }

            if (closestCol != null)
            {
                if (closestCol.tag == "Enemy")
                {
                    ApplyDamage(closestCol, encounteredCols[closestCol], id);
                    return closestCol;
                }
                else if (closestCol.tag == "Shield")
                {
                    // Interrupt character
                    character.psm.Interrupt(INTERRUPT_DURATION);
                }
            }

            return null;
        }

        private void ApplyDamage(Collider col, Vector3 origin, int id)
        {
            Vector3 impactPoint = col.ClosestPoint(origin);

            Damage dmg = character.psm.AlterDealtDamage(new Damage(DAMAGE, DamageType.Melee, DamageElement.Light));
            col.GetComponent<DamageTaker>().TakeDamage(dmg, character, origin, id);

            GameObject impactEffect = GameObject.Instantiate(impactEffetPrefab, null);
            impactEffect.transform.position = impactPoint;
            impactEffect.transform.rotation = Quaternion.LookRotation(character.transform.position + Vector3.up - impactPoint, Vector3.up);
            GameObject.Destroy(impactEffect, 1f);
        }

        public override void AbortCasting()
        {
            base.AbortCasting();

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            FadeIn();
        }

        public override void OnCollision(AbilityColliderTrigger act, Collider col)
        {
            if ((col.tag == "Enemy" || col.tag == "Shield") && !encounteredCols.ContainsKey(col))
            {
                encounteredCols.Add(col, character.transform.position + Vector3.up);
            }
        }

        public override void ComputeSpecial()
        {
            if (vanished && Time.time > forcedFadeInTime)
            {
                StartChanneling();
            }
        }

        public override void SpecialCancel()
        {
            StartChanneling();
        }
    }
}