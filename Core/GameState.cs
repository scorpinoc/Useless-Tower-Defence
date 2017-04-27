using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Core.GameCells;

namespace Core
{
    public sealed class GameState : INotifyPropertyChanged, ICloneable
    {
        #region fields

        private int _score;
        private int _gold;
        private int _level;
        private int _enemiesLeft;
        private int _currentTurn;
        private int _lives;

        #endregion

        #region properties

        public event PropertyChangedEventHandler PropertyChanged;

        #region auto

        public Size GridSize { get; }

        public IEnumerable<GameCell> Cells { get; }

        #endregion

        #region delegate

        public int Score
        {
            get { return _score; }
            private set
            {
                if (value < 0) // todo change to uint
                    throw new ArgumentOutOfRangeException(nameof(value));
                _score = value;
                OnPropertyChanged();
            }
        }

        public int Gold
        {
            get { return _gold; }
            private set
            {
                if (value < 0) // todo change to uint
                    throw new ArgumentOutOfRangeException(nameof(value));
                _gold = value;
                OnPropertyChanged();
            }
        }

        public int Level
        {
            get { return _level; }
            set // todo to private
            {
                if (value < 0)  // todo change to uint
                    throw new ArgumentOutOfRangeException(nameof(value));
                _level = value;
                OnPropertyChanged();
            }
        }

        public int EnemiesLeft
        {
            get { return _enemiesLeft; }
            set// todo to private
            {
                // todo extract method
                if (value < _enemiesLeft)
                {
                    if (value < 0) value = 0;
                    var killed = _enemiesLeft - value;
                    Gold += killed * Level;

                    _enemiesLeft = value;

                    Score += Math.Max(killed * Level + Gold / 100 - Math.Max(_enemiesLeft * CurrentTurn / 2, 0), killed);
                }
                else
                    _enemiesLeft = value;
                OnPropertyChanged();
            }
        }

        public int CurrentTurn
        {
            get { return _currentTurn; }
            set // todo to private
            {
                _currentTurn = value;
                OnPropertyChanged();
            }
        }

        public int Lives
        {
            get { return _lives; }
            set // todo to private
            {
                _lives = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #endregion

        #region constructors

        public GameState(IEnumerable<GameCell> cells, Size gridSize)
        {
            if (cells == null) throw new ArgumentNullException(nameof(cells));
            Cells = cells as List<GameCell> ?? cells.ToList();
            GridSize = gridSize;
            Gold = 100;
            Lives = 10;
        }

        #endregion

        #region event invokers

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region pure methods

        public void BuildTowerIn(TowerCell towerCell, ITower tower, int cost)
        {
            if (!CanBuildTowerIn(towerCell, tower, cost)) return;
            towerCell.Build(tower.Clone() as ITower);
            Gold -= cost;
        }

        public bool CanBuildTowerIn(TowerCell towerCell, ITower tower, int cost)
            => Cells.Contains(towerCell) && (towerCell?.Buildable ?? false) && cost <= Gold;

        public GameState SetScoreTo(int score)
        {
            Score = score;
            return this;
        }

        public GameState SetGoldTo(int gold)
        {
            Gold = gold;
            return this;
        }

        public GameState SetLevelTo(int level)
        {
            Level = level;
            return this;
        }

        public GameState SetEnemiesTo(int enemiesCount)
        {
            EnemiesLeft = enemiesCount;
            return this;
        }

        public GameState SetCurrentTurnTo(int turn)
        {
            CurrentTurn = turn;
            return this;
        }

        public GameState SetLivesTo(int lives)
        {
            Lives = lives;
            return this;
        }

        #endregion

        public object Clone()
            =>
                new GameState(new ObservableCollection<GameCell>(Cells.Select(cell => (GameCell)cell.Clone())), GridSize)
                    .SetScoreTo(Score)
                    .SetGoldTo(Gold)
                    .SetLevelTo(Level)
                    .SetEnemiesTo(EnemiesLeft)
                    .SetCurrentTurnTo(CurrentTurn)
                    .SetLivesTo(Lives);


        public struct Size
        {
            public int Height { get; set; }
            public int Width { get; set; }
        }
    }
}