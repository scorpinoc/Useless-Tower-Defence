using System.Collections.Generic;
using System.ComponentModel;
using Core.GameCells;

namespace Core
{
    public interface IGameStateView : INotifyPropertyChanged
    {
        GameState.Size GridSize { get; }
        IEnumerable<GameCell> Cells { get; }
        int Score { get; }
        int Gold { get; }
        int Level { get; }
        int EnemiesLeft { get; }
        int EnemiesHealth { get; }

        int CurrentTurn { get; }

        int Lives { get; }
    }
}