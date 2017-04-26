namespace Core
{
    public class Tower : ITower
    {
        public string Name { get; }

        public int Power { get; }

        public Tower(string name, int power)
        {
            Name = name;
            Power = power;
        }
        
        public object Clone() => new Tower(Name, Power);
    }
}