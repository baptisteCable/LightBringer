using System;
using UnityEngine;

namespace LightBringer.TerrainGeneration
{
    class SlopeData
    {
        public Vector2 baseVertex;

        public bool topOnRight;

        public Vector2 vec1;
        public Vector2 vec2;
        public Vector2 norm1;
        public Vector2 norm2;
        public Vector2 altVec1;
        public Vector2 altVec2;
        public Vector2 altNorm1;
        public Vector2 altNorm2;

        public float slopeDistInTurn;

        public Vector2 origin1;
        public Vector2 origin2;
        public Vector2 corner;

        public Vector2 currentPoint;
        public Vector2 vec;
        public Vector2 vecInOr1;
        public Vector2 vecInOr2;

        public float dot1;
        public float dot2;
        public float dotNorm1;
        public float dotNorm2;
        public float altDot1;
        public float altDot2;
        public float altDotNorm1;
        public float altDotNorm2;
        public float baseDistance;
        public float cornerDistance;

        public SlopeData(Vector2 baseVertex, Vector2 nextVertex, Vector2 previousVertex, bool topOnRight)
        {
            this.topOnRight = topOnRight;
            this.baseVertex = baseVertex;

            if (topOnRight)
            {
                vec1 = previousVertex - baseVertex;
                vec2 = nextVertex - baseVertex;
                norm1 = Island.RotateVector(vec1, Math.PI / 2f);
                norm2 = Island.RotateVector(vec2, -Math.PI / 2f);
            }
            else
            {
                vec1 = nextVertex - baseVertex;
                vec2 = previousVertex - baseVertex;
                norm1 = Island.RotateVector(vec1, -Math.PI / 2f);
                norm2 = Island.RotateVector(vec2, Math.PI / 2f);
            }

            slopeDistInTurn = (1 - Island.SlopeEquation((1 - Island.SLOPE_LANDING) / 2f)) / Island.CLIFF_SLOPE;

            origin1 = baseVertex + norm1 * slopeDistInTurn;
            origin2 = baseVertex + norm2 * slopeDistInTurn;

            altVec1 = (baseVertex + vec1 * (1 - Island.SLOPE_LANDING) / 2f - origin1).normalized;
            altVec2 = (baseVertex + vec2 * Island.SLOPE_DESCENT + norm2 / Island.CLIFF_SLOPE - origin2).normalized;

            if (topOnRight)
            {
                altNorm1 = Island.RotateVector(altVec1, Math.PI / 2f);
                altNorm2 = Island.RotateVector(altVec2, -Math.PI / 2f);
            }
            else
            {
                altNorm1 = Island.RotateVector(altVec1, -Math.PI / 2f);
                altNorm2 = Island.RotateVector(altVec2, Math.PI / 2f);
            }

            float b = (1 - Island.SLOPE_LANDING) / 2f;
            corner = baseVertex + vec1 * (1 + Island.SLOPE_LANDING) / 2f
                + norm1 * (
                    Island.SLOPE_WIDTH * (float)Math.Sqrt(slopeDistInTurn * slopeDistInTurn + b * b) 
                    - Island.SLOPE_LANDING * slopeDistInTurn
                ) / b;
        }

        public void SetPoint(Vector2 currentPoint)
        {
            this.currentPoint = currentPoint;

            vec = currentPoint - baseVertex;
            vecInOr1 = currentPoint - origin1;
            vecInOr2 = currentPoint - origin2;

            dot1 = Vector2.Dot(vec, vec1);
            dot2 = Vector2.Dot(vec, vec2);
            dotNorm1 = Vector2.Dot(vec, norm1);
            dotNorm2 = Vector2.Dot(vec, norm2);

            altDot1 = Vector2.Dot(vecInOr1, altVec1);
            altDot2 = Vector2.Dot(vecInOr1, altVec2);
            altDotNorm1 = Vector2.Dot(vecInOr1, altNorm1);
            altDotNorm2 = Vector2.Dot(vecInOr1, altNorm2);


            cornerDistance = (corner - currentPoint).magnitude;
            baseDistance = vec.magnitude;
        }

        public override string ToString()
        {
            return "SlopeData : \n\tbaseVertex : " + baseVertex.ToString("F3")
                + "\n\tvec1 : " + vec1.ToString("F3")
                + "\n\tvec2 : " + vec2.ToString("F3")
                + "\n\tnorm1 : " + norm1.ToString("F3")
                + "\n\tnorm2 : " + norm2.ToString("F3")
                + "\n\tor1 : " + origin1.ToString("F3")
                + "\n\tor2 : " + origin2.ToString("F3")
                + "\n\taltVec1 : " + altVec1.ToString("F3")
                + "\n\taltVec2 : " + altVec2.ToString("F3")
                + "\n\taltNorm1 : " + altNorm1.ToString("F3")
                + "\n\taltNorm2: " + altNorm2.ToString("F3")
                + "\n\tcorner: " + corner.ToString("F3")
                + "\n\tpathDistTurn: " + slopeDistInTurn;

        }

    }
}
