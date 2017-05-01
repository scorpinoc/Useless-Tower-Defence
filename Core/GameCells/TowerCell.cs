namespace Core.GameCells
{
    public sealed class TowerCell : GameCell
    {
        private ITower _tower;
        private GameState _owner;

        public bool Buildable => Tower == null || Tower.AttackPower == 0; // todo create better check

        public ITower Tower
        {
            get { return _tower; }
            private set
            {
                _tower = value;
                Name = _tower?.Name;
                if (_tower != null)
                    _tower.Owner = Owner;
                OnPropertyChanged();
            }
        }

        public GameState Owner
        {
            private get { return _owner; }
            set
            {
                if (_owner != null) return;
                _owner = value;
                if (Tower == null || Tower?.Owner != null) return;
                Tower.Owner = Owner;
            }
        }

        public TowerCell(ITower tower = null)
        {
            Build(tower);
        }

        public void Build(ITower tower)
        {
            if (!Buildable) return;
            Tower = tower;
        }

        public override object Clone() => new TowerCell((ITower)Tower?.Clone());
    }
}