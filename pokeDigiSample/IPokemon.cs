namespace pokeDigiSample
{
    public class IPokemon : ICreature
    {
        public int CreatureId { get; set; }
        public string Image { get; set; }
        public List<string> Types { get; set; }
        public List<int> TypeIds { get; set; }
        public IPokemon()
        {
            APIPrefix = string.Empty;
            Name = string.Empty;
            Url = string.Empty;
            Image = string.Empty;
            Types = new List<string>();
            TypeIds = new List<int>();
        }
    }
}
