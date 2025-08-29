
using Microsoft.AspNetCore.Mvc;
using poke_db.Models;
using poke_db.Services;

namespace poke_db.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PokemonController : ControllerBase
	{
		private readonly IPokemonService _pokemonService;
		private readonly ILogger<PokemonController> _logger;

		public PokemonController(IPokemonService pokemonService, ILogger<PokemonController> logger)
		{
			_pokemonService = pokemonService;
			_logger = logger;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Pokemon>>> GetAllPokemon([FromQuery] int limit = 20, [FromQuery] int offset = 0)
		{
			_logger.LogInformation("Getting all Pokemon with limit: {Limit}, offset: {Offset}", limit, offset);
			var pokemon = await _pokemonService.GetAllPokemonAsync(limit, offset);
			_logger.LogDebug("Retrieved {Count} Pokemon from service", pokemon.Count());
			return Ok(pokemon);
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<Pokemon>> GetPokemonById(int id)
		{
			_logger.LogInformation("Getting Pokemon by ID: {PokemonId}", id);
			var pokemon = await _pokemonService.GetPokemonByIdAsync(id);
			if (pokemon == null)
			{
				_logger.LogWarning("Pokemon with ID {PokemonId} not found", id);
				return NotFound();
			}
			_logger.LogDebug("Successfully retrieved Pokemon: {PokemonName}", pokemon.Name);
			return Ok(pokemon);
		}

		[HttpGet("name/{name}")]
		public async Task<ActionResult<Pokemon>> GetPokemonByName(string name)
		{
			_logger.LogInformation("Getting Pokemon by name: {PokemonName}", name);
			var pokemon = await _pokemonService.GetPokemonByNameAsync(name);
			if (pokemon == null)
			{
				_logger.LogWarning("Pokemon with name '{PokemonName}' not found", name);
				return NotFound();
			}
			_logger.LogDebug("Successfully retrieved Pokemon: {PokemonName}", pokemon.Name);
			return Ok(pokemon);
		}

		[HttpGet("type/{type}")]
		public async Task<ActionResult<IEnumerable<Pokemon>>> GetPokemonByType(string type)
		{
			var pokemon = await _pokemonService.GetPokemonByTypeAsync(type);
			return Ok(pokemon);
		}

		[HttpGet("ability/{ability}")]
		public async Task<ActionResult<IEnumerable<Pokemon>>> GetPokemonByAbility(string ability)
		{
			var pokemon = await _pokemonService.GetPokemonByAbilityAsync(ability);
			return Ok(pokemon);
		}

		[HttpGet("move/{move}")]
		public async Task<ActionResult<IEnumerable<Pokemon>>> GetPokemonByMove(string move)
		{
			var pokemon = await _pokemonService.GetPokemonByMoveAsync(move);
			return Ok(pokemon);
		}

		[HttpGet("stats")]
		public async Task<ActionResult<IEnumerable<Pokemon>>> GetPokemonByStats(
			[FromQuery] string? stat = null,
			[FromQuery] int? minValue = null,
			[FromQuery] int? maxValue = null)
		{
			if (stat == null)
				return BadRequest("Stat parameter is required");

			var pokemon = await _pokemonService.GetPokemonByStatRangeAsync(stat, minValue, maxValue);
			return Ok(pokemon);
		}

		[HttpPost]
		public async Task<ActionResult<Pokemon>> CreatePokemon([FromBody] Pokemon pokemon)
		{
			try
			{
				var created = await _pokemonService.CreatePokemonAsync(pokemon);
				return CreatedAtAction(nameof(GetPokemonById), new { id = created.Id }, created);
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(ex.Message);
			}
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult<Pokemon>> UpdatePokemon(int id, [FromBody] Pokemon pokemon)
		{
			if (id != pokemon.Id)
				return BadRequest("ID mismatch");
			try
			{
				var updated = await _pokemonService.UpdatePokemonAsync(id, pokemon);
				return Ok(updated);
			}
			catch (InvalidOperationException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> DeletePokemon(int id)
		{
			_logger.LogInformation("Deleting Pokemon with ID: {PokemonId}", id);
			var deleted = await _pokemonService.DeletePokemonAsync(id);
			if (!deleted)
			{
				_logger.LogWarning("Pokemon with ID {PokemonId} not found for deletion", id);
				return NotFound();
			}
			_logger.LogInformation("Successfully deleted Pokemon with ID: {PokemonId}", id);
			return NoContent();
		}

		[HttpGet("fetch/{id:int}")]
		public async Task<ActionResult<Pokemon>> FetchPokemonById(int id)
		{
			_logger.LogInformation("Fetching and saving Pokemon by ID: {PokemonId} from PokeAPI", id);

			try
			{
				var pokemon = await _pokemonService.FetchAndSavePokemonFromPokeApiAsync(id);
				if (pokemon == null)
				{
					_logger.LogWarning("Pokemon with ID {PokemonId} not found in PokeAPI", id);
					return NotFound($"Pokemon with ID {id} not found in PokeAPI");
				}
				_logger.LogInformation("Successfully fetched and saved Pokemon: {PokemonName} (ID: {PokemonId}) from PokeAPI", pokemon.Name, pokemon.Id);
				return Ok(pokemon);
			}
			catch (Exception ex)
			{
				_logger.LogError("Error fetching Pokemon {PokemonId}: {Error}", id, ex.Message);
				return StatusCode(500, $"Error fetching Pokemon: {ex.Message}");
			}
		}

		[HttpGet("fetch/name/{name}")]
		public async Task<ActionResult<Pokemon>> FetchPokemonByName(string name)
		{
			_logger.LogInformation("Fetching and saving Pokemon by name: {PokemonName} from PokeAPI", name);
			var pokemon = await _pokemonService.FetchAndSavePokemonFromPokeApiAsync(name);
			if (pokemon == null)
			{
				_logger.LogWarning("Pokemon with name '{PokemonName}' not found in PokeAPI", name);
				return NotFound($"Pokemon with name '{name}' not found in PokeAPI");
			}
			_logger.LogInformation("Successfully fetched and saved Pokemon: {PokemonName} from PokeAPI", pokemon.Name);
			return Ok(pokemon);
		}
	}
}
