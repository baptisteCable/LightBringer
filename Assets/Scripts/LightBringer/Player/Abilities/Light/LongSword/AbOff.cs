using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LightBringer.Abilities;

namespace LightBringer.Player.Abilities.Light.LongSword
{
    public class AbOff : CollisionPlayerAbility
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = true;
        private const bool CASTING_CANCELLABLE = false;

        // const
        private const float COOLDOWN_DURATION_A = 1f; // TODO 10f
        private const float COOLDOWN_DURATION_B = .1f;
        private const float CHANNELING_DURATION_A = 12f / 60f;
        private const float CHANNELING_DURATION_B = 6f / 60f;
        private const float ABILITY_DURATION_A = 6f / 60f;
        private const float ABILITY_DURATION_B = 6f / 60f;

        private const float CHANNELING_MOVE_MULTIPLICATOR = .7f;
        private const float CASTING_MOVE_MULTIPLICATOR = .3f;
        private const float DAMAGE_A = 6f;
        private const float DAMAGE_B = 6f;

        private const float VANISH_DURATION = 1f;

        private const float INTERRUPT_DURATION = .6f;

        // Colliders
        private List<Collider> encounteredCols;

        // Prefabs
        private GameObject triggerPrefab;
        private GameObject impactEffetPrefab;
        private GameObject fadeOutEffetPrefab;
        private GameObject fadeInEffetPrefab;

        // GameObjects
        private LightSword sword;
        private GameObject trigger;
        private Transform characterContainer;

        // Status
        private int currentAttack = 1;
        private bool vanished = false;
        private float forcedFadeInTime;

        // Respawn point relatively to enemy center
        private Vector3 fadeInPosition;
        private Quaternion fadeInRotation;

        public AbOff(Character character, LightSword sword) :
            base(COOLDOWN_DURATION_A, CHANNELING_DURATION_A, ABILITY_DURATION_A, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
        {
            this.sword = sword;
            triggerPrefab = Resources.Load("Player/Light/LongSword/AbOff/Trigger") as GameObject;
            impactEffetPrefab = Resources.Load("Player/Light/LongSword/ImpactEffect") as GameObject;
            //fadeOutEffetPrefab = Resources.Load("Player/Light/LongSword/AbOff/FadeOutEffect") as GameObject;
            //fadeInEffetPrefab = Resources.Load("Player/Light/LongSword/AbOff/FadeInEffect") as GameObject;

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
            encounteredCols = new List<Collider>();

            if (!vanished)
            {
                currentAttack = 1;
                character.animator.Play("AbOffa");
                channelingDuration = CHANNELING_DURATION_A;

            }
            else
            {

                // No more rotation
                character.abilityMaxRotation = 0f;

                FadeIn();

                currentAttack = 2;
                character.animator.Play("AbOffb");
                channelingDuration = CHANNELING_DURATION_B;
            }
            
            // TODO : indicator
        }

        public override void StartAbility()
        {
            base.StartAbility();

            // Trail effect
            sword.transform.Find("FxTrail").GetComponent<ParticleSystem>().Play();

            // No more rotation
            character.abilityMaxRotation = 0f;

            CreateTrigger();
            
        }

        private void FadeIn()
        {
            // TODO effect
            Debug.Log(fadeInPosition);
            Vector3 pos = character.psm.anchor.position + fadeInPosition;
            character.SetMovementMode(MovementMode.Player);
            character.transform.position = pos;
            characterContainer.rotation = fadeInRotation;
            character.psm.isTargetable = true;
            vanished = false;
            coolDownDuration = COOLDOWN_DURATION_A;
            SetLockedOtherAbilities(false);
        }

        private void FadeOut(Collider col)
        {
            // TODO effect
            fadeInPosition = col.transform.position - character.transform.position;
            fadeInRotation = Quaternion.LookRotation(new Vector3(-fadeInPosition.x, 0, -fadeInPosition.z), Vector3.up);
            character.MergeWith(col.transform);
            character.psm.isTargetable = false;
            vanished = true;
            coolDownDuration = COOLDOWN_DURATION_B;
            forcedFadeInTime = Time.time + VANISH_DURATION;
            SetLockedOtherAbilities(true);
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
                Collider col = ApplyAllDamage(DAMAGE_A);
                if (col)
                {
                    FadeOut(col);
                }
            }
            else
            {
                ApplyAllDamage(DAMAGE_B);
            }

            if (trigger != null)
            {
                GameObject.Destroy(trigger);
            }

            base.End();
        }

        private Collider ApplyAllDamage(float dmg)
        {
            Collider closestCol = null;
            float minDist = 10000f;
            float dist;
            Vector3 basePoint = character.transform.position + Vector3.up;

            // find the closest collider
            foreach (Collider col in encounteredCols)
            {
                dist = (col.ClosestPoint(basePoint) - basePoint).magnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    closestCol = col;
                }
            }

            if (closestCol != null)
            {
                if (closestCol.tag == "Enemy")
                {
                    ApplyDamage(closestCol, closestCol.ClosestPoint(basePoint), dmg);
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

        private void ApplyDamage(Collider col, Vector3 impactPoint, float dmg)
        {
            col.GetComponent<DamageController>().TakeDamage(dmg);
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
            if ((col.tag == "Enemy" || col.tag == "Shield") && !encounteredCols.Contains(col))
            {
                encounteredCols.Add(col);
            }
        }

        public override void ComputeSpecial()
        {
            if (vanished && Time.time > forcedFadeInTime)
            {
                StartChanneling();
            }
        }
    }
}