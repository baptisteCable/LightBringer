using LightBringer.Enemies;
using UnityEngine;

namespace LightBringer.Player
{
    public class Immaterial : State
    {
        private const bool CANCELLABLE = true;

        public Immaterial(float duration) : base(CANCELLABLE, duration)
        {
        }

        public override void Start(PlayerStatusManager psm)
        {
            base.Start(psm);
            psm.playerMotor.CallForAll(PlayerMotor.M_StartImmaterial);
        }

        public override void Stop()
        {
            base.Stop();
            psm.playerMotor.CallForAll(PlayerMotor.M_StopImmaterial);
        }

        public override void Cancel()
        {
            base.Cancel();
            Stop();
        }

        public override bool IsAffectedBy(Damage dmg, Motor dealer, Vector3 origin)
        {
            if (!complete)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override Damage AlterTakenDamage(Damage dmg, Motor dealer, Vector3 origin)
        {
            if (!complete)
            {
                dmg.amount = 0;
            }
            return dmg;
        }

        public override Damage AlterDealtDamage(Damage dmg)
        {
            if (!complete)
            {
                dmg.amount /= 2f;
            }
            return dmg;
        }

        public override bool isAffectedByCC(CrowdControl cc)
        {
            if (cc.damageType == DamageType.Self)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void RecTransparentOn(Transform tr)
        {
            if (tr.tag != "Spell" && tr.tag != "UI")
            {
                Renderer renderer = tr.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material mat = tr.GetComponent<Renderer>().material;

                    if (mat.shader.name == "Standard")
                    {
                        Color color = mat.GetColor("_Color");

                        color.a = .3f;
                        mat.SetColor("_Color", color);
                        mat.SetFloat("_Mode", 3);

                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.DisableKeyword("_ALPHABLEND_ON");
                        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }
                }

                foreach (Transform child in tr)
                {
                    RecTransparentOn(child);
                }
            }
        }

        public static void RecTransparentOff(Transform tr)
        {
            if (tr.tag != "Spell" && tr.tag != "UI")
            {
                Renderer renderer = tr.GetComponent<Renderer>();

                if (renderer != null)
                {
                    Material mat = tr.GetComponent<Renderer>().material;

                    if (mat.shader.name == "Standard")
                    {
                        Color color = mat.GetColor("_Color");

                        color.a = 1f;
                        mat.SetColor("_Color", color);
                        mat.SetFloat("_Mode", 0);

                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        mat.SetInt("_ZWrite", 1);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.DisableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = -1;
                    }
                }

                foreach (Transform child in tr)
                {
                    RecTransparentOff(child);
                }
            }
        }
    }
}

