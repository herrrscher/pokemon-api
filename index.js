const pokemonGrid      = document.getElementById("pokemon-grid");
const searchInput      = document.getElementById("search-input");
const typeFilter       = document.getElementById("type-filter");
const sortOrderSelect  = document.getElementById("sort-order");
const resetBtn         = document.getElementById("reset-filters");

let allPokemon = [];

async function fetchPokemon() {
    allPokemon = [];
    for (let i = 1; i <= 151; i++) {
        const url = "https://pokeapi.co/api/v2/pokemon/" + i;
        const response    = await fetch(url);
        const pokemonData = await response.json();
        allPokemon.push(pokemonData);
    }
    populateTypeFilter();
    renderPokemonList(allPokemon);
}

function renderPokemonList(list) {
    pokemonGrid.innerHTML = "";

    for (let i = 0; i < list.length; i++) {
        const pok = list[i];
        const card = document.createElement("div");
        card.classList.add("pokemon-card");

        const name = pok.name.charAt(0).toUpperCase() + pok.name.slice(1);

        const types = [];
        for (let t = 0; t < pok.types.length; t++) {
            types.push(pok.types[t].type.name);
        }

        let badgesHtml = "";
        for (let j = 0; j < types.length; j++) {
            badgesHtml += '<span class="type-badge">' + types[j] + '</span>';
        }

        card.innerHTML =
            '<span class="poke-number">#' + pok.id.toString().padStart(3, "0") + '</span>' +
            '<img src="' + pok.sprites.other['official-artwork'].front_default + '" alt="' + name + '">' +
            '<h3>' + name + '</h3>' +
            '<div>' + badgesHtml + '</div>';

        const badges = card.querySelectorAll(".type-badge");
        for (let j = 0; j < badges.length; j++) {
            const badge = badges[j];
            badge.style.cursor = "pointer";
            badge.addEventListener("click", function() {
                const selectedType = badge.textContent.toLowerCase();
                searchInput.value = "";
                typeFilter.value    = "";
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

searchInput.addEventListener("input", applyFilters);

function populateTypeFilter() {
    const typesSet = new Set();
    for (let i = 0; i < allPokemon.length; i++) {
        for (let j = 0; j < allPokemon[i].types.length; j++) {
            typesSet.add(allPokemon[i].types[j].type.name);
        }
    }
    typesSet.forEach(function(type) {
        const option = document.createElement("option");
        option.value       = type;
        option.textContent = type.charAt(0).toUpperCase() + type.slice(1);
        typeFilter.appendChild(option);
    });
}

typeFilter.addEventListener("change", applyFilters);
sortOrderSelect.addEventListener("change", applyFilters);

resetBtn.addEventListener("click", function() {
    searchInput.value      = "";
    typeFilter.value       = "";
    sortOrderSelect.value  = "asc";
    renderPokemonList(allPokemon);
});

function applyFilters() {
    let filtered = allPokemon.slice();

    const term = searchInput.value.toLowerCase();
    if (term) {
        filtered = filtered.filter(function(pokemon) {
            return pokemon.name.toLowerCase().includes(term) ||
                   pokemon.id.toString().includes(term);
        });
    }

    const selType = typeFilter.value;
    if (selType) {
        const temp = [];
        for (let i = 0; i < filtered.length; i++) {
            const pokemon = filtered[i];
            for (let j = 0; j < pokemon.types.length; j++) {
                if (pokemon.types[j].type.name === selType) {
                    temp.push(pokemon);
                    break;
                }
            }
        }
        filtered = temp;
    }

    const sortOrder = sortOrderSelect.value;
    filtered.sort(function(a, b) {
        if (sortOrder === "asc") {
            return a.id - b.id;
        } else {
            return b.id - a.id;
        }
    });

    renderPokemonList(filtered);
}

fetchPokemon();