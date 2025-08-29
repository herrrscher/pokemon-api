using Firebase.Database;
using Firebase.Database.Query;
using poke_db.Models;
using System.Text.Json;

namespace poke_db.Services
{
    public class RealtimePokemonService : IPokemonService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly string _databaseUrl;
        private readonly HttpClient _httpClient;

        public RealtimePokemonService(string databaseUrl, string? authSecret = null)
        {
            _databaseUrl = databaseUrl;
            _httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(authSecret))
            {
                _firebaseClient = new FirebaseClient(
                    _databaseUrl,
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(authSecret)
                    });
            }
            else
            {
                _firebaseClient = new FirebaseClient(_databaseUrl);
            }
        }

        public async Task<List<Pokemon>> GetAllPokemonAsync(int limit = 20, int offset = 0)
        {
            var pokemon = await _firebaseClient
                .Child("pokemon")
                .OnceSingleAsync<List<Pokemon>>();

            return pokemon?
                .Skip(offset)
                .Take(limit)
                .ToList() ?? new List<Pokemon>();
        }

        public async Task<Pokemon?> GetPokemonByIdAsync(int id)
        {
            try
            {
                var allPokemon = await _firebaseClient
                    .Child("pokemon")
                    .OnceSingleAsync<List<Pokemon>>();

                if (allPokemon?.Any() == true)
                {
                    var found = allPokemon.FirstOrDefault(p => p?.Id == id);
                    return found;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Pokemon?> GetPokemonByNameAsync(string name)
        {
            var allPokemon = await _firebaseClient
                .Child("pokemon")
                .OnceSingleAsync<List<Pokemon>>();

            return allPokemon?
                .FirstOrDefault(p => p?.Name == name);
        }

        public async Task<Pokemon> CreatePokemonAsync(Pokemon pokemon)
        {
            if (pokemon == null)
                throw new ArgumentNullException(nameof(pokemon));

            var existingPokemon = await _firebaseClient
                .Child("pokemon")
                .OnceSingleAsync<List<Pokemon>>() ?? new List<Pokemon>();

            var existingIndex = existingPokemon.FindIndex(p => p?.Id == pokemon.Id);
            if (existingIndex >= 0)
            {
                throw new InvalidOperationException($"Pokemon with ID {pokemon.Id} already exists");
            }

            existingPokemon.Add(pokemon);

            await _firebaseClient
                .Child("pokemon")
                .PutAsync(existingPokemon);

            return pokemon;
        }

        public async Task<Pokemon> UpdatePokemonAsync(int id, Pokemon pokemon)
        {
            if (pokemon == null)
                throw new ArgumentNullException(nameof(pokemon));

            var existingPokemon = await _firebaseClient
                .Child("pokemon")
                .OnceSingleAsync<List<Pokemon>>() ?? new List<Pokemon>();

            var index = existingPokemon.FindIndex(p => p?.Id == id);
            if (index >= 0)
            {
                existingPokemon[index] = pokemon;
                pokemon.Id = id;
            }
            else
            {
                throw new ArgumentException($"Pokemon with ID {id} not found");
            }

            await _firebaseClient
                .Child("pokemon")
                .PutAsync(existingPokemon);

            return pokemon;
        }

        public async Task<bool> DeletePokemonAsync(int id)
        {
            try
            {
                var existingPokemon = await _firebaseClient
                    .Child("pokemon")
                    .OnceSingleAsync<List<Pokemon>>() ?? new List<Pokemon>();

                var index = existingPokemon.FindIndex(p => p?.Id == id);

                if (index >= 0)
                {
                    existingPokemon.RemoveAt(index);

                    await _firebaseClient
                        .Child("pokemon")
                        .PutAsync(existingPokemon);

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Pokemon>> GetPokemonByTypeAsync(string type)
        {
            var allPokemon = await _firebaseClient
                .Child("pokemon")
                .OnceSingleAsync<List<Pokemon>>();

            return allPokemon?
                .Where(p => p?.Types?.Contains(type, StringComparer.OrdinalIgnoreCase) == true)
                .ToList() ?? new List<Pokemon>();
        }

        public async Task<List<Pokemon>> GetPokemonByAbilityAsync(string ability)
        {
            var allPokemon = await _firebaseClient
                .Child("pokemon")
                .OnceSingleAsync<List<Pokemon>>();

            return allPokemon?
                .Where(p => p?.Abilities?.Any(a =>
                    string.Equals(a.Name, ability, StringComparison.OrdinalIgnoreCase)) == true)
                .ToList() ?? new List<Pokemon>();
        }

        public async Task<List<Pokemon>> GetPokemonByMoveAsync(string move)
        {
            var allPokemon = await _firebaseClient
                .Child("pokemon")
                .OnceSingleAsync<List<Pokemon>>();

            return allPokemon?
                .Where(p => p?.Moves?.Any(m =>
                    string.Equals(m.MoveName, move, StringComparison.OrdinalIgnoreCase)) == true)
                .ToList() ?? new List<Pokemon>();
        }

        public async Task<List<Pokemon>> GetPokemonByStatRangeAsync(string stat, int? minValue, int? maxValue)
        {
            var allPokemon = await _firebaseClient
                .Child("pokemon")
                .OnceSingleAsync<List<Pokemon>>();

            return allPokemon?
                .Where(p => p?.Stats?.ContainsKey(stat) == true)
                .Where(p =>
                {
                    var statValue = p.Stats[stat];
                    return (minValue == null || statValue >= minValue) &&
                            (maxValue == null || statValue <= maxValue);
                })
                .ToList() ?? new List<Pokemon>();
        }

        public async Task<Pokemon?> FetchAndSavePokemonFromPokeApiAsync(int id)
        {
            try
            {
                var existingPokemon = await GetPokemonByIdAsync(id);
                if (existingPokemon != null)
                {
                    return existingPokemon;
                }

                var response = await _httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var pokeApiPokemon = JsonSerializer.Deserialize<PokeApiPokemon>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (pokeApiPokemon == null)
                {
                    return null;
                }

                var pokemon = ConvertFromPokeApi(pokeApiPokemon);

                await CreatePokemonAsync(pokemon);

                return pokemon;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Pokemon?> FetchAndSavePokemonFromPokeApiAsync(string name)
        {
            try
            {
                var existingPokemon = await GetPokemonByNameAsync(name);
                if (existingPokemon != null)
                {
                    return existingPokemon;
                }

                var response = await _httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon/{name.ToLower()}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var pokeApiPokemon = JsonSerializer.Deserialize<PokeApiPokemon>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (pokeApiPokemon == null)
                {
                    return null;
                }

                var pokemon = ConvertFromPokeApi(pokeApiPokemon);

                await CreatePokemonAsync(pokemon);

                return pokemon;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Pokemon ConvertFromPokeApi(PokeApiPokemon pokeApiPokemon)
        {
            return new Pokemon
            {
                Id = pokeApiPokemon.Id,
                Name = pokeApiPokemon.Name,
                Height = pokeApiPokemon.Height,
                Weight = pokeApiPokemon.Weight,
                BaseExperience = pokeApiPokemon.BaseExperience,
                Order = pokeApiPokemon.Order,
                IsDefault = pokeApiPokemon.IsDefault,
                Types = pokeApiPokemon.Types?.Select(t => t.Type?.Name ?? "unknown").ToList() ?? new List<string>(),
                Abilities = pokeApiPokemon.Abilities?.Select(a => new PokemonAbility
                {
                    Name = a.Ability?.Name ?? "unknown",
                    IsHidden = a.IsHidden,
                    Slot = a.Slot
                }).ToList() ?? new List<PokemonAbility>(),
                Moves = pokeApiPokemon.Moves?.Take(10).Select(m => new PokemonMove
                {
                    MoveName = m.Move?.Name ?? "unknown",
                    LearnMethod = m.VersionGroupDetails?.FirstOrDefault()?.MoveLearnMethod?.Name ?? "unknown",
                    LevelLearnedAt = m.VersionGroupDetails?.FirstOrDefault()?.LevelLearnedAt ?? 0
                }).ToList() ?? new List<PokemonMove>(),
                Sprites = new PokemonSprites
                {
                    FrontDefault = pokeApiPokemon.Sprites?.FrontDefault ?? "",
                    FrontShiny = pokeApiPokemon.Sprites?.FrontShiny ?? "",
                    BackDefault = pokeApiPokemon.Sprites?.BackDefault ?? "",
                    BackShiny = pokeApiPokemon.Sprites?.BackShiny ?? ""
                },
                Stats = new Dictionary<string, int>
                {
                    ["hp"] = pokeApiPokemon.Stats?.FirstOrDefault(s => s.Stat?.Name == "hp")?.BaseStat ?? 0,
                    ["attack"] = pokeApiPokemon.Stats?.FirstOrDefault(s => s.Stat?.Name == "attack")?.BaseStat ?? 0,
                    ["defense"] = pokeApiPokemon.Stats?.FirstOrDefault(s => s.Stat?.Name == "defense")?.BaseStat ?? 0,
                    ["special-attack"] = pokeApiPokemon.Stats?.FirstOrDefault(s => s.Stat?.Name == "special-attack")?.BaseStat ?? 0,
                    ["special-defense"] = pokeApiPokemon.Stats?.FirstOrDefault(s => s.Stat?.Name == "special-defense")?.BaseStat ?? 0,
                    ["speed"] = pokeApiPokemon.Stats?.FirstOrDefault(s => s.Stat?.Name == "speed")?.BaseStat ?? 0
                },
                GameIndices = pokeApiPokemon.GameIndices?.Select(gi => new PokemonGameIndex
                {
                    GameIndex = gi.GameIndex,
                    Version = gi.Version?.Name ?? "unknown"
                }).ToList() ?? new List<PokemonGameIndex>(),
                HeldItems = new List<PokemonHeldItem>(),
                SpeciesId = pokeApiPokemon.Species?.Url?.Split('/').Where(s => !string.IsNullOrEmpty(s)).LastOrDefault() != null
                    ? int.TryParse(pokeApiPokemon.Species.Url.Split('/').Where(s => !string.IsNullOrEmpty(s)).Last(), out var speciesId) ? speciesId : pokeApiPokemon.Id
                    : pokeApiPokemon.Id,
                SpeciesName = pokeApiPokemon.Species?.Name ?? pokeApiPokemon.Name
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class PokeApiPokemon
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Height { get; set; }
        public int Weight { get; set; }
        public int BaseExperience { get; set; }
        public int Order { get; set; }
        public bool IsDefault { get; set; }
        public List<PokeApiType>? Types { get; set; }
        public List<PokeApiAbility>? Abilities { get; set; }
        public List<PokeApiMove>? Moves { get; set; }
        public PokeApiSprites? Sprites { get; set; }
        public List<PokeApiStat>? Stats { get; set; }
        public List<PokeApiGameIndex>? GameIndices { get; set; }
        public PokeApiSpecies? Species { get; set; }
    }

    public class PokeApiType
    {
        public int Slot { get; set; }
        public PokeApiNamedResource? Type { get; set; }
    }

    public class PokeApiAbility
    {
        public bool IsHidden { get; set; }
        public int Slot { get; set; }
        public PokeApiNamedResource? Ability { get; set; }
    }

    public class PokeApiMove
    {
        public PokeApiNamedResource? Move { get; set; }
        public List<PokeApiVersionGroupDetail>? VersionGroupDetails { get; set; }
    }

    public class PokeApiVersionGroupDetail
    {
        public int LevelLearnedAt { get; set; }
        public PokeApiNamedResource? MoveLearnMethod { get; set; }
        public PokeApiNamedResource? VersionGroup { get; set; }
    }

    public class PokeApiSprites
    {
        public string? FrontDefault { get; set; }
        public string? FrontShiny { get; set; }
        public string? BackDefault { get; set; }
        public string? BackShiny { get; set; }
    }

    public class PokeApiStat
    {
        public int BaseStat { get; set; }
        public int Effort { get; set; }
        public PokeApiNamedResource? Stat { get; set; }
    }

    public class PokeApiGameIndex
    {
        public int GameIndex { get; set; }
        public PokeApiNamedResource? Version { get; set; }
    }

    public class PokeApiSpecies
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class PokeApiNamedResource
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}