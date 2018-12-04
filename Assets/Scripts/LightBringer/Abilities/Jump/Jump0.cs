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
        private const bool c_channelingCancellable = false;

        private GameObject landingIndicatorPrefab;
        private GameObject rangeIndicatorPrefab;
        private GameObject landingIndicator;
        private GameObject rangeIndicator;

        private AnimationCurve channelingCurveX;
        private AnimationCurve channelingCurveY;
        private AnimationCurve channelingCurveZ;
        private AnimationCurve jumpCurveX;
        private AnimationCurve jumpCurveY;
        private AnimationCurve jumpCurveZ;

        private Vector3 targetPosition;

        public Jump0(Character character) :
            base(c_coolDownDuration, c_channelingDuration, c_abilityDuration, character, c_channelingCancellable)
        {
            landingIndicatorPrefab = Resources.Load("Projectors/CircleSAbilityIndicatorPrefab") as GameObject;
            rangeIndicatorPrefab = Resources.Load("Projectors/CircleMAbilityIndicatorPrefab") as GameObject;
        }

        private void computeChannelingCurves(Vector3 playerPosition)
        {
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

        private void computeAbilityCurve()
        {
            jumpCurveX = new AnimationCurve();
            jumpCurveY = new AnimationCurve();
            jumpCurveZ = new AnimationCurve();

            jumpCurveX.AddKey(new Keyframe(0f, character.gameObject.transform.position.x));
            jumpCurveY.AddKey(new Keyframe(0f, character.gameObject.transform.position.y));
            jumpCurveZ.AddKey(new Keyframe(0f, character.gameObject.transform.position.z));

            jumpCurveX.AddKey(new Keyframe(abilityDuration, targetPosition.x));
            jumpCurveY.AddKey(new Keyframe(abilityDuration, targetPosition.y));
            jumpCurveZ.AddKey(new Keyframe(abilityDuration, targetPosition.z));
        }

        private void ModifyTarget()
        {
            if ((character.gameObject.transform.position - character.lookingPoint).magnitude < c_maxRange)
            {
                targetPosition = character.lookingPoint;
            }
            else
            {
                targetPosition = character.gameObject.transform.position + (character.lookingPoint - character.gameObject.transform.position).normalized * c_maxRange;
            }

            targetPosition.y = 0f;
            landingIndicator.transform.position = new Vector3(this.targetPosition.x, GameManager.projectorHeight, this.targetPosition.z);
        }

        public override void StartChanneling()
        {
            character.currentChanneling = this;
            
            // Spaw the landing point
            landingIndicator = Object.Instantiate(landingIndicatorPrefab);
            rangeIndicator = Object.Instantiate(rangeIndicatorPrefab);
            rangeIndicator.transform.position = new Vector3(
                    character.gameObject.transform.position.x,
                    GameManager.projectorHeight,
                    character.gameObject.transform.position.z
                );
            Projector rangeProj = rangeIndicator.GetComponent<Projector>();
            rangeProj.orthographicSize = c_maxRange;

            computeChannelingCurves(character.gameObject.transform.position);
            coolDownRemaining = coolDownDuration;
            channelingTime = 0;
        }

        public override void Channel()
        {
            channelingTime += Time.deltaTime;

            ModifyTarget();

            if (channelingTime > channelingDuration)
            {
                StartAbility();
            }
            else
            {
                character.gameObject.transform.position = new Vector3(
                        channelingCurveX.Evaluate(channelingTime),
                        channelingCurveY.Evaluate(channelingTime),
                        channelingCurveZ.Evaluate(channelingTime)
                    );
            }
        }

        public override void StartAbility()
        {
            ModifyTarget();
            computeAbilityCurve();

            character.currentAbility = this;
            character.currentChanneling = null;
            abilityTime = 0;
        }

        public override void DoAbility()
        {
            abilityTime += Time.deltaTime;
            if (abilityTime > abilityDuration)
            {
                End();
            }
            else
            {
                character.gameObject.transform.position = new Vector3(
                        jumpCurveX.Evaluate(abilityTime),
                        jumpCurveY.Evaluate(abilityTime),
                        jumpCurveZ.Evaluate(abilityTime)
                    );
            }
        }

        public override void End()
        {
            Object.Destroy(landingIndicator);
            Object.Destroy(rangeIndicator);
            character.currentAbility = null;
        }

        public override void CancelChanelling()
        {
            throw new System.NotImplementedException();
        }
    }
}