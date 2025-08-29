# PokeSharp

[EN](./README.md) | NL

Een RESTful API voor het beheren van Pokemon data met volledige CRUD operaties en zoekmogelijkheden.

## Inhoudsopgave
- [Aan de slag](#aan-de-slag)
- [Basis URL](#basis-url)
- [API Endpoints](#api-endpoints)
  - [Alle Pokemon ophalen](#alle-pokemon-ophalen)
  - [Pokemon ophalen op ID](#pokemon-ophalen-op-id)
  - [Pokemon ophalen op naam](#pokemon-ophalen-op-naam)
  - [Pokemon ophalen op type](#pokemon-ophalen-op-type)
  - [Pokemon ophalen op vaardigheid](#pokemon-ophalen-op-vaardigheid)
  - [Pokemon ophalen op beweging](#pokemon-ophalen-op-beweging)
  - [Pokemon ophalen op statistieken](#pokemon-ophalen-op-statistieken)
  - [Pokemon aanmaken](#pokemon-aanmaken)
  - [Pokemon bijwerken](#pokemon-bijwerken)
  - [Pokemon verwijderen](#pokemon-verwijderen)
  - [Ophalen van PokéAPI](#ophalen-van-pokéapi)
- [Data Modellen](#data-modellen)
- [Foutafhandeling](#foutafhandeling)
- [Voorbeelden](#voorbeelden)

## Aan de slag

De Pokemon Database API is gebouwd met ASP.NET Core en biedt Pokemon data beheermogelijkheden. Je kunt CRUD operaties uitvoeren, zoeken op verschillende criteria, en zelfs data direct ophalen van de officiële PokéAPI.

## API Endpoints

### Alle Pokemon ophalen

Haal een gepagineerde lijst op van alle Pokemon in de database.

**Endpoint:** `GET /api/pokemon`

**Query Parameters:**
- `limit` (optioneel): Aantal Pokemon om terug te geven (standaard: 20)
- `offset` (optioneel): Aantal Pokemon om over te slaan (standaard: 0)

**Response:** Array van Pokemon objecten

**Voorbeeld Verzoek:**
```javascript
const response = await fetch('http://localhost:5000/api/pokemon?limit=3&offset=0');
const pokemon = await response.json();
```

**Voorbeeld Response:**
```json
[
  {
    "id": 1,
    "name": "bulbasaur",
    "height": 7,
    "weight": 69,
    "baseExperience": 64,
    "order": 1,
    "isDefault": true,
    "types": ["grass", "poison"],
    "abilities": [
      {
        "name": "overgrow",
        "isHidden": false,
        "slot": 1
      }
    ],
    "sprites": {
      "frontDefault": "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/1.png",
      "frontShiny": "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/shiny/1.png",
      "backDefault": "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/back/1.png",
      "backShiny": "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/back/shiny/1.png"
    },
    "stats": {
      "hp": 45,
      "attack": 49,
      "defense": 49,
      "special-attack": 65,
      "special-defense": 65,
      "speed": 45
    }
  }
]
```

### Pokemon ophalen op ID

Haal een specifieke Pokemon op via zijn ID.

**Endpoint:** `GET /api/pokemon/{id}`

**Pad Parameters:**
- `id` (vereist): Pokemon ID (geheel getal)

**Response:** Pokemon object or 404 als niet gevonden

**Voorbeeld Verzoek:**
```javascript
const response = await fetch('http://localhost:5000/api/pokemon/25');
const pikachu = await response.json();
```

### Pokemon ophalen op naam

Haal een specifieke Pokemon op via zijn naam.

**Endpoint:** `GET /api/pokemon/name/{name}`

**Pad Parameters:**
- `name` (vereist): Pokemon naam (string)

**Response:** Pokemon object or 404 als niet gevonden

**Voorbeeld Verzoek:**
```javascript
const response = await fetch('http://localhost:5000/api/pokemon/name/charizard');
const charizard = await response.json();
```

### Pokemon ophalen op type

Haal alle Pokemon op van een specifiek type.

**Endpoint:** `GET /api/pokemon/type/{type}`

**Pad Parameters:**
- `type` (vereist): Pokemon type (string, bijv. "electric", "fire", "water")

**Response:** Array van Pokemon objecten

**Voorbeeld Verzoek:**
```javascript
const response = await fetch('http://localhost:5000/api/pokemon/type/electric');
const electricPokemon = await response.json();
```

### Pokemon ophalen op vaardigheid

Haal alle Pokemon op met een specifieke vaardigheid.

**Endpoint:** `GET /api/pokemon/ability/{ability}`

**Pad Parameters:**
- `ability` (vereist): Vaardigheid naam (string, bijv. "static", "overgrow")

**Response:** Array van Pokemon objecten

**Voorbeeld Verzoek:**
```javascript
const response = await fetch('http://localhost:5000/api/pokemon/ability/static');
const staticPokemon = await response.json();
```

### Pokemon ophalen op beweging

Haal alle Pokemon op die een specifieke beweging kunnen leren.

**Endpoint:** `GET /api/pokemon/move/{move}`

**Pad Parameters:**
- `move` (vereist): Beweging naam (string, bijv. "thunderbolt", "flamethrower")

**Response:** Array van Pokemon objecten

**Voorbeeld Verzoek:**
```javascript
const response = await fetch('http://localhost:5000/api/pokemon/move/thunderbolt');
const thunderboltPokemon = await response.json();
```

### Pokemon ophalen op statistieken

Haal Pokemon op gebaseerd op statistiek criteria.

**Endpoint:** `GET /api/pokemon/stats`

**Query Parameters:**
- `stat` (vereist): Statistiek naam ("hp", "attack", "defense", "special-attack", "special-defense", "speed")
- `minValue` (optioneel): Minimum statistiek waarde
- `maxValue` (optioneel): Maximum statistiek waarde

**Response:** Array van Pokemon objecten

**Voorbeeld Verzoek:**
```javascript
const response = await fetch('http://localhost:5000/api/pokemon/stats?stat=hp&minValue=80&maxValue=120');
const highHpPokemon = await response.json();
```

### Pokemon aanmaken

Voeg een nieuwe Pokemon toe aan de database.

**Endpoint:** `POST /api/pokemon`

**Request Body:** Pokemon object (JSON)

**Response:** Aangemaakte Pokemon object met status 201

**Voorbeeld Verzoek:**
```javascript
const newPokemon = {
  "id": 999,
  "name": "custompokemon",
  "height": 10,
  "weight": 100,
  "baseExperience": 100,
  "order": 999,
  "isDefault": true,
  "types": ["normal"],
  "abilities": [
    {
      "name": "run-away",
      "isHidden": false,
      "slot": 1
    }
  ],
  "sprites": {
    "frontDefault": "custom-url.png",
    "frontShiny": "custom-shiny-url.png",
    "backDefault": "custom-back-url.png",
    "backShiny": "custom-back-shiny-url.png"
  },
  "stats": {
    "hp": 100,
    "attack": 100,
    "defense": 100,
    "special-attack": 100,
    "special-defense": 100,
    "speed": 100
  }
};

const response = await fetch('http://localhost:5000/api/pokemon', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(newPokemon)
});
const created = await response.json();
```

### Pokemon bijwerken

Werk een bestaande Pokemon bij in de database.

**Endpoint:** `PUT /api/pokemon/{id}`

**Pad Parameters:**
- `id` (vereist): Pokemon ID om bij te werken

**Request Body:** Bijgewerkt Pokemon object (JSON)

**Response:** Bijgewerkt Pokemon object

**Voorbeeld Verzoek:**
```javascript
const updatedPokemon = {
  "id": 999,
  "name": "updatedpokemon",
  // ... andere eigenschappen
};

const response = await fetch('http://localhost:5000/api/pokemon/999', {
  method: 'PUT',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(updatedPokemon)
});
const updated = await response.json();
```

### Pokemon verwijderen

Verwijder een Pokemon uit de database.

**Endpoint:** `DELETE /api/pokemon/{id}`

**Pad Parameters:**
- `id` (vereist): Pokemon ID om te verwijderen

**Response:** 204 No Content bij succes, 404 als niet gevonden

**Voorbeeld Verzoek:**
```javascript
const response = await fetch('http://localhost:5000/api/pokemon/999', {
  method: 'DELETE'
});
// Response status 204 betekent succesvolle verwijdering
```

### Ophalen van PokéAPI

Haal Pokemon data direct op van de officiële PokéAPI en sla het op in je database.

#### Ophalen op ID

**Endpoint:** `GET /api/pokemon/fetch/{id}`

**Pad Parameters:**
- `id` (vereist): Pokemon ID van PokéAPI

**Response:** Opgehaalde en opgeslagen Pokemon object

**Voorbeeld Verzoek:**
```javascript
const response = await fetch('http://localhost:5000/api/pokemon/fetch/1010');
const ironHands = await response.json();
```

#### Ophalen op naam

**Endpoint:** `GET /api/pokemon/fetch/name/{name}`

**Pad Parameters:**
- `name` (vereist): Pokemon naam van PokéAPI

**Response:** Opgehaalde en opgeslagen Pokemon object

**Voorbeeld Verzoek:**
```javascript
const response = await fetch('http://localhost:5000/api/pokemon/fetch/name/iron-hands');
const ironHands = await response.json();
```

## Data Modellen

### Pokemon Model

```json
{
  "id": "geheel getal",
  "name": "string (vereist)",
  "height": "geheel getal",
  "weight": "geheel getal",
  "baseExperience": "geheel getal",
  "order": "geheel getal",
  "isDefault": "boolean",
  "types": ["string"],
  "abilities": [
    {
      "name": "string (vereist)",
      "isHidden": "boolean",
      "slot": "geheel getal"
    }
  ],
  "moves": [
    {
      "moveName": "string (vereist)",
      "learnMethod": "string (vereist)",
      "levelLearnedAt": "geheel getal (optioneel)"
    }
  ],
  "sprites": {
    "frontDefault": "string (vereist)",
    "frontShiny": "string (vereist)",
    "backDefault": "string (vereist)",
    "backShiny": "string (vereist)"
  },
  "stats": {
    "hp": "geheel getal",
    "attack": "geheel getal",
    "defense": "geheel getal",
    "special-attack": "geheel getal",
    "special-defense": "geheel getal",
    "speed": "geheel getal"
  },
  "gameIndices": [
    {
      "gameIndex": "geheel getal",
      "version": "string (vereist)"
    }
  ],
  "heldItems": [
    {
      "item": "string (vereist)",
      "versionDetails": [
        {
          "version": "string (vereist)",
          "rarity": "geheel getal"
        }
      ]
    }
  ],
  "speciesId": "geheel getal",
  "speciesName": "string"
}
```

## Foutafhandeling

De API retourneert juiste HTTP status codes:

- `200 OK`: Succesvolle GET verzoeken
- `201 Created`: Succesvolle POST verzoeken
- `204 No Content`: Succesvolle DELETE verzoeken
- `400 Bad Request`: Ongeldige verzoek parameters of body
- `404 Not Found`: Bron niet gevonden
- `409 Conflict`: Bron bestaat al (dubbele aanmaak)
- `500 Internal Server Error`: Server-side fouten

Fout responses bevatten beschrijvende berichten:

```json
{
  "message": "Pokemon met ID 999 niet gevonden",
  "status": 404
}
```

## Voorbeelden

### Volledig JavaScript Client Voorbeeld

Gebaseerd op de meegeleverde `example/example.js`, hier is een uitgebreid voorbeeld van het gebruik van de API:

```javascript
const baseUrl = 'http://localhost:5000/api/pokemon';

// Hulpfunctie voor het maken van verzoeken
async function makeRequest(url, method = 'GET', body = null) {
  const options = {
    method: method,
    headers: {
      'Content-Type': 'application/json',
    }
  };

  if (body) {
    options.body = JSON.stringify(body);
  }

  const response = await fetch(url, options);
  
  if (response.status === 204) {
    return { success: true, message: 'Operatie succesvol voltooid' };
  }

  const data = await response.json();
  
  if (!response.ok) {
    throw new Error(`HTTP ${response.status}: ${JSON.stringify(data)}`);
  }

  return data;
}

// Voorbeeld gebruik
async function voorbeelden() {
  try {
    // Haal eerste 3 Pokemon op
    const eersteThree = await makeRequest(`${baseUrl}?limit=3&offset=0`);
    console.log('Eerste 3 Pokemon:', eersteThree);

    // Haal Pikachu op via ID
    const pikachu = await makeRequest(`${baseUrl}/25`);
    console.log('Pikachu:', pikachu);

    // Haal elektrische type Pokemon op
    const electricTypes = await makeRequest(`${baseUrl}/type/electric`);
    console.log('Elektrische Pokemon:', electricTypes);

    // Haal Pokemon op met HP tussen 80-120
    const highHp = await makeRequest(`${baseUrl}/stats?stat=hp&minValue=80&maxValue=120`);
    console.log('Hoge HP Pokemon:', highHp);

    // Haal nieuwe Pokemon op van PokéAPI
    const newPokemon = await makeRequest(`${baseUrl}/fetch/1010`);
    console.log('Opgehaalde Pokemon:', newPokemon);

  } catch (error) {
    console.error('API Fout:', error.message);
  }
}

voorbeelden();
```

### Snelle Referentie

- **Basis URL**: `http://localhost:5000/api/pokemon`
- **Content-Type**: `application/json`
- **Paginering**: Gebruik `limit` en `offset` parameters
- **Zoeken**: Meerdere zoek endpoints op type, vaardigheid, beweging, statistieken
- **CRUD**: Volledige Create, Read, Update, Delete operaties
- **Integratie**: Directe PokéAPI ophaal mogelijkheden

Voor meer gedetailleerde voorbeelden en interactieve testen, bekijk de `example/` directory in deze repository.
