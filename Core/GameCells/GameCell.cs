using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.GameCells
{
    public abstract class GameCell : INotifyPropertyChanged, ICloneable
    {
        private string _name;

        #region properties

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return _name; }
            protected set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        #endregion
        
        protected GameCell(string name)
        {
            Name = name;
        }

        protected GameCell()
        {
            Name = GetType().Name;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public abstract object Clone();
    }
}