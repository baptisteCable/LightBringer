using UnityEngine;

namespace LightBringer
{
    public class Jump0 : Ability
    {
        private const float c_coolDownDuration = 5f;
        private const float c_abilityDuration = .5f;
        private const float c_channelingDuration = .5f;
        private const float c_height = 3f;
        private const float c_maxRange = 12f;

        private GameObject landingIndicatorPrefab;
        private GameObject rangeIndicatorPrefab;
        private GameObject landingIndicator;
        private GameObject rangeIndicator;

        public Jump0(GameObject landingIndicatorPrefab, GameObject rangeIndicatorPrefab) :
            base(c_coolDownDuration, c_channelingDuration, c_abilityDuration)
        {
            this.landingIndicatorPrefab = landingIndicatorPrefab;
            this.rangeIndicatorPrefab = rangeIndicatorPrefab;
        }

        private void computeChannelingCurves(Vector3 playerPosition)
        {
            Debug.Log("Construction de la courbe de canalisation");
            channelingCurveX = new AnimationCurve();
            channelingCurveY = new AnimationCurve();
            channelingCurveZ = new AnimationCurve();

            channelingCurveX.AddKey(new Keyframe(0f, playerPosition.x));
            channelingCurveY.AddKey(new Keyframe(0f, playerPosition.y));
            channelingCurveZ.AddKey(new Keyframe(0f, playerPosition.z));

            channelingCurveX.AddKey(new Keyframe(.25f * channelingDuration, playerPosition.x));
            channelingCurveY.AddKey(new Keyframe(.25f * channelingDuration, playerPosition.y));
            channelingCurveZ.AddKey(new Keyframe(.25f * channelingDuration, playerPosition.z));

            channelingCurveY.AddKey(new Keyframe(.75f * channelingDuration, .95f * c_height + playerPosition.y, 2 * c_height, 2 * c_height));

            channelingCurveX.AddKey(new Keyframe(1f * channelingDuration, playerPosition.x));
            channelingCurveY.AddKey(new Keyframe(1f * channelingDuration, c_height + playerPosition.y, 0f, 0f, 0f, 1f));
            channelingCurveZ.AddKey(new Keyframe(1f * channelingDuration, playerPosition.z));
        }

        private void computeAbilityCurve(Vector3 playerPosition)
        {
            jumpCurveX = new AnimationCurve();
            jumpCurveY = new AnimationCurve();
            jumpCurveZ = new AnimationCurve();

            jumpCurveX.AddKey(new Keyframe(0f, playerPosition.x));
            jumpCurveY.AddKey(new Keyframe(0f, playerPosition.y));
            jumpCurveZ.AddKey(new Keyframe(0f, playerPosition.z));

            jumpCurveX.AddKey(new Keyframe(abilityDuration, targetPosition.x));
            jumpCurveY.AddKey(new Keyframe(abilityDuration, targetPosition.y));
            jumpCurveZ.AddKey(new Keyframe(abilityDuration, targetPosition.z));
        }

        public override Vector3 GetChannelingPosition()
        {
            return new Vector3(channelingCurveX.Evaluate(channelingTime), channelingCurveY.Evaluate(channelingTime), channelingCurveZ.Evaluate(channelingTime));
        }

        public override Vector3 GetAbilityPosition()
        {
            return new Vector3(jumpCurveX.Evaluate(abilityTime), jumpCurveY.Evaluate(abilityTime), jumpCurveZ.Evaluate(abilityTime));
        }

        public override void ModifyTarget(Vector3 playerPosition, Vector3 targetPosition)
        {
            if ((playerPosition - targetPosition).magnitude < c_maxRange)
            {
                this.targetPosition = targetPosition;
            }
            else
            {
                this.targetPosition = playerPosition + (targetPosition - playerPosition).normalized * c_maxRange;
            }

            this.targetPosition.y = 0f;
            landingIndicator.transform.position = new Vector3(this.targetPosition.x, GameManager.projectorHeight, this.targetPosition.z);
        }

        public override void StartChanneling(Vector3 playerPosition)
        {
            // Spaw the landing point
            landingIndicator = Object.Instantiate(landingIndicatorPrefab);
            rangeIndicator = Object.Instantiate(rangeIndicatorPrefab);
            rangeIndicator.transform.position = new Vector3(playerPosition.x, GameManager.projectorHeight, playerPosition.z);
            Projector rangeProj = rangeIndicator.GetComponent<Projector>();
            rangeProj.orthographicSize = c_maxRange;

            computeChannelingCurves(playerPosition);
            coolDownRemaining = coolDownDuration;
            channelingTime = 0;
        }

        public override void StartAbility(Vector3 playerPosition, Vector3 targetPosition)
        {
            ModifyTarget(playerPosition, targetPosition);
            computeAbilityCurve(playerPosition);
            abilityTime = 0;
        }

        public override void End()
        {
            Object.Destroy(landingIndicator);
            Object.Destroy(rangeIndicator);
        }
    }
}