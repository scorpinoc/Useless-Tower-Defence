using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core
{
    public sealed class GameCell : INotifyPropertyChanged
    {
        public enum CellState
        {
            Empty,
            Tower,
            Road
        }

        private CellState _state;

        public CellState State
        {
            get { return _state; }
            set
            {
                if (_state == value) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        public GameCell(CellState cellState)
        {
            State = cellState;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}