# ShopMaster

Application e-commerce, **Sprint 1** : architecture posée, environnement déployé et première version
d'un système de comptes (inscription, connexion sécurisée, page d'accueil connectée).

```
                      ┌──────────── Docker Compose / Render ────────────┐
 Navigateur ──────────►  frontend          api              db           │
     │                │  React + nginx ──► C# / .NET 10 ──► PostgreSQL   │
     │                │  (connexion,       (REST, LINQ,     (table users) │
     │                │   accueil)          EF Core)                      │
     │                └─────────────────────────────────────────────────┘
     └──── inscription / connexion / rôles ────► Auth0 (jetons JWT vérifiés par l'API)

 Push GitHub ──► CI (tests API, tests frontend, Docker Compose) ──► déploiement Render si tout est vert
```

| Dossier | Contenu |
| --- | --- |
| [`backend/`](backend) | API ASP.NET Core : `UsersController`, EF Core + PostgreSQL, validation des jetons Auth0, tests xUnit |
| [`frontend/`](frontend) | React + Vite : pages `/connexion`, `/inscription`, `/` (accueil connecté), servies par nginx |
| [`docker-compose.yml`](docker-compose.yml) | Les trois conteneurs : `db`, `api`, `frontend` |
| [`.github/workflows/ci.yml`](.github/workflows/ci.yml) | Intégration continue |
| [`render.yaml`](render.yaml) | Environnement Render (Blueprint) |
| [`docs/`](docs) | [Auth0](docs/auth0.md) · [Render](docs/deploiement-render.md) · [Backlog → livrables](docs/sprint-backlog.md) |

## Démarrer en local (Docker)

Prérequis : Docker Desktop et un tenant Auth0 configuré ([docs/auth0.md](docs/auth0.md)).

```bash
cp .env.example .env
```

Renseignez `AUTH0_DOMAIN`, `AUTH0_CLIENT_ID` et `AUTH0_AUDIENCE` dans `.env`, puis :

```bash
docker compose up --build
```

- Frontend : http://localhost:3000
- API : http://localhost:8080 (`/health`, `/health/ready`, `/api/users`)
- PostgreSQL : `localhost:5432` (identifiants dans `.env`)

## API

Toutes les routes `/api/users` exigent un jeton Auth0 (`Authorization: Bearer …`).

| Méthode | Route | Rôle | Réponse |
| --- | --- | --- | --- |
| `POST` | `/api/users` | connecté | crée le compte à la 1re connexion (**201**), le resynchronise avec Auth0 ensuite (**200**) |
| `GET` | `/api/users/me` | connecté | son propre compte (404 tant que le compte n'est pas créé) |
| `GET` | `/api/users/{id}` | connecté | son propre compte, ou n'importe lequel pour un `Admin` (sinon 403) |
| `GET` | `/api/users` | `Admin` | tous les comptes |
| `GET` | `/health`, `/health/ready` | public | processus vivant / base joignable |

Un compte contient `id`, `email`, `name`, `role` (`Customer` ou `Admin`), `createdAt`, `lastLoginAt`.
L'identifiant Auth0 est stocké mais jamais exposé.

## Tests

```bash
cd backend && dotnet test
```

25 tests d'intégration (PBI-04) : l'API tourne en mémoire, PostgreSQL est remplacé par SQLite et Auth0
par une clé de signature de test. Aucun Docker ni compte Auth0 n'est nécessaire. Le job CI `docker-compose`
complète ces tests sur un vrai PostgreSQL (migrations, table `users`, routes protégées).

```bash
cd frontend && npm install && npm test
```

## Développer sans Docker

```bash
docker compose up db
```

```bash
cd backend && dotnet run --project src/ShopMaster.Api
```

L'API lit `Auth0__Domain` et `Auth0__Audience` depuis l'environnement ou `dotnet user-secrets`.

```bash
cd frontend && npm install && npm run dev
```

Le frontend lit `frontend/.env.local` (modèle : `frontend/.env.example`).

Nouvelle migration après modification du modèle :

```bash
cd backend && dotnet tool restore && dotnet ef migrations add <Nom> --project src/ShopMaster.Api --output-dir Data/Migrations
```

## Choix techniques (PBI-01-a)

| Domaine | Choix | Pourquoi | Limite connue |
| --- | --- | --- | --- |
| Organisation | Docker Compose | chacun avance sur sa partie en parallèle | mise en place initiale plus longue |
| Frontend | React | déjà maîtrisé, composants réutilisables | référencement limité (rendu côté client) |
| Backend | C# / .NET 10 | LINQ, migrations et API intégrés | équipe moins expérimentée en C# |
| Stockage | PostgreSQL 17 | relationnel, s'intègre bien à Docker | plus long à modéliser qu'une base NoSQL |
| Identité | Auth0 | authentification externalisée, gain de temps | coût à grande échelle, dépendance fournisseur |
| Hébergement | Render + GitHub | gratuit, simple, tests à chaque push | mise en veille : démarrage lent |
