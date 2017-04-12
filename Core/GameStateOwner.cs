using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Core
{
    public sealed class GameStateOwner : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Stack<GameState> States { get; }

        public GameState CurrentGameState => States.Peek();

        public int SavesCount => States.Count - 1;

        public IEnumerable<GameState> GameStates => States.ToArray();

        public GameStateOwner(GameState initialState)
        {
            States = new Stack<GameState>();
            States.Push(initialState);
            Save();
        }

        public GameStateOwner(IEnumerable<GameState> gameStates)
        {
            States = new Stack<GameState>(gameStates.Reverse());
            Save();
        }

        public void Save()
        {
            States.Push((GameState)States.Peek().Clone());
            OnPropertyChanged(null);
        }

        public void Load()
        {
            if (States.Count > 1)
                States.Pop();
            OnPropertyChanged(null);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}