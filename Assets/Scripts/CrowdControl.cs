namespace LightBringer
{
    public class CrowdControl
    {
        public CrowdControlType ccType;
        public DamageType damageType;
        public DamageElement element;

        public CrowdControl(CrowdControlType ccType, DamageType damageType, DamageElement element)
        {
            this.ccType = ccType;
            this.damageType = damageType;
            this.element = element;
        }
    }
}