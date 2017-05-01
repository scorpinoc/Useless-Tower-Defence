using System;

namespace Core.GameCells
{
    public interface ITower : ICloneable
    {
        string Name { get; }

        int AttackPower { get; }
        
        TimeSpan AttackSpeed { get; }

        GameState Owner { get; set; }
    }
}