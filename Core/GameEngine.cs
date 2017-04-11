using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using static Core.GameCell;

namespace Core
{
    public sealed class GameEngine : INotifyPropertyChanged
    {
        #region fields

        private readonly object _locker;

        private readonly Timer _timer;

        private GameStateOwner _stateOwner;

        private readonly GameStateSaver _gameStateSaver;

        #endregion

        #region properties

        public event PropertyChangedEventHandler PropertyChanged;

        #region delegate

        private GameState GameState => StateOwner.CurrentGameState;

        public GameState.Size GridSize => GameState.GridSize;

        public ObservableCollection<GameCell> Cells => GameState.Cells;

        public int Gold => GameState.Gold;

        public int Level => GameState.Level;

        public int EnemiesLeft => GameState.EnemiesLeft;

        public int CurrentTurn => GameState.CurrentTurn;

        public int Lives => GameState.Lives;

        public int SavesCount => StateOwner.SavesCount;

        private GameStateOwner StateOwner
        {
            get { return _stateOwner; }
            set
            {
                _stateOwner = value;
                _stateOwner.PropertyChanged += (sender, args) =>
                {
                    OnPropertyChanged(null);
                    GameState.PropertyChanged += (o, eventArgs) => OnPropertyChanged(eventArgs.PropertyName);
                };
                OnPropertyChanged(null);
            }
        }

        #endregion

        #endregion

        #region constructors

        public GameEngine()
        {
            _locker = new object();
            _gameStateSaver = new GameStateSaver();

            _timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
            _timer.Elapsed += (sender, args) => { if (!NextTurn()) _timer.Stop(); };

            #region another timer

            //_timer = new System.Threading.Timer(state =>
            //{
            //    var engine = (GameEngine)state;
            //    var towers = engine.Cells.Count(cell => cell.State == CellState.Tower);
            //    engine.EnemiesLeft -= towers + towers;

            //}, this, Timeout.Infinite, Timeout.Infinite);

            #endregion

            NewGame();
        }

        #endregion

        #region methods

        #region event handlers

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region pure methods

        public void NewGame()
        {
            _timer.Stop();

            GameState gameState;
            if (new Random().Next(2) == 0)
                gameState = new GameState(
                    new ObservableCollection<GameCell>
                    {
                        new GameCell(CellState.Empty), new GameCell(CellState.Empty), new GameCell(CellState.Road),  new GameCell(CellState.Empty),
                        new GameCell(CellState.Empty), new GameCell(CellState.Road),  new GameCell(CellState.Road),  new GameCell(CellState.Empty),
                        new GameCell(CellState.Empty), new GameCell(CellState.Road),  new GameCell(CellState.Empty), new GameCell(CellState.Empty),
                        new GameCell(CellState.Empty), new GameCell(CellState.Road),  new GameCell(CellState.Road),  new GameCell(CellState.Empty),
                        new GameCell(CellState.Empty), new GameCell(CellState.Empty), new GameCell(CellState.Road),  new GameCell(CellState.Empty)
                    },
                    new GameState.Size { Height = 5, Width = 4 });
            else
                gameState = new GameState(
                    new ObservableCollection<GameCell>
                    {
                       new GameCell(CellState.Empty), new GameCell(CellState.Road),  new GameCell(CellState.Empty),
                       new GameCell(CellState.Empty), new GameCell(CellState.Road),  new GameCell(CellState.Empty),
                       new GameCell(CellState.Empty), new GameCell(CellState.Road),  new GameCell(CellState.Empty),
                       new GameCell(CellState.Empty), new GameCell(CellState.Road),  new GameCell(CellState.Empty),
                       new GameCell(CellState.Empty), new GameCell(CellState.Road),  new GameCell(CellState.Empty)
                    },
                    new GameState.Size { Height = 5, Width = 3 });

            StateOwner = new GameStateOwner(gameState);

            GameState.PropertyChanged += (o, eventArgs) => OnPropertyChanged(eventArgs.PropertyName);

            OnPropertyChanged(null);
        }

        public void UndoGameFor(int levels = 1)
        {
            if (levels < 1 || levels > SavesCount) return;
            for (; levels > 0; --levels)
                StateOwner.Load();
        }

        public void BuildTower(GameCell cell)
        {
            if (cell.State != CellState.Empty || Gold < 80) return;
            cell.State = CellState.Tower;
            GameState.Gold -= 80;
        }

        public void NextLevel()
        {
            StateOwner.Save();
            GameState.EnemiesLeft = ++GameState.Level * GameState.Cells.Count(cell => cell.State == CellState.Road);
            _timer.Start();
        }

        private bool NextTurn()
        {
            // TODO : 0 == 0 == FALSE ?
            lock (_locker)
            {
                var towers = GameState.Cells.Count(cell => cell.State == CellState.Tower);
                GameState.EnemiesLeft -= towers + towers;
                if (GameState.EnemiesLeft != 0)
                {
                    if (++GameState.CurrentTurn <= 10) return true;
                    --GameState.EnemiesLeft;
                    --GameState.Lives;
                    if (GameState.EnemiesLeft > 0 && GameState.Lives > 0)
                        return true;
                }
                GameState.CurrentTurn = 0;
                return false;
            }
        }

        public void SaveGameTo(string file) => _gameStateSaver.SaveTo(file, StateOwner.GameStates);

        public void LoadGameFrom(string file)
        {
            _timer.Stop();
            StateOwner = new GameStateOwner(_gameStateSaver.LoadFrom(file));
        }

        #endregion

        #endregion
    }
}