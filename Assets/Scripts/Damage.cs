using UnityEngine.Networking;

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
        
        public DamageMessage ToMessage()
        {
            DamageMessage message = new DamageMessage();
            message.amount = amount;
            message.type = (int)type;
            message.element = (int)element;
            return message;
        }

        public static Damage FromMessage(DamageMessage message)
        {
            return new Damage(message.amount, (DamageType)message.type, (DamageElement)message.element);
        }
    }
    
    public class DamageMessage : MessageBase
    {
        public float amount;
        public int type;
        public int element;
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