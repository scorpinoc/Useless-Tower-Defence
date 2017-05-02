using System;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Core.GameCells
{
    public class Tower : ITower
    {
        private GameState _owner;

        private Task AttackWaiter { get; set; }

        public string Name { get; }

        public int AttackPower { get; }

        public TimeSpan AttackSpeed { get; }

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

        public Tower(string name, int attackPower, TimeSpan attackSpeed)
        {
            Name = name;
            AttackPower = attackPower;
            AttackSpeed = attackSpeed;
        }
        
        private async void Attack()
        {
            while (Owner.EnemiesLeft > 0 && AttackWaiter == null)
            {
                await (AttackWaiter = Task.Run(() => Thread.Sleep(AttackSpeed)));
                Owner.Attack(AttackPower);
                AttackWaiter = null;
            }
        }

        public object Clone() => new Tower(Name, AttackPower, AttackSpeed);
    }
}
// todo 58