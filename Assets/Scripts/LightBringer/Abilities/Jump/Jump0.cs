using UnityEngine;

namespace LightBringer
{
    public class Jump0 : Ability
    {
        // cancelling const
        private const bool CHANNELING_CANCELLABLE = false;
        private const bool CASTING_CANCELLABLE = false;

        // const
        private const float COOLDOWN_DURATION = 4f;
        private const float CASTING_DURATION = .5f;
        private const float CHANNELING_DURATION = .5f;
        private const float HEIGHT = 3f;
        private const float MAX_RANGE = 12f;

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
            base(COOLDOWN_DURATION, CHANNELING_DURATION, CASTING_DURATION, character, CHANNELING_CANCELLABLE, CASTING_CANCELLABLE)
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

            channelingCurveY.AddKey(new Keyframe(.75f * channelingDuration, .95f * HEIGHT + playerPosition.y, 2 * HEIGHT, 2 * HEIGHT));

            channelingCurveX.AddKey(new Keyframe(1f * channelingDuration, playerPosition.x));
            channelingCurveY.AddKey(new Keyframe(1f * channelingDuration, HEIGHT + playerPosition.y, 0f, 0f, 0f, 1f));
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

            jumpCurveX.AddKey(new Keyframe(castingDuration, targetPosition.x));
            jumpCurveY.AddKey(new Keyframe(castingDuration, targetPosition.y));
            jumpCurveZ.AddKey(new Keyframe(castingDuration, targetPosition.z));
        }

        private void ModifyTarget()
        {
            if ((character.gameObject.transform.position - character.lookingPoint).magnitude < MAX_RANGE)
            {
                targetPosition = character.lookingPoint;
            }
            else
            {
                targetPosition = character.gameObject.transform.position + (character.lookingPoint - character.gameObject.transform.position).normalized * MAX_RANGE;
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
            rangeProj.orthographicSize = MAX_RANGE;

            computeChannelingCurves(character.gameObject.transform.position);
            channelingTime = 0;

            // animation
            character.animator.Play("JumpChanneling");

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
            castingTime = 0;
        }

        public override void DoAbility()
        {
            castingTime += Time.deltaTime;
            if (castingTime > castingDuration)
            {
                End();
            }
            else
            {
                character.gameObject.transform.position = new Vector3(
                        jumpCurveX.Evaluate(castingTime),
                        jumpCurveY.Evaluate(castingTime),
                        jumpCurveZ.Evaluate(castingTime)
                    );
            }
        }

        public override void End()
        {
            Object.Destroy(landingIndicator);
            Object.Destroy(rangeIndicator);
            character.currentAbility = null;
            coolDownRemaining = coolDownDuration;
        }

        public override void CancelChanelling()
        {
            throw new System.NotImplementedException();
        }
    }
}