using System.Collections.Generic;
using LightBringer.Effects;
using LightBringer.Enemies;
using UnityEngine;

namespace LightBringer.Player
{
    [RequireComponent(typeof(PlayerMotor)), RequireComponent(typeof(FlashEffect))]
    public class PlayerStatusManager : MonoBehaviour
    {
        // HP MP
        public float maxHP;
        [HideInInspector]
        public float currentHP;

        // Gradable
        [HideInInspector]
        public Dictionary<State, float> moveMultiplicators;
        [HideInInspector]
        public Dictionary<State, float> maxRotation;

        // Crowd control
        [HideInInspector]
        public bool isRooted;
        [HideInInspector]
        public bool isStunned;
        [HideInInspector]
        public float rootDuration;
        [HideInInspector]
        public float stunDuration;

        // Status
        [HideInInspector]
        public List<State> states;
        private List<State> queuedStates;
        public bool isDead;

        // Special status
        [HideInInspector]
        public Transform anchor;
        [HideInInspector]
        public bool isTargetable;
        [HideInInspector]
        public bool abilitySuppress; // Can't do anything because an ability prevents it

        // Components
        public StatusBar statusBar;
        [HideInInspector]
        public PlayerMotor playerMotor;
        private FlashEffect flashEffect;

        // Training
        public bool canDie = true;

        // State Effects
        [Header("States Effects")]
        public ParticleSystem hasteTrailsEffect;
        public ParticleSystem.MainModule hasteTrailsEffectMain;
        public ParticleSystem immaterialCloudEffect;

        void Start()
        {
            playerMotor = GetComponent<PlayerMotor>();
            flashEffect = GetComponent<FlashEffect>();

            statusBar.psm = this;

            hasteTrailsEffectMain = hasteTrailsEffect.main;
        }

        private void Update()
        {
            // get state information from server on connection
            if (queuedStates != null)
            {
                AddAndStartQueuedStates();

                foreach (State s in states)
                {
                    s.Update();
                }

                RemoveCompletedStates();
            }

        }

        private void RemoveCompletedStates()
        {
            int i = 0;

            while (i < states.Count)
            {
                if (states[i].complete)
                {
                    states.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        // return false if the damage cannot target the player
        public bool IsAffectedBy(Damage dmg, Motor dealer, Vector3 origin = default(Vector3))
        {
            foreach (State s in states)
            {
                if (!s.IsAffectedBy(dmg, dealer, origin))
                {
                    return false;
                }
            }

            return true;
        }

        // return false if the damage cannot target the player
        public bool IsAffectedByCC(CrowdControl cc)
        {
            foreach (State s in states)
            {
                if (!s.isAffectedByCC(cc))
                {
                    return false;
                }
            }

            return true;
        }

        public void TakeDamage(Damage dmg, Motor dealer, Vector3 origin = default(Vector3))
        {
            foreach (State s in states)
            {
                dmg = s.AlterTakenDamage(dmg, dealer, origin);
            }

            if (dmg.amount > 0)
            {
                flashEffect.Flash();
                currentHP -= dmg.amount;
            }

            if (currentHP <= 0 && canDie)
            {
                Die();
            }
        }

        // compute damage depending on states
        public Damage AlterDealtDamage(Damage dmg)
        {
            foreach (State s in states)
            {
                dmg = s.AlterDealtDamage(dmg);
            }

            return dmg;
        }

        public void AddAndStartState(State state)
        {
            queuedStates.Add(state);
        }

        private void AddAndStartQueuedStates()
        {
            while (queuedStates.Count > 0)
            {
                State s = queuedStates[0];
                states.Add(s);
                s.Start(this);
                queuedStates.RemoveAt(0);
            }
        }

        public void StopState(State state)
        {
            state.Stop();
        }

        public void RemoveState(State state)
        {
            states.Remove(state);
        }

        public void ApplyCrowdControl(CrowdControl cc, float duration)
        {
            bool affected = true;
            foreach (State s in states)
            {
                if (!s.isAffectedByCC(cc))
                {
                    affected = false;
                }
            }

            if (affected)
            {
                switch (cc.ccType)
                {
                    case CrowdControlType.Root: Root(duration); break;
                    case CrowdControlType.Stun: Stun(duration); break;
                }
            }
        }

        private void Stun(float duration)
        {
            isStunned = true;
            if (stunDuration < duration)
            {
                stunDuration = duration;
                playerMotor.animator.SetBool("isStunned", true);
            }
        }

        private void Root(float duration)
        {
            isRooted = true;
            if (rootDuration < duration)
            {
                rootDuration = duration;
            }
        }

        public void CCComputation()
        {
            if (isRooted)
            {
                rootDuration -= Time.deltaTime;
                if (rootDuration <= 0f)
                {
                    isRooted = false;
                }
            }

            if (isStunned)
            {
                stunDuration -= Time.deltaTime;
                if (stunDuration <= 0f)
                {
                    isStunned = false;
                    playerMotor.animator.SetBool("isStunned", false);
                }
            }
        }

        public void CancelStates()
        {
            foreach (State s in states)
            {
                if (s.cancellable)
                {
                    s.Cancel();
                }
            }
        }

        private void Die()
        {
            if (!isDead)
            {
                isDead = true;

                // TODO animation

                playerMotor.Die();

            }
        }

        public float GetStateMoveSpeedMultiplicator()
        {
            float mult = 1f;

            foreach (KeyValuePair<State, float> pair in moveMultiplicators)
            {
                mult *= pair.Value;
            }

            return mult;
        }

        public float GetMaxRotationSpeed()
        {
            float mrs = Mathf.Infinity;

            foreach (KeyValuePair<State, float> pair in maxRotation)
            {
                if (pair.Value < mrs)
                {
                    mrs = pair.Value;
                }
            }

            return mrs;
        }

        public void Init()
        {
            //HP
            currentHP = maxHP;

            // Crowd control
            isRooted = false;
            isStunned = false;
            rootDuration = 0f;
            stunDuration = 0f;
            anchor = null;
            isTargetable = true;
            abilitySuppress = false;

            // States
            states = new List<State>();
            queuedStates = new List<State>();
            isDead = false;

            // Gradables
            moveMultiplicators = new Dictionary<State, float>();
            maxRotation = new Dictionary<State, float>();
        }
    }
}