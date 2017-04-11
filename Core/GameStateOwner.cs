using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Core
{
    public class GameStateOwner : INotifyPropertyChanged
    {
        private readonly Stack<GameState> _states;

        public event PropertyChangedEventHandler PropertyChanged;

        public GameState CurrentGameState => _states.Peek();

        public int SavesCount => _states.Count - 1;

        public IEnumerable<GameState> GameStates => _states.ToArray();

        public GameStateOwner(GameState initialState)
        {
            _states = new Stack<GameState>();
            _states.Push(initialState);
            Save();
        }

        public GameStateOwner(IEnumerable<GameState> gameStates)
        {
            _states = new Stack<GameState>(gameStates.Reverse());
            Save();
        }

        public void Save()
        {
            _states.Push((GameState)_states.Peek().Clone());
            OnPropertyChanged(null);
        }

        public void Load()
        {
            if (_states.Count > 1)
                _states.Pop();
            OnPropertyChanged(null);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}