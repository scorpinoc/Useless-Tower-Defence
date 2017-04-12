using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core
{
    public abstract class GameCell : INotifyPropertyChanged, ICloneable
    {
        private string _name;

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

        protected GameCell()
        {
            _name = GetType().Name;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public abstract object Clone();

    }
}