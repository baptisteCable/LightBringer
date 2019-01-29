namespace LightBringer
{
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
        Melee,
        RangeInstant,
        Projectile,
        AreaOfEffect,
        Self
    }

    public enum CrowdControlType
    {
        Stun,
        Root,
        Sleep
    }
}