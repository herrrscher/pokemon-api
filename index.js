const pokemonGrid       = document.getElementById("pokemon-grid");
const searchInput       = document.getElementById("search-input");
const typeFilter        = document.getElementById("type-filter");
const sortOrderSelect   = document.getElementById("sort-order");
const eggGroupFilter    = document.getElementById("egg-group-filter");
const statFilter        = document.getElementById("stat-filter");   // NEW
const statTier          = document.getElementById("stat-tier");     // NEW
const resetBtn          = document.getElementById("reset-filters");

let allPokemon = [];

async function fetchPokemon() {
    allPokemon = [];
    for (let i = 1; i <= 151; i++) {
        const url = "https://pokeapi.co/api/v2/pokemon/" + i;
        const response = await fetch(url);
        const pokemonData = await response.json();

        // Fetch species data for egg groups
        const speciesUrl = "https://pokeapi.co/api/v2/pokemon-species/" + i;
        const speciesResponse = await fetch(speciesUrl);
        const speciesData = await speciesResponse.json();

        const eggGroups = speciesData.egg_groups.map(group => group.name);
        pokemonData.eggGroups = eggGroups;

        allPokemon.push(pokemonData);
    }

    populateTypeFilter();
    populateEggGroupFilter();
    renderPokemonList(allPokemon);
}

function renderPokemonList(list) {
    pokemonGrid.innerHTML = "";

    const selectedStat = statFilter.value;
    const selectedTier = statTier.value;

    for (let i = 0; i < list.length; i++) {
        const pok = list[i];
        const card = document.createElement("div");
        card.classList.add("pokemon-card");

        const name = pok.name.charAt(0).toUpperCase() + pok.name.slice(1);

        const types = pok.types.map(t => t.type.name);

        let badgesHtml = "";
        for (let j = 0; j < types.length; j++) {
            badgesHtml += '<span class="type-badge">' + types[j] + '</span>';
        }

        card.innerHTML =
            '<span class="poke-number">#' + pok.id.toString().padStart(3, "0") + '</span>' +
            '<img src="' + pok.sprites.other['official-artwork'].front_default + '" alt="' + name + '">' +
            '<h3>' + name + '</h3>' +
            '<div>' + badgesHtml + '</div>';

        // Highlight if matching stat filter
        if (selectedStat && selectedTier) {
            const statObj = pok.stats.find(s => s.stat.name === selectedStat);
            if (statObj) {
                const value = statObj.base_stat;
                if (
                    (selectedTier === "high" && value >= 120) ||
                    (selectedTier === "medium" && value >= 81 && value < 120) ||
                    (selectedTier === "low" && value < 81)
                ) {
                    card.classList.add("highlight-stat");
                }
            }
        }

        const badges = card.querySelectorAll(".type-badge");
        for (let j = 0; j < badges.length; j++) {
            const badge = badges[j];
            badge.style.cursor = "pointer";
            badge.addEventListener("click", function () {
                const selectedType = badge.textContent.toLowerCase();
                searchInput.value      = "";
                typeFilter.value       = "";
                eggGroupFilter.value   = "";
                statFilter.value       = "";
                statTier.value         = "";
                const filtered = [];
                for (let k = 0; k < allPokemon.length; k++) {
                    const pokemon = allPokemon[k];
                    for (let m = 0; m < pokemon.types.length; m++) {
                        if (pokemon.types[m].type.name === selectedType) {
                            filtered.push(pokemon);
                            break;
                        }
                    }
                }
                renderPokemonList(filtered);
            });
        }

        pokemonGrid.appendChild(card);
    }
}

function populateTypeFilter() {
    const typesSet = new Set();
    for (let i = 0; i < allPokemon.length; i++) {
        for (let j = 0; j < allPokemon[i].types.length; j++) {
            typesSet.add(allPokemon[i].types[j].type.name);
        }
    }
    typesSet.forEach(function (type) {
        const option = document.createElement("option");
        option.value = type;
        option.textContent = type.charAt(0).toUpperCase() + type.slice(1);
        typeFilter.appendChild(option);
    });
}

function populateEggGroupFilter() {
    const eggGroupSet = new Set();
    for (let i = 0; i < allPokemon.length; i++) {
        const groups = allPokemon[i].eggGroups || [];
        for (let j = 0; j < groups.length; j++) {
            eggGroupSet.add(groups[j]);
        }
    }
    eggGroupSet.forEach(function (group) {
        const option = document.createElement("option");
        option.value = group;
        option.textContent = group.charAt(0).toUpperCase() + group.slice(1);
        eggGroupFilter.appendChild(option);
    });
}

function applyFilters() {
    let filtered = allPokemon.slice();

    const term = searchInput.value.toLowerCase();
    if (term) {
        filtered = filtered.filter(function (pokemon) {
            return pokemon.name.toLowerCase().includes(term) ||
                pokemon.id.toString().includes(term);
        });
    }

    const selType = typeFilter.value;
    if (selType) {
        filtered = filtered.filter(pokemon =>
            pokemon.types.some(t => t.type.name === selType)
        );
    }

    const selectedEggGroup = eggGroupFilter.value;
    if (selectedEggGroup) {
        filtered = filtered.filter(pokemon =>
            pokemon.eggGroups && pokemon.eggGroups.includes(selectedEggGroup)
        );
    }

    const selectedStat = statFilter.value;
    const selectedTier = statTier.value;
    if (selectedStat && selectedTier) {
        filtered = filtered.filter(pokemon => {
            const statObj = pokemon.stats.find(s => s.stat.name === selectedStat);
            if (!statObj) return false;

            const baseStat = statObj.base_stat;

            if (selectedTier === "high") {
                return baseStat >= 120;
            } else if (selectedTier === "medium") {
                return baseStat >= 81 && baseStat < 120;
            } else if (selectedTier === "low") {
                return baseStat < 81;
            }

            return true;
        });
    }

    const sortOrder = sortOrderSelect.value;
    filtered.sort(function (a, b) {
        return sortOrder === "asc" ? a.id - b.id : b.id - a.id;
    });

    renderPokemonList(filtered);
}

// Event listeners
searchInput.addEventListener("input", applyFilters);
typeFilter.addEventListener("change", applyFilters);
sortOrderSelect.addEventListener("change", applyFilters);
eggGroupFilter.addEventListener("change", applyFilters);
statFilter.addEventListener("change", applyFilters);
statTier.addEventListener("change", applyFilters);

resetBtn.addEventListener("click", function () {
    searchInput.value       = "";
    typeFilter.value        = "";
    eggGroupFilter.value    = "";
    sortOrderSelect.value   = "asc";
    statFilter.value        = "";
    statTier.value          = "";
    renderPokemonList(allPokemon);
});

fetchPokemon();
