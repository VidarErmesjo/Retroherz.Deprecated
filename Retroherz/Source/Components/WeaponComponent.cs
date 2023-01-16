using System;
using Retroherz.Math;

namespace Retroherz.Components
{
    public class WeaponComponent : IEquatable<WeaponComponent>
    {
        public int Id { get; }

        public float charge { get; set; }
        public bool isCharging { get; private set; }
        public Vector origin { get ; set; }
        public Vector destination { get; set; }

        public WeaponComponent(int id)
        {
            Id = id;
            charge = 0.0f;
            isCharging = false;
            destination = new Vector(0.0f, 0.0f);
        }

        public void toggleIsCharging()
        {
            isCharging = !isCharging;
        }

        public bool Equals(WeaponComponent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WeaponComponent) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(WeaponComponent left, WeaponComponent right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WeaponComponent left, WeaponComponent right)
        {
            return !Equals(left, right);
        }
    }
}
