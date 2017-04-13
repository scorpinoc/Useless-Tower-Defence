namespace Core
{
    public sealed class TowerCell : GameCell
    {
        private ITower _tower;

        public bool Buildable { get; private set; }

        public ITower Tower
        {
            get { return _tower; }
            private set
            {
                _tower = value;
                Name = _tower?.TowerType.ToString();
                OnPropertyChanged();
            }
        }

        public TowerCell(ITower tower = null)
        {
            Buildable = true;
            Build(tower);
        }

        public void Build(ITower tower)
        {
            if (!Buildable) return;
            Tower = tower;
            if (Tower == null) return;
            Buildable = Tower.TowerType == TowerType.Empty;
        }

        public override object Clone() => new TowerCell(Tower);
    }
}