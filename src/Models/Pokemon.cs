namespace poke_db.Models
{
    public class Pokemon
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public int BaseExperience { get; set; }
        public int Order { get; set; }
        public bool IsDefault { get; set; }
        public List<string> Types { get; set; } = new List<string>();
        public List<PokemonAbility> Abilities { get; set; } = new List<PokemonAbility>();
        public List<PokemonMove> Moves { get; set; } = new List<PokemonMove>();
        public required PokemonSprites Sprites { get; set; }
        public Dictionary<string, int> Stats { get; set; } = new Dictionary<string, int>();
        public List<PokemonGameIndex> GameIndices { get; set; } = new List<PokemonGameIndex>();
        public List<PokemonHeldItem> HeldItems { get; set; } = new List<PokemonHeldItem>();
        public int SpeciesId { get; set; }
        public string? SpeciesName { get; set; }
    }

    public class PokemonGameIndex
    {
        public int GameIndex { get; set; }
        public required string Version { get; set; }
    }

    public class PokemonHeldItem
    {
        public required string Item { get; set; }
        public List<PokemonHeldItemVersion> VersionDetails { get; set; } = new List<PokemonHeldItemVersion>();
    }

    public class PokemonHeldItemVersion
    {
        public required string Version { get; set; }
        public int Rarity { get; set; }
    }

    public class PokemonAbility
    {
        public required string Name { get; set; }
        public bool IsHidden { get; set; }
        public int Slot { get; set; }
    }

    public class PokemonSprites
    {
        public required string FrontDefault { get; set; }
        public required string FrontShiny { get; set; }
        public required string BackDefault { get; set; }
        public required string BackShiny { get; set; }
    }
}
