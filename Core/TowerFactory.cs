using System;

namespace Core
{
    public class TowerFactory
    {
        public enum TowerType
        {
            Empty,
            SimpleTower,
            PowerfullTower
        }

        #region methods

        public TowerInfo GeTowerInfoFor(TowerType towerType)
        {
            switch (towerType)
            {
                case TowerType.Empty:
                    return new TowerInfo(TowerType.Empty, 0, 0);
                case TowerType.SimpleTower:
                    return new TowerInfo(TowerType.SimpleTower, 1, 50);
                case TowerType.PowerfullTower:
                    return new TowerInfo(TowerType.PowerfullTower, 2, 80);
                default:
                    throw new ArgumentOutOfRangeException(nameof(towerType), towerType, null);
            }
        }

        public ITower GetTower(TowerType towerType) => GeTowerInfoFor(towerType).GetTower();

        #endregion

        #region inner classes

        public sealed class TowerInfo
        {
            public string Name { get; }
            public int Power { get; }
            public int Cost { get; }

            private TowerInfo(string name, int power, int cost)
            {
                Name = name;
                Power = power;
                Cost = cost;
            }

            public TowerInfo(TowerType towerType, int power, int cost)
                : this(towerType.ToString(), power, cost)
            { }

            public ITower GetTower() => new Tower(Name, Power);
        }

        #endregion
    }
}