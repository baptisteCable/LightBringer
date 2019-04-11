using UnityEngine;

namespace LightBringer.Player
{
    public class Haste : State
    {
        private const bool CANCELLABLE = false;

        private const int IMMATERIAL_LAYER = 12;
        private const int PLAYER_LAYER = 10;

        private const float HASTE_DURATION = 2f;
        private const float MAX_SPEED_DURATION = .25f; // in proportion of the total duration
        private const float ADDITIONNAL_SPEED = 1f; // Additionnal coeff to base apply to speed. 1 = +100% = x2. 0 = +0% = x1
        
        private AnimationCurve speedCurve;

        private float lastLength = 0;

        public Haste(float duration = HASTE_DURATION) : base(CANCELLABLE, duration) {
        }

        public override void Start(PlayerStatusManager psm)
        {
            base.Start(psm);

            ComputeCurve();

            psm.moveMultiplicators.Add(this, 1f);
            psm.hasteTrailsEffect.Play();
            lastLength = 0f;
        }

        public override void Update()
        {
            float currentSpeed = speedCurve.Evaluate(Time.time - startTime);
            psm.moveMultiplicators[this] = 1 + currentSpeed;
            float length = .2f + .5f * currentSpeed / ADDITIONNAL_SPEED;

            if (Mathf.Abs(lastLength - length) >= .1f)
            {
                lastLength = length;
                psm.hasteTrailsEffectMain.startLifetime = length;
            }

            base.Update();
        }

        public override void Stop()
        {
            base.Stop();

            psm.hasteTrailsEffect.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            psm.moveMultiplicators.Remove(this);
        }

        private void ComputeCurve()
        {
            speedCurve = new AnimationCurve();
            speedCurve.AddKey(new Keyframe(0f, ADDITIONNAL_SPEED, 0f, 0f));
            speedCurve.AddKey(new Keyframe(.5f, ADDITIONNAL_SPEED, 0f, 0f));
            speedCurve.AddKey(new Keyframe(2f, 0f, - ADDITIONNAL_SPEED * (HASTE_DURATION - MAX_SPEED_DURATION) / 3f, 0f));
        }
    }
}

