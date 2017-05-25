using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.GameCells
{
    public class Tower : ITower
    {
        private GameState _owner;
        
        public string Name { get; }

        public int AttackPower { get; }

        public TimeSpan AttackSpeed { get; }

        public int Cost { get; }

        private SemaphoreSlim SemaphoreSlim { get; } 

        public GameState Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                if (_owner == null) return;
                _owner.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(_owner.EnemiesLeft))
                        Attack();
                };
            }
        }

        public Tower(string name, int attackPower, TimeSpan attackSpeed, int cost)
        {
            Name = name;
            AttackPower = attackPower;
            AttackSpeed = attackSpeed;
            Cost = cost;
            SemaphoreSlim = new SemaphoreSlim(1,1);
        }

        private async void Attack()
        {
            await SemaphoreSlim.WaitAsync();
            while (Owner.CanAttack())
            {
                Owner.Attack(AttackPower);
                await Task.Delay(AttackSpeed);
            }
            SemaphoreSlim.Release();
        }

        public object Clone() => new Tower(Name, AttackPower, AttackSpeed, Cost);
    }
}