using System;

namespace Core
{
    public interface ITower : ICloneable
    {
        TowerType TowerType { get; }

        int Power { get; }
    }
}