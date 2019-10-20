using UnityEngine;

namespace LightBringer.Player
{
    class MovementCurve
    {
        float curveStart;
        float curveEnd;
        AnimationCurve xCurve;
        AnimationCurve yCurve;
        AnimationCurve zCurve;

        public MovementCurve (float duration, AnimationCurve xCurve, AnimationCurve yCurve, AnimationCurve zCurve)
        {
            curveStart = Time.time;
            curveEnd = Time.time + duration;
            this.xCurve = xCurve;
            this.yCurve = yCurve;
            this.zCurve = zCurve;
        }

        public Vector3 GetPosition ()
        {
            Vector3 position = new Vector3 ();
            position.x = xCurve.Evaluate (Time.time - curveStart);
            position.y = yCurve.Evaluate (Time.time - curveStart);
            position.z = zCurve.Evaluate (Time.time - curveStart);
            return position;
        }

        public bool isEnded ()
        {
            return Time.time >= curveEnd;
        }
    }
}
