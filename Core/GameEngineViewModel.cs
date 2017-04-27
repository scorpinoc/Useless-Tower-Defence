using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;

using Core.GameCells;
using static Core.TowerFactory;
using Timer = System.Timers.Timer;

namespace Core
{
    public sealed class GameEngineViewModel : INotifyPropertyChanged
    {
        #region static

        private static GameState GenerateGameState()
        {
            switch (new Random().Next(3))
            {
                case 0:
                    return new GameState(
                         new GameCell[]
                         {
                            new TowerCell(), new TowerCell(), new RoadCell(),  new TowerCell(),
                            new TowerCell(), new RoadCell(),  new RoadCell(),  new TowerCell(),
                            new TowerCell(), new RoadCell(),  new TowerCell(), new TowerCell(),
                            new TowerCell(), new RoadCell(),  new RoadCell(),  new TowerCell(),
                            new TowerCell(), new TowerCell(), new RoadCell(),  new TowerCell()
                         },
                         new GameState.Size { Height = 5, Width = 4 });
                case 1:
                    return new GameState(
                         new GameCell[]
                         {
                            new RoadCell(),  new RoadCell(), new TowerCell(), new TowerCell(),
                            new TowerCell(), new RoadCell(), new TowerCell(), new TowerCell(),
                            new TowerCell(), new RoadCell(), new TowerCell(), new TowerCell(),
                            new TowerCell(), new RoadCell(), new TowerCell(), new TowerCell(),
                            new TowerCell(), new RoadCell(), new RoadCell(),  new RoadCell()
                         },
                         new GameState.Size { Height = 5, Width = 4 });
                default:
                    return new GameState(
                        new GameCell[]
                        {
                            new TowerCell(), new RoadCell(), new TowerCell(),
                            new TowerCell(), new RoadCell(), new TowerCell(),
                            new TowerCell(), new RoadCell(), new TowerCell(),
                            new TowerCell(), new RoadCell(), new TowerCell(),
                            new TowerCell(), new RoadCell(), new TowerCell()
                        },
                        new GameState.Size { Height = 5, Width = 3 });
            }
        }

        #endregion

        #region fields

        private GameStateOwner _stateOwner;
        private TowerInfo _currentTowerType;

        #endregion

        #region properties

        public event PropertyChangedEventHandler PropertyChanged;

        #region auto

        private object Locker { get; }
        private Timer Timer { get; }
        private GameStateSaver GameStateSaver { get; }
        private TowerFactory TowerFactory { get; }

        public IEnumerable<TowerInfo> TowerTypes { get; }

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
                    // ReSharper disable once RedundantArgumentDefaultValue
                    OnPropertyChanged(null);
                    // ReSharper disable once ExplicitCallerInfoArgument
                    _stateOwner.CurrentGameState.PropertyChanged += (o, eventArgs) => OnPropertyChanged(eventArgs.PropertyName);
                };
                // ReSharper disable once ExplicitCallerInfoArgument
                // ReSharper disable once RedundantArgumentDefaultValue
                OnPropertyChanged(null);
                CurrentTowerType = null;
            }
        }

        private GameState GameState => StateOwner.CurrentGameState;

        public TowerInfo CurrentTowerType
        {
            get { return _currentTowerType; }
            private set
            {
                _currentTowerType = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<GameCell> Cells => GameState.Cells;
        public GameState.Size GridSize => GameState.GridSize;

        public int Score => GameState.Score;
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
                if (CanNextTurn())
                    NextTurn();
                else
                    Timer.Stop();
            };
            GameStateSaver = new GameStateSaver();
            TowerFactory = new TowerFactory();
            // todo check for skip TowerType.Empty
            TowerTypes =
                Enum.GetValues(typeof(TowerType))
                    .Cast<TowerType>()
                    .Skip(1)
                    .Select(type => TowerFactory.GeTowerInfoFor(type));

            #region Commands

            BuildTowerCommand = new DelegateCommand<GameCell>(BuildTowerIn, CanBuildTowerIn, this);
            NextLevelCommand = new DelegateCommand(NextLevel, o => CanNextLevel(), this);
            SetTowerTypeCommand = new DelegateCommand<TowerInfo>(SetTowerTypeTo, CanSetTowerTypeTo, this);

            // menu commands
            NewGameCommand = new DelegateCommand(NewGame);
            UndoTurnCommand = new DelegateCommand<int>(UndoGameFor, CanUndoGameFor, this);

            const string file = "test.xml";
            // todo : auto start from pause if enemies left ?
            // todo : select file 
            SaveToFileCommand = new DelegateCommand(() => SaveGameTo(file), o => EnemiesLeft == 0, this);
            LoadFromFileCommand = new DelegateCommand(() => LoadGameFrom(file));

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

            StateOwner = new GameStateOwner(GenerateGameState());
            // ReSharper disable once ExplicitCallerInfoArgument
            GameState.PropertyChanged += (o, eventArgs) => OnPropertyChanged(eventArgs.PropertyName);
        }

        private void NextTurn()
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

        private bool CanNextTurn() => GameState.EnemiesLeft > 0 && GameState.Lives > 0;
        
        // todo rework
        private void BuildTowerIn(GameCell cell)
            => GameState.BuildTowerIn(cell as TowerCell, CurrentTowerType.GetTower(), CurrentTowerType.Cost);

        private bool CanBuildTowerIn(GameCell cell)
            =>
                CanSetTowerTypeTo(CurrentTowerType) &&
                GameState.CanBuildTowerIn(cell as TowerCell, CurrentTowerType.GetTower(), CurrentTowerType.Cost);

        private void NextLevel()
        {
            StateOwner.Save();
            GameState.EnemiesLeft = ++GameState.Level * GameState.Cells.OfType<RoadCell>().Count();
            Timer.Start();
        }

        private bool CanNextLevel() => GameState.EnemiesLeft == 0 && GameState.Lives > 0;

        private void UndoGameFor(int turns = 1)
        {
            for (; turns > 0; --turns)
                StateOwner.Load();
        }

        private bool CanUndoGameFor(int turns = 1) => turns >= 1 && turns <= StateOwner.SavesCount;

        private void SaveGameTo(string file) => GameStateSaver.SaveTo(file, StateOwner.GameStates);

        private void LoadGameFrom(string file)
        {
            Timer.Stop();
            StateOwner = new GameStateOwner(GameStateSaver.LoadFrom(file));
        }

        private void SetTowerTypeTo(TowerInfo towerType) => CurrentTowerType = towerType;

        private bool CanSetTowerTypeTo(TowerInfo towerType) => towerType != null && towerType.Cost <= Gold;

        #endregion

        #endregion

        #region inner classes

        private abstract class DelegateCommandBase<T> : ICommand
        {
            #region properties

            public event EventHandler CanExecuteChanged;

            private Predicate<T> CanExecutePredicate { get; }
            private Delegate ExecuteAction { get; }

            #endregion

            #region constructors

            protected DelegateCommandBase(Delegate executeAction, Predicate<T> canExecutePredicate,
                INotifyPropertyChanged notifier)
            {
                if (executeAction == null)
                    throw new ArgumentNullException(nameof(executeAction));
                ExecuteAction = executeAction;

                CanExecutePredicate = canExecutePredicate ?? (obj => true);

                if (notifier == null) return;
                // MultiThread
                var currentContext = SynchronizationContext.Current;
                notifier.PropertyChanged +=
                    (sender, args) =>
                        currentContext.Post(state => ((DelegateCommandBase<T>)state).OnCanExecuteChanged(), this);
            }

            #endregion

            public bool CanExecute(object parameter)
                => CanExecutePredicate?.Invoke((T)parameter) ?? true;

            public void Execute(object parameter)
            {
                if (!CanExecute(parameter)) return;
                ExecuteAction?.DynamicInvoke(parameter == null ? null : new[] { parameter });
                OnCanExecuteChanged();
            }

            private void OnCanExecuteChanged()
                => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        private sealed class DelegateCommand : DelegateCommandBase<object>
        {
            public DelegateCommand(Action execute, Predicate<object> canExecute = null, INotifyPropertyChanged notifier = null)
                : base(execute, canExecute, notifier)
            { }
        }

        private sealed class DelegateCommand<T> : DelegateCommandBase<T>
        {
            public DelegateCommand(Action<T> execute, Predicate<T> canExecute = null, INotifyPropertyChanged notifier = null)
                : base(execute, canExecute, notifier)
            { }
        }

        #endregion
    }
}