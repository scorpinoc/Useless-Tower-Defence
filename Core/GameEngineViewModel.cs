using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using Timer = System.Timers.Timer;

namespace Core
{
    public sealed class GameEngineViewModel : INotifyPropertyChanged
    {
        #region fields
        private GameStateOwner _stateOwner;
        private TowerType _currentTowerType;
        #endregion

        #region properties

        public event PropertyChangedEventHandler PropertyChanged;

        #region auto
        private object Locker { get; }
        private Timer Timer { get; }
        private GameStateSaver GameStateSaver { get; }
        private TowerFactory TowerFactory { get; }

        public IEnumerable<TowerType> TowerTypes { get; }

        public ICommand NewGameCommand { get; }
        public ICommand BuildTowerCommand { get; }
        public ICommand NextLevelCommand { get; }
        public ICommand UndoTurnCommand { get; }
        public ICommand SaveToFileCommand { get; }
        public ICommand LoadFromFileCommand { get; }
        public ICommand SetTowerTypeCommand { get; }
        #endregion

        #region delegate
        private GameStateOwner StateOwner
        {
            get { return _stateOwner; }
            set
            {
                _stateOwner = value;
                _stateOwner.PropertyChanged += (sender, args) =>
                {
                    // ReSharper disable once ExplicitCallerInfoArgument
                    OnPropertyChanged(null);
                    // ReSharper disable once ExplicitCallerInfoArgument
                    _stateOwner.CurrentGameState.PropertyChanged += (o, eventArgs) => OnPropertyChanged(eventArgs.PropertyName);
                };
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(null);
            }
        }

        private TowerType CurrentTowerType
        {
            get { return _currentTowerType; }
            set
            {
                _currentTowerType = value;
                OnPropertyChanged();
            }
        }

        private GameState GameState => StateOwner.CurrentGameState;

        public ObservableCollection<GameCell> Cells => GameState.Cells;
        public GameState.Size GridSize => GameState.GridSize;

        public int Gold => GameState.Gold;
        public int Level => GameState.Level;
        public int EnemiesLeft => GameState.EnemiesLeft;
        public int CurrentTurn => GameState.CurrentTurn;
        public int Lives => GameState.Lives;
        #endregion

        #endregion

        #region constructors
        public GameEngineViewModel()
        {
            Locker = new object();
            Timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
            Timer.Elapsed += (sender, args) =>
            {
                if (NextTurnCanExecute())
                    NextTurnExecute();
                else
                    Timer.Stop();
            };
            GameStateSaver = new GameStateSaver();
            TowerFactory = new TowerFactory();
            TowerTypes = Enum.GetValues(typeof(TowerType)).Cast<TowerType>().Skip(1);

            #region Commands

            BuildTowerCommand = new ParameterisedDelegateCommand<GameCell>(BuildTowerExecute, BuildTowerCanExecute, this);
            NextLevelCommand = new VoidDelegateCommand(NextLevelExecute, o => NextLevelCanExecute(), this);
            // menu commands
            NewGameCommand = new VoidDelegateCommand(NewGame);
            UndoTurnCommand = new ParameterisedDelegateCommand<int>(UndoGameExecute, UndoGameCanExecute, this);
            SetTowerTypeCommand = new ParameterisedDelegateCommand<TowerType>(SetTowerTypeExecute, SetTowerTypeCanExecute, this);

            const string file = "test.xml";
            // todo : auto start from pause if enemies left ?
            // todo : select file 
            SaveToFileCommand = new VoidDelegateCommand(() => SaveGameTo(file), o => EnemiesLeft == 0, this);
            LoadFromFileCommand = new VoidDelegateCommand(() => LoadGameFrom(file));

            #endregion

            NewGame();
        }
        #endregion

        #region methods

        #region event invokers

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region pure methods

        private void NewGame()
        {
            Timer.Stop();

            GameState gameState;
            if (new Random().Next(2) == 0)
                gameState = new GameState(
                    new ObservableCollection<GameCell>
                    {
                        new TowerCell(), new TowerCell(), new RoadCell(),   new TowerCell(),
                        new TowerCell(), new RoadCell(),  new RoadCell(),   new TowerCell(),
                        new TowerCell(), new RoadCell(),  new TowerCell(),  new TowerCell(),
                        new TowerCell(), new RoadCell(),  new RoadCell(),   new TowerCell(),
                        new TowerCell(), new TowerCell(), new RoadCell(),   new TowerCell()
                    },
                    new GameState.Size { Height = 5, Width = 4 });
            else
                gameState = new GameState(
                    new ObservableCollection<GameCell>
                    {
                       new TowerCell(), new RoadCell(),  new TowerCell(),
                       new TowerCell(), new RoadCell(),  new TowerCell(),
                       new TowerCell(), new RoadCell(),  new TowerCell(),
                       new TowerCell(), new RoadCell(),  new TowerCell(),
                       new TowerCell(), new RoadCell(),  new TowerCell()
                    },
                    new GameState.Size { Height = 5, Width = 3 });

            StateOwner = new GameStateOwner(gameState);
            // ReSharper disable once ExplicitCallerInfoArgument
            GameState.PropertyChanged += (o, eventArgs) => OnPropertyChanged(eventArgs.PropertyName);
        }

        private void NextTurnExecute()
        {
            lock (Locker)
            {
                GameState.EnemiesLeft -= GameState.Cells.OfType<TowerCell>().Sum(cell => cell.Tower?.Power ?? 0);
                if (GameState.EnemiesLeft != 0)
                {
                    if (++GameState.CurrentTurn <= 10) return;
                    --GameState.EnemiesLeft;
                    --GameState.Lives;
                    if (GameState.EnemiesLeft > 0 && GameState.Lives > 0)
                        return;
                }
                GameState.CurrentTurn = 0;
            }
        }

        private bool NextTurnCanExecute() => GameState.EnemiesLeft > 0 && GameState.Lives > 0;

        private void BuildTowerExecute(GameCell cell)
        {
            var towerCell = cell as TowerCell;
            if (towerCell == null) return;
            var towerInfo = TowerFactory.GeTowerInfoFor(CurrentTowerType);
            towerCell.Build(TowerFactory.GetTower(towerInfo));
            GameState.Gold -= towerInfo.Cost;
        }

        private bool BuildTowerCanExecute(GameCell cell)
            =>
                ((cell as TowerCell)?.Buildable ?? false) && CurrentTowerType != TowerType.Empty &&
                SetTowerTypeCanExecute(CurrentTowerType);

        private void NextLevelExecute()
        {
            StateOwner.Save();
            GameState.EnemiesLeft = ++GameState.Level * GameState.Cells.OfType<RoadCell>().Count();
            Timer.Start();
        }

        private bool NextLevelCanExecute() => GameState.EnemiesLeft == 0 && GameState.Lives > 0;

        private void UndoGameExecute(int turns = 1)
        {
            for (; turns > 0; --turns)
                StateOwner.Load();
        }

        private bool UndoGameCanExecute(int turns = 1) => turns >= 1 && turns <= StateOwner.SavesCount;

        private void SaveGameTo(string file) => GameStateSaver.SaveTo(file, StateOwner.GameStates);

        private void LoadGameFrom(string file)
        {
            Timer.Stop();
            StateOwner = new GameStateOwner(GameStateSaver.LoadFrom(file));
        }

        private void SetTowerTypeExecute(TowerType towerType) => CurrentTowerType = towerType;

        private bool SetTowerTypeCanExecute(TowerType towerType) => TowerFactory.GeTowerInfoFor(towerType).Cost <= Gold;

        #endregion

        #endregion

        #region inner classes

        private abstract class DelegateCommand<T> : ICommand
        {
            #region properties 
            public event EventHandler CanExecuteChanged;

            private Predicate<T> CanExecutePredicate { get; }
            #endregion

            #region constructors
            protected DelegateCommand(Predicate<T> canExecute, INotifyPropertyChanged notifier)
            {
                CanExecutePredicate = canExecute;
                if (notifier == null) return;
                var currentContext = SynchronizationContext.Current;
                notifier.PropertyChanged +=
                    (sender, args) =>
                        currentContext.Post(state => ((DelegateCommand<T>)state).OnCanExecuteChanged(), this);
            }
            #endregion

            #region methods

            public bool CanExecute(object parameter) => CanExecutePredicate?.Invoke((T)parameter) ?? false;

            public abstract void Execute(object parameter);

            protected void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

            #endregion
        }

        private class VoidDelegateCommand : DelegateCommand<object>
        {
            private Action ExecuteAction { get; }

            public VoidDelegateCommand(Action execute, Predicate<object> canExecute = null, INotifyPropertyChanged notifier = null)
                : base(canExecute ?? (o => true), notifier)
            {
                ExecuteAction = execute;
            }

            public override void Execute(object parameter)
            {
                if (!CanExecute(parameter)) return;
                ExecuteAction?.Invoke();
                OnCanExecuteChanged();
            }
        }

        private class ParameterisedDelegateCommand<T> : DelegateCommand<T>
        {
            private Action<T> ExecuteAction { get; }

            public ParameterisedDelegateCommand(Action<T> execute, Predicate<T> canExecute, INotifyPropertyChanged notifier = null)
                : base(canExecute, notifier)
            {
                ExecuteAction = execute;
            }

            public override void Execute(object parameter)
            {
                if (!CanExecute(parameter)) return;
                ExecuteAction?.Invoke((T)parameter);
                OnCanExecuteChanged();
            }
        }
        #endregion
    }
}