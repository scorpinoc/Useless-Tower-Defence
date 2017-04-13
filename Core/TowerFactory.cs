using System;

namespace Core
{
    public class TowerFactory
    {
        #region methods

        #region pure methods
        public TowerInfo GeTowerInfoFor(TowerType towerType)
        {
            switch (towerType)
            {
                case TowerType.Empty:
                    return null;
                case TowerType.SimpleTower:
                    return new TowerInfo(TowerType.SimpleTower, 1, 50);
                case TowerType.PowerfullTower:
                    return new TowerInfo(TowerType.PowerfullTower, 2, 80);
                default:
                    throw new ArgumentOutOfRangeException(nameof(towerType), towerType, null);
            }
        }

        public ITower GetTower(TowerType towerType) => GetTower(GeTowerInfoFor(towerType));

        public ITower GetTower(TowerInfo towerInfo) => new Tower(towerInfo);

        #endregion

        #endregion

        #region inner classes

        public sealed class TowerInfo
        {
            public TowerType TowerType { get; }
            public int Power { get; }
            public int Cost { get; }

            public TowerInfo(TowerType towerType, int power, int cost)
            {
                TowerType = towerType;
                Power = power;
                Cost = cost;
            }
        }

        private class Tower : ITower
        {
            public TowerType TowerType { get; }

            public int Power { get; }

            public Tower(int power, TowerType towerType)
            {
                Power = power;
                TowerType = towerType;
            }
            public Tower(TowerInfo towerInfo)
                : this(towerInfo?.Power ?? 0, towerInfo?.TowerType ?? TowerType.Empty)
            { }

            public object Clone() => new Tower(Power, TowerType);
        }

        #endregion
    }
}
