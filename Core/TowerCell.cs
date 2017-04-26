namespace Core
{
    public sealed class TowerCell : GameCell
    {
        private ITower _tower;

        public bool Buildable => Tower == null || Tower.Power != 0;

        public ITower Tower
        {
            get { return _tower; }
            private set
            {
                _tower = value;
                Name = _tower?.Name;
                OnPropertyChanged();
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