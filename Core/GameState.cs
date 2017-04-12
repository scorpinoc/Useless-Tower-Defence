using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Core
{
    public sealed class GameState : INotifyPropertyChanged, ICloneable
    {
        #region fields

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

        public ObservableCollection<GameCell> Cells { get; }

        #endregion

        #region delegate

        public int Gold
        {
            get { return _gold; }
            set
            {
                _gold = value;
                OnPropertyChanged();
            }
        }

        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                OnPropertyChanged();
            }
        }

        public int EnemiesLeft
        {
            get { return _enemiesLeft; }
            set
            {
                if (value < _enemiesLeft)
                {
                    if (value < 0) value = 0;
                    Gold += (_enemiesLeft - value) * Level;
                    _enemiesLeft = value;
                }
                else
                    _enemiesLeft = value;
                OnPropertyChanged();
            }
        }

        public int CurrentTurn
        {
            get { return _currentTurn; }
            set
            {
                _currentTurn = value;
                OnPropertyChanged();
            }
        }

        public int Lives
        {
            get { return _lives; }
            set
            {
                _lives = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #endregion

        #region constructors

        public GameState(ObservableCollection<GameCell> cells, Size gridSize)
        {
            Cells = cells;
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

        public object Clone()
            =>
                new GameState(new ObservableCollection<GameCell>(Cells.Select(cell => (GameCell)cell.Clone())), GridSize)
                    .SetGoldTo(Gold)
                    .SetLevelTo(Level)
                    .SetEnemiesTo(EnemiesLeft)
                    .SetCurrentTurnTo(CurrentTurn)
                    .SetLivesTo(Lives);

        #endregion

        public struct Size
        {
            public int Height { get; set; }
            public int Width { get; set; }
        }
    }
}