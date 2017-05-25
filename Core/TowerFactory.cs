using System;
using Core.GameCells;

namespace Core
{
    public enum TowerType
    {
        Empty,
        SimpleTower,
        PowerfullTower
    }

    public static class TowerFactory
    {
        public static ITower CreateTower(this TowerType towerType)
        {
            switch (towerType)
            {
                case TowerType.Empty:
                    return new Tower(TowerType.Empty.ToString(), 0, TimeSpan.MinValue, 0);
                case TowerType.SimpleTower:
                    return new Tower(TowerType.SimpleTower.ToString(), 1, TimeSpan.FromSeconds(0.8), 50);
                case TowerType.PowerfullTower:
                    return new Tower(TowerType.PowerfullTower.ToString(), 2, TimeSpan.FromSeconds(1), 80);
                default:
                    throw new ArgumentOutOfRangeException(nameof(towerType), towerType, null);
            }
        }
    }
}