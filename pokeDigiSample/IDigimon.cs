namespace pokeDigiSample
{
    public class IDigimon : ICreature
    {
        public int CreatureID { get; set; }
        public string Image { get; set; }
        public string Level { get; set; }
        public IDigimon() {
            APIPrefix = string.Empty;
            Name = string.Empty;
            Url = string.Empty;
            Image = string.Empty;
            Level = string.Empty;
        }
    }
}
