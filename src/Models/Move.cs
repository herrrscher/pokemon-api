namespace poke_db.Models
{
    public class PokemonMove
    {
        public required string MoveName { get; set; }
        public required string LearnMethod { get; set; }
        public int? LevelLearnedAt { get; set; }
    }
}
