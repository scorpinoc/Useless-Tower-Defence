using System;
using Timer = System.Timers.Timer;

namespace Core.GameCells
{
    public class Tower : ITower
    {
        private GameState _owner;

        private Timer Timer { get; }

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
                InitializeAttack();
            }
        }
        
        public Tower(string name, int attackPower, TimeSpan attackSpeed)
        {
            Name = name;
            AttackPower = attackPower;
            AttackSpeed = attackSpeed;
            Timer = new Timer(attackSpeed.TotalMilliseconds);
        }

        private void InitializeAttack()
        {
            // todo rework from timer to sleep-reset
            Owner.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Owner.EnemiesLeft))
                    Timer.Start();
            };
            // todo lock thread (or in GameState)
            Timer.Elapsed += (sender, args) =>
            {
                if (Owner.EnemiesLeft > 0)
                    Owner.EnemiesLeft -= AttackPower;
                else
                    Timer.Stop();
            };
        }

        public object Clone() => new Tower(Name, AttackPower, AttackSpeed);
    }
}