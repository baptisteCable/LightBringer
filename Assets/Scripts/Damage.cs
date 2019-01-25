namespace LightBringer
{
    public class Damage
    {
        public float amount;
        public DamageType type;
        public DamageElement element;

        public Damage (float amount, DamageType type, DamageElement element)
        {
            this.amount = amount;
            this.type = type;
            this.element = element;
        }
    }
}