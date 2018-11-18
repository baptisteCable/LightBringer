using UnityEngine;

namespace LightBringer
{
    public abstract class Ability
    {
        public bool coolDownUp { get; set; }
        public float coolDownRemaining { get; set; }
        public float coolDownDuration { get; set; }
        public float abilityDuration { get; set; }
        public float abilityTime { get; set; }
        public float channelingDuration { get; set; }
        public float channelingTime { get; set; }
        public AnimationCurve channelingCurveX { get; set; }
        public AnimationCurve channelingCurveY { get; set; }
        public AnimationCurve channelingCurveZ { get; set; }
        public AnimationCurve jumpCurveX { get; set; }
        public AnimationCurve jumpCurveY { get; set; }
        public AnimationCurve jumpCurveZ { get; set; }
        public GameObject indicatorPrefab { get; set; }
        public GameObject indicator { get; set; }
        public Vector3 targetPosition;

        public Ability(float coolDownDuration, float channelingDuration, float abilityDuration, GameObject indicatorPrefab)
        {
            coolDownUp = true;
            this.coolDownDuration = coolDownDuration;
            this.channelingDuration = channelingDuration;
            this.abilityDuration = abilityDuration;
            this.indicatorPrefab = indicatorPrefab;
        }

        public abstract Vector3 GetChannelingPosition();

        public abstract Vector3 GetAbilityPosition();

        public abstract void ModifyTarget(Vector3 playerPosition, Vector3 targetPosition);

        public abstract void StartChanneling(Vector3 playerPosition);

        public abstract void StartAbility(Vector3 playerPosition, Vector3 targetPosition);

        public abstract void End();
    }
}

