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
    
    public enum DamageElement
    {
        Physical = 0,
        Pure = 1,
        Light = 2,
        Fire = 3,
        Ice = 4,
        Energy = 5,
        None = 6
    }

    public enum DamageType
    {
        Melee = 0,
        RangeInstant = 1,
        Projectile = 2,
        AreaOfEffect = 3,
        Self = 4
    }
}