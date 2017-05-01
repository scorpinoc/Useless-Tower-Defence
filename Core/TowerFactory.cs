using System;
using Core.GameCells;

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
                    return new TowerInfo(TowerType.Empty, 0, TimeSpan.MinValue, 0);
                case TowerType.SimpleTower:
                    return new TowerInfo(TowerType.SimpleTower, 1,TimeSpan.FromSeconds(0.8), 50);
                case TowerType.PowerfullTower:
                    return new TowerInfo(TowerType.PowerfullTower, 2, TimeSpan.FromSeconds(1), 80);
                default:
                    throw new ArgumentOutOfRangeException(nameof(towerType), towerType, null);
            }
        }

        public ITower GetTower(TowerType towerType) => GeTowerInfoFor(towerType).GetTower();

        #endregion

        #region inner classes

        // todo rework to use with real ITower
        public sealed class TowerInfo
        {
            public string Name { get; }
            public int AttackPower { get; }
            public TimeSpan AttackSpeed { get; }
            public int Cost { get; }

            private TowerInfo(string name, int attackPower, TimeSpan attackSpeed, int cost)
            {
                Name = name;
                AttackPower = attackPower;
                AttackSpeed = attackSpeed;
                Cost = cost;
            }

            public TowerInfo(TowerType towerType, int attackPower, TimeSpan attackSpeed, int cost)
                : this(towerType.ToString(), attackPower, attackSpeed, cost)
            { }

            public ITower GetTower() => new Tower(Name, AttackPower, AttackSpeed);
        }

        #endregion
    }
}