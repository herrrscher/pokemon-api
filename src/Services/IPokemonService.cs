using poke_db.Models;

namespace poke_db.Services
{
    public interface IPokemonService
    {
        Task<List<Pokemon>> GetAllPokemonAsync(int limit = 20, int offset = 0);
        Task<Pokemon?> GetPokemonByIdAsync(int id);
        Task<Pokemon?> GetPokemonByNameAsync(string name);
        Task<List<Pokemon>> GetPokemonByTypeAsync(string type);
        Task<List<Pokemon>> GetPokemonByAbilityAsync(string ability);
        Task<List<Pokemon>> GetPokemonByMoveAsync(string move);
        Task<List<Pokemon>> GetPokemonByStatRangeAsync(string stat, int? minValue, int? maxValue);
        Task<Pokemon> CreatePokemonAsync(Pokemon pokemon);
        Task<Pokemon> UpdatePokemonAsync(int id, Pokemon pokemon);
        Task<bool> DeletePokemonAsync(int id);
        Task<Pokemon?> FetchAndSavePokemonFromPokeApiAsync(int id);
        Task<Pokemon?> FetchAndSavePokemonFromPokeApiAsync(string name);
    }
}
