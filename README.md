
# Kay — Jeu de Warri haïtien

Projet de session réalisé dans le cadre du cours **INF1011 — Génie logiciel** à l'Université du Québec à Trois-Rivières (UQTR), automne 2026.

## Présentation

Kay est une application permettant à deux joueurs de jouer au Warri haïtien à distance, en temps réel.

L'application comprend :
- Un mode multijoueur en ligne.
- Deux variantes de règles, dont « Couper la tête ».
- La gestion des manches, des captures et des réceptacles.
- Un tableau d'honneur des parties terminées.
- Des tests automatisés.

## Technologies

| Composant | Technologie |
|---|---|
| Backend | C# — ASP.NET Core (.NET 10) |
| Frontend | React — Vite |
| Communication en temps réel | SignalR |
| Persistance | Entity Framework Core — SQLite |
| Tests backend | MSTest |
| Tests frontend | Vitest |

## Structure du projet

- `Kay.Api/` : API, logique du jeu et persistance.
- `Kay.Client/` : interface React.
- `Kay.Tests/` : tests du backend.
- `Kay.slnx` : solution .NET.

## Démarrage

### Prérequis

- SDK .NET 10
- Node.js et npm

### Backend

Depuis la racine du projet :

```bash
dotnet restore Kay.slnx
dotnet run --project Kay.Api
```

### Frontend

Dans un deuxième terminal :

```bash
cd Kay.Client
npm ci
npm run dev
```

Ouvrir ensuite l'adresse locale affichée par Vite.

## Exécuter les tests

Tests backend :

```bash
dotnet test Kay.Tests/Kay.Tests.csproj
```

Tests frontend :

```bash
cd Kay.Client
npm test -- --run
```

## Projet universitaire

Ce dépôt contient le code source du projet Kay développé pour la phase 2 du cours INF1011.

