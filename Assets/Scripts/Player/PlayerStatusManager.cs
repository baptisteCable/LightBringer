using System.Collections.Generic;
using LightBringer.Effects;
using LightBringer.Enemies;
using LightBringer.Networking;
using LightBringer.Tools;
using UnityEngine;

namespace LightBringer.Player
{
    [RequireComponent(typeof(PlayerMotor)), RequireComponent(typeof(FlashEffect))]
    public class PlayerStatusManager : DelayedNetworkBehaviour2
    {
        private const string IMMATERIAL_LAYER = "Immaterial";
        private const string PLAYER_LAYER = "Player";

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
        [SerializeField] private ParticleSystem hasteTrailsEffect;
        private ParticleSystem.MainModule hasteTrailsEffectMain;
        [SerializeField] private ParticleSystem immaterialCloudEffect;

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
            if (queuedStates != null && isServer)
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


        public void TakeDamage(Damage dmg, Motor dealer, Vector3 origin = default(Vector3))
        {
            foreach (State s in states)
            {
                dmg = s.AlterTakenDamage(dmg, dealer, origin);
            }

            if (dmg.amount > 0)
            {
                currentHP -= dmg.amount;

                flashEffect.Flash();
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

        protected override bool CallById(int methdodId)
        {
            if (base.CallById(methdodId))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_PlayHasteTrails: PlayHasteTrails(); return true;
                case M_StopHasteTrails: StopHasteTrails(); return true;
                case M_StartImmaterial: StartImmaterial(); return true;
                case M_StopImmaterial: StopImmaterial(); return true;
            }

            return false;
        }

        //called by id
        public const int M_PlayHasteTrails = 2;
        private void PlayHasteTrails()
        {
            hasteTrailsEffect.Play();
        }

        //called by id
        public const int M_StopHasteTrails = 3;
        private void StopHasteTrails()
        {
            hasteTrailsEffect.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }

        //called by id
        public const int M_StartImmaterial = 4;
        private void StartImmaterial()
        {
            immaterialCloudEffect.Play();
            LayerTools.recSetLayer(gameObject, PLAYER_LAYER, IMMATERIAL_LAYER);
            Immaterial.RecTransparentOn(transform);
        }

        //called by id
        public const int M_StopImmaterial = 5;
        private void StopImmaterial()
        {
            immaterialCloudEffect.Play();
            LayerTools.recSetLayer(gameObject, IMMATERIAL_LAYER, PLAYER_LAYER);
            Immaterial.RecTransparentOff(transform);
        }

        protected override bool CallById(int methdodId, float f)
        {
            if (base.CallById(methdodId, f))
            {
                return true;
            }

            switch (methdodId)
            {
                case M_HasteTrailsLength: HasteTrailsLength(f); return true;
            }

            return false;
        }

        //called by id
        public const int M_HasteTrailsLength = 200;
        private void HasteTrailsLength(float length)
        {
            hasteTrailsEffectMain.startLifetime = length;
        }
    }
}