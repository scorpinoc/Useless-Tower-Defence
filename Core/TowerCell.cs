namespace Core
{
    public sealed class TowerCell : GameCell
    {
        private Tower _tower;

        public Tower Tower
        {
            get { return _tower; }
            private set
            {
                if (_tower == value) return;
                _tower = value;
                if (_tower != null)
                    Name = _tower.GetType().Name;
                OnPropertyChanged();
            }
        }

        public TowerCell(Tower tower = null)
        {
            Tower = tower;
        }

        public void Build(Tower tower)
        {
            if (Tower == null)
                Tower = tower;
        }

        public override object Clone() => new TowerCell(Tower);
    }

    public class Tower
    {
        // TODO : implement
    }
}