const baseUrl = 'http://localhost:5000/api/pokemon';

function toggleResultArea(elementId) {
    const container = document.getElementById(elementId);
    let textarea = container.querySelector('textarea');

    if (!textarea) {
        textarea = document.createElement('textarea');
        textarea.rows = 15;
        textarea.cols = 80;
        textarea.className = 'result-textarea';
        textarea.readOnly = true;
        container.appendChild(textarea);
    } else {
        textarea.style.display = textarea.style.display === 'none' ? 'block' : 'none';
    }

    return textarea;
}

function displayResult(elementId, data, url = '') {
    const textarea = toggleResultArea(elementId);
    const timestamp = new Date().toLocaleTimeString();

    let resultText = `Response (${timestamp}):\n`;

    if (url) {
        resultText += `URL: ${url}\n`;
    }

    resultText += '\n' + JSON.stringify(data, null, 2);

    textarea.value = resultText;
    textarea.style.display = 'block';
} function displayError(elementId, error, url = '') {
    const textarea = toggleResultArea(elementId);
    const timestamp = new Date().toLocaleTimeString();

    let errorText = `Error (${timestamp}):\n`;

    if (url) {
        errorText += `URL: ${url}\n`;
    }

    errorText += '\n' + error;

    textarea.value = errorText;
    textarea.style.display = 'block';
}

async function makeRequest(url, method = 'GET', body = null) {
    try {
        const options = {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'X-API-Key': 'frontend-key-abcde'
            }
        };

        if (body) {
            options.body = JSON.stringify(body);
        }

        const response = await fetch(url, options);

        if (response.status === 204) {
            return { success: true, message: 'Operation completed successfully (No Content)' };
        }

        const contentType = response.headers.get('content-type');

        let data;
        if (contentType && contentType.includes('application/json')) {
            const responseText = await response.text();

            if (!responseText || responseText.trim() === '') {
                data = {
                    message: 'Empty response body',
                    status: response.status,
                    statusText: response.statusText
                };
            } else {
                try {
                    data = JSON.parse(responseText);
                } catch (parseError) {
                    data = {
                        message: 'Failed to parse JSON response',
                        rawResponse: responseText,
                        parseError: parseError.message
                    };
                }
            }
        } else {
            data = await response.text();
        }

        if (!response.ok) {
            if (response.status === 401) {
                console.error('Invalid API key provided');
                throw new Error('You have an invalid API key. Please check your API key and try again.');
            }

            if (response.status === 429) {
                console.error('Rate limit exceeded');
                throw new Error('Rate limit exceeded. You are making too many requests. Please wait a moment and try again.');
            }

            throw new Error('HTTP ' + response.status + ': ' + (typeof data === 'string' ? data : JSON.stringify(data)));
        }

        return data;
    } catch (error) {
        throw error;
    }
}

async function getFirstThreePokemon() {
    const url = baseUrl + '?limit=3&offset=0';
    try {
        const data = await makeRequest(url);
        displayResult('getFirstThreeResult', data, url);
    } catch (error) {
        displayError('getFirstThreeResult', error.message, url);
    }
}

async function getPikachuById() {
    const url = baseUrl + '/25';
    try {
        const data = await makeRequest(url);
        displayResult('getPikachuByIdResult', data, url);
    } catch (error) {
        displayError('getPikachuByIdResult', error.message, url);
    }
}

async function getCharizardByName() {
    const url = baseUrl + '/name/charizard';
    try {
        const data = await makeRequest(url);
        displayResult('getCharizardByNameResult', data, url);
    } catch (error) {
        displayError('getCharizardByNameResult', error.message, url);
    }
}

async function getElectricTypePokemon() {
    const url = baseUrl + '/type/electric';
    try {
        const data = await makeRequest(url);
        displayResult('getElectricTypeResult', data, url);
    } catch (error) {
        displayError('getElectricTypeResult', error.message, url);
    }
}

async function getPokemonWithStaticAbility() {
    const url = baseUrl + '/ability/static';
    try {
        const data = await makeRequest(url);
        displayResult('getStaticAbilityResult', data, url);
    } catch (error) {
        displayError('getStaticAbilityResult', error.message, url);
    }
}

async function getPokemonWithThunderbolt() {
    const url = baseUrl + '/move/thunderbolt';
    try {
        const data = await makeRequest(url);
        displayResult('getThunderboltMoveResult', data, url);
    } catch (error) {
        displayError('getThunderboltMoveResult', error.message, url);
    }
}

async function getPokemonWithHpRange() {
    const url = baseUrl + '/stats?stat=hp&minValue=80&maxValue=120';
    try {
        const data = await makeRequest(url);
        displayResult('getHpRangeResult', data, url);
    } catch (error) {
        displayError('getHpRangeResult', error.message, url);
    }
}

async function checkApiHealth() {
    const healthUrl = baseUrl.replace('/api/pokemon', '/health');
    try {
        const response = await fetch(healthUrl);
        const result = {
            status: response.status,
            statusText: response.statusText,
            timestamp: new Date().toISOString(),
            healthy: response.status === 200
        };
        displayResult('checkHealthResult', result, healthUrl);
    } catch (error) {
        displayError('checkHealthResult', error.message, healthUrl);
    }
}

async function fetchPokemonFromPokeApi() {
    const url = baseUrl + '/fetch/1010';
    try {
        const data = await makeRequest(url);
        displayResult('fetchPokeApiResult', data, url);
    } catch (error) {
        displayError('fetchPokeApiResult', error.message, url);
    }
}

async function fetchIronHandsByName() {
    const url = baseUrl + '/fetch/name/iron-hands';
    try {
        const data = await makeRequest(url);
        displayResult('fetchIronHandsResult', data, url);
    } catch (error) {
        displayError('fetchIronHandsResult', error.message, url);
    }
}

async function runQuickExamples() {
    const examples = [
        getFirstThreePokemon,
        getPikachuById,
        getCharizardByName,
        getElectricTypePokemon,
        getPokemonWithStaticAbility,
        getPokemonWithThunderbolt,
        getPokemonWithHpRange,
        checkApiHealth,
        fetchPokemonFromPokeApi,
        fetchIronHandsByName
    ];

    for (let i = 0; i < examples.length; i++) {
        try {
            await examples[i]();
            await new Promise(resolve => setTimeout(resolve, 500));
        } catch (error) {
            console.error('Example ' + (i + 1) + ' failed:', error);
        }
    }
}

async function getAllPokemon() {
    const limit = document.getElementById('getAllLimit').value;
    const offset = document.getElementById('getAllOffset').value;
    const url = baseUrl + '?limit=' + limit + '&offset=' + offset;

    try {
        const data = await makeRequest(url);
        displayResult('getAllResult', data, url);
    } catch (error) {
        displayError('getAllResult', error.message, url);
    }
}

async function getPokemonById() {
    const id = document.getElementById('pokemonId').value;
    const url = baseUrl + '/' + id;

    try {
        const data = await makeRequest(url);
        displayResult('getByIdResult', data, url);
    } catch (error) {
        displayError('getByIdResult', error.message, url);
    }
}

async function getPokemonByName() {
    const name = document.getElementById('pokemonName').value;
    const url = baseUrl + '/name/' + encodeURIComponent(name);

    try {
        const data = await makeRequest(url);
        displayResult('getByNameResult', data, url);
    } catch (error) {
        displayError('getByNameResult', error.message, url);
    }
}

async function getPokemonByType() {
    const type = document.getElementById('pokemonType').value;
    const url = baseUrl + '/type/' + encodeURIComponent(type);

    try {
        const data = await makeRequest(url);
        displayResult('getByTypeResult', data, url);
    } catch (error) {
        displayError('getByTypeResult', error.message, url);
    }
}

async function getPokemonByAbility() {
    const ability = document.getElementById('pokemonAbility').value;
    const url = baseUrl + '/ability/' + encodeURIComponent(ability);

    try {
        const data = await makeRequest(url);
        displayResult('getByAbilityResult', data, url);
    } catch (error) {
        displayError('getByAbilityResult', error.message, url);
    }
}

async function getPokemonByMove() {
    const move = document.getElementById('pokemonMove').value;
    const url = baseUrl + '/move/' + encodeURIComponent(move);

    try {
        const data = await makeRequest(url);
        displayResult('getByMoveResult', data, url);
    } catch (error) {
        displayError('getByMoveResult', error.message, url);
    }
}

async function getPokemonByStats() {
    const stat = document.getElementById('statName').value;
    const minValue = document.getElementById('minStatValue').value;
    const maxValue = document.getElementById('maxStatValue').value;

    const url = baseUrl + '/stats?stat=' + encodeURIComponent(stat) + '&minValue=' + minValue + '&maxValue=' + maxValue;

    try {
        const data = await makeRequest(url);
        displayResult('getByStatsResult', data, url);
    } catch (error) {
        displayError('getByStatsResult', error.message, url);
    }
}



async function createPokemon() {
    const jsonText = document.getElementById('newPokemonJson').value;

    try {
        const pokemonData = JSON.parse(jsonText);
        const data = await makeRequest(baseUrl, 'POST', pokemonData);
        displayResult('createResult', data);
    } catch (error) {
        displayError('createResult', error.message);
    }
}

async function updatePokemon() {
    const id = document.getElementById('updatePokemonId').value;
    const jsonText = document.getElementById('updatePokemonJson').value;

    try {
        const pokemonData = JSON.parse(jsonText);
        const url = baseUrl + '/' + id;
        const data = await makeRequest(url, 'PUT', pokemonData);
        displayResult('updateResult', data, url);
    } catch (error) {
        displayError('updateResult', error.message, url);
    }
}

async function deletePokemon() {
    const id = document.getElementById('deletePokemonId').value;
    const url = baseUrl + '/' + id;

    try {
        const data = await makeRequest(url, 'DELETE');
        displayResult('deleteResult', data, url);
    } catch (error) {
        displayError('deleteResult', error.message, url);
    }
}

async function customTest() {
    const endpoint = document.getElementById('customEndpoint').value;
    const url = baseUrl + endpoint;
    try {
        const data = await makeRequest(url);
        displayResult('customResult', data, url);
    } catch (error) {
        displayError('customResult', error.message, url);
    }
}

async function fetchPokemonById() {
    const id = document.getElementById('fetchPokemonId').value;
    const url = baseUrl + '/fetch/' + id;
    try {
        const data = await makeRequest(url);
        displayResult('fetchByIdResult', data, url);
    } catch (error) {
        displayError('fetchByIdResult', error.message, url);
    }
}

async function fetchPokemonByName() {
    const name = document.getElementById('fetchPokemonName').value;
    const url = baseUrl + '/fetch/name/' + name;
    try {
        const data = await makeRequest(url);
        displayResult('fetchByNameResult', data, url);
    } catch (error) {
        displayError('fetchByNameResult', error.message, url);
    }
}

function clearAllResults() {
    const resultElements = [
        'getFirstThreeResult', 'getPikachuByIdResult', 'getCharizardByNameResult', 'getElectricTypeResult',
        'getStaticAbilityResult', 'getThunderboltMoveResult', 'getHpRangeResult', 'checkHealthResult',
        'fetchPokeApiResult', 'fetchIronHandsResult',
        'getAllResult', 'getByIdResult', 'getByNameResult',
        'getByTypeResult', 'getByAbilityResult', 'getByMoveResult', 'getByStatsResult',
        'fetchByIdResult', 'fetchByNameResult',
        'createResult', 'updateResult', 'deleteResult', 'customResult'
    ];

    resultElements.forEach(function (id) {
        const element = document.getElementById(id);
        if (element) {
            const textarea = element.querySelector('textarea');
            if (textarea) {
                textarea.style.display = 'none';
                textarea.value = '';
            }
            element.innerHTML = '';
        }
    });
}
