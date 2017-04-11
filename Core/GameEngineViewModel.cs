using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;

namespace Core
{
    public sealed class GameEngineViewModel : INotifyPropertyChanged
    {
        #region fields
        private readonly GameEngine _engine;
        #endregion

        #region properties

        public event PropertyChangedEventHandler PropertyChanged;

        #region auto
        public ICommand NewGameCommand { get; }
        public ICommand BuildTowerCommand { get; }
        public ICommand NextLevelCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand SaveToFileCommand { get; }
        public ICommand LoadFromFileCommand { get; }
        #endregion

        #region delegate
        public ObservableCollection<GameCell> Cells => _engine.Cells;
        public GameState.Size GridSize => _engine.GridSize;

        public int Gold => _engine.Gold;
        public int Level => _engine.Level;
        public int EnemiesLeft => _engine.EnemiesLeft;
        public int CurrentTurn => _engine.CurrentTurn;
        public int Lives => _engine.Lives;
        #endregion

        #endregion

        #region constructors
        public GameEngineViewModel()
        {
            _engine = new GameEngine();
            _engine.PropertyChanged += (sender, args) => OnPropertyChanged(args.PropertyName);

            BuildTowerCommand = new ParameterisedDelegateCommand<GameCell>(_engine.BuildTower,
                cell => cell?.State == GameCell.CellState.Empty, _engine);
            NextLevelCommand = new VoidDelegateCommand(_engine.NextLevel,
                o => _engine.EnemiesLeft == 0 && _engine.Lives > 0, _engine);
            NewGameCommand = new VoidDelegateCommand(_engine.NewGame);
            LoadCommand = new ParameterisedDelegateCommand<int>(_engine.UndoGameFor,
                levels => _engine.SavesCount > levels - 1, _engine);
            const string file = "test.xml";
            // todo : auto start from pause if enemies left
            SaveToFileCommand = new VoidDelegateCommand(() => _engine.SaveGameTo(file), o => _engine.EnemiesLeft == 0, _engine);
            LoadFromFileCommand = new VoidDelegateCommand(() => _engine.LoadGameFrom(file));
        }
        #endregion

        #region event handlers
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region inner classes

        private abstract class DelegateCommand<T> : ICommand
        {
            #region fields
            private readonly Predicate<T> _canExecute;
            #endregion

            #region properties 
            public event EventHandler CanExecuteChanged;
            #endregion

            #region constructors
            protected DelegateCommand(Predicate<T> canExecute, INotifyPropertyChanged notifier)
            {
                _canExecute = canExecute;
                if (notifier == null) return;
                var currentContext = SynchronizationContext.Current;
                notifier.PropertyChanged +=
                    (sender, args) =>
                        currentContext.Post(state => ((DelegateCommand<T>)state).CanExecuteUpdate(), this);
            }
            #endregion

            #region methods

            public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? false;

            public abstract void Execute(object parameter);

            protected void CanExecuteUpdate() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

            #endregion
        }

        private class VoidDelegateCommand : DelegateCommand<object>
        {
            private readonly Action _execute;

            public VoidDelegateCommand(Action execute, Predicate<object> canExecute = null, INotifyPropertyChanged notifier = null)
                : base(canExecute ?? (o => true), notifier)
            {
                _execute = execute;
            }

            public override void Execute(object parameter)
            {
                _execute?.Invoke();
                CanExecuteUpdate();
            }
        }

        private class ParameterisedDelegateCommand<T> : DelegateCommand<T>
        {
            private readonly Action<T> _execute;

            public ParameterisedDelegateCommand(Action<T> execute, Predicate<T> canExecute, INotifyPropertyChanged notifier = null)
                : base(canExecute, notifier)
            {
                _execute = execute;
            }

            public override void Execute(object parameter)
            {
                _execute?.Invoke((T)parameter);
                CanExecuteUpdate();
            }
        }
        #endregion
    }
}