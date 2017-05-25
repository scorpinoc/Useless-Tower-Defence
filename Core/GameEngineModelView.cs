using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using Core.GameCells;
using static Core.DelegateCommand;
using Timer = System.Timers.Timer;

namespace Core
{

    public sealed class GameEngineModelView : IGameStateView
    {
        #region fields

        private GameStateOwner _stateOwner;
        private ITower _currentTowerType;

        #endregion

        #region properties

        public event PropertyChangedEventHandler PropertyChanged;

        #region auto

        private object Locker { get; }
        private Timer Timer { get; }
        private GameStateSaver GameStateSaver { get; }

        public IEnumerable<ITower> TowerTypes { get; }

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

        public ITower CurrentTowerType
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
        public int EnemiesHealth => GameState.EnemiesHealth;
        public int CurrentTurn => GameState.CurrentTurn;
        public int Lives => GameState.Lives;

        #endregion

        #endregion

        #region constructors

        public GameEngineModelView()
        {
            Locker = new object();
            Timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
            Timer.Elapsed += (sender, args) =>
            {
                if (CanNextTurn())
                    NextTurn();
                else
                {
                    Timer.Stop();
                    GameState.CurrentTurn = 0; // todo check and rework
                }
            };
            GameStateSaver = new GameStateSaver();
            // todo check for skip TowerType.Empty
            TowerTypes =
                Enum.GetValues(typeof(TowerType))
                    .Cast<TowerType>()
                    .Skip(1)
                    .Select(type => type.CreateTower());

            NewGame();

            #region Commands

            BuildTowerCommand = CreateCommand<GameCell>(BuildTowerIn, CanBuildTowerIn, this);
            NextLevelCommand = CreateCommand(NextLevel, () => GameState.CanNextLevel(), this);
            SetTowerTypeCommand = CreateCommand<ITower>(SetTowerTypeTo, CanSetTowerTypeTo, this);

            // menu commands
            NewGameCommand = CreateCommand(NewGame);
            UndoTurnCommand = CreateCommand<int>(UndoGameFor, CanUndoGameFor, this);

            const string file = "test.xml";
            // todo : auto start from pause if enemies left ?
            // todo : select file 
            SaveToFileCommand = CreateCommand(() => SaveGameTo(file), () => EnemiesLeft == 0, this);
            LoadFromFileCommand = CreateCommand(() => LoadGameFrom(file));

            #endregion
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

            StateOwner = new GameStateOwner(GameStateGenerator.GenerateGameState());
            // ReSharper disable once ExplicitCallerInfoArgument
            GameState.PropertyChanged += (o, eventArgs) => OnPropertyChanged(eventArgs.PropertyName);
        }

        private void NextTurn()
        {
            lock (Locker)
            {
                if (GameState.EnemiesLeft == 0) return;
                if (++GameState.CurrentTurn <= 10) return;
                GameState.Attack(GameState.EnemiesHealth); // todo rework
                --GameState.Lives;
            }
        }

        private bool CanNextTurn() => GameState.EnemiesLeft > 0 && GameState.Lives > 0;

        // todo rework to delete TowerCell
        private void BuildTowerIn(GameCell cell)
            => GameState.BuildTowerIn(cell as TowerCell, (ITower)CurrentTowerType.Clone(), CurrentTowerType.Cost);

        // todo rework to delete TowerCell
        private bool CanBuildTowerIn(GameCell cell)
            =>
                CanSetTowerTypeTo(CurrentTowerType) &&
                GameState.CanBuildTowerIn(cell as TowerCell, CurrentTowerType, CurrentTowerType.Cost);

        private void NextLevel()
        {
            StateOwner.Save();
            GameState.NextLevel();
            Timer.Start();
        }

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

        private void SetTowerTypeTo(ITower towerType) => CurrentTowerType = towerType;

        private bool CanSetTowerTypeTo(ITower towerType) => towerType != null && towerType.Cost <= Gold;

        #endregion

        #endregion

        private static class GameStateGenerator
        {
            public static GameState GenerateGameState()
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
                        return GameState_SimpleLineRoad();
                }
            }

            private static GameState GameState_SimpleLineRoad()
            {
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
    }
}