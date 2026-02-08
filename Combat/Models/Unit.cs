namespace Combat.Models
{
    public enum TargetType
    {
        Hostile,
        Friendly
    }

    public class Attack
    {
        public string Name { get; set; }
        public TargetType Target { get; set; }
    }

    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int CR { get; set; }
        public List<Attack> PrimaryAttacks { get; set; } = new();
        public List<Attack> BonusAttacks { get; set; } = new();
    }
}
