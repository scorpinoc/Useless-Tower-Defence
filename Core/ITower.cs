using System;

namespace Core
{
    public interface ITower : ICloneable
    {
        string Name { get; }

        int Power { get; }
    }
}