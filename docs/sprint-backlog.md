# Sprint 1 : correspondance backlog → livrables

Objectif : *définir l'architecture et configurer ses parties ; un rendu très minimaliste, déployé,
présentant la première version d'un système de comptes fonctionnel.*

| Tâche | Pts | Livrable dans le dépôt |
| --- | --- | --- |
| PBI-01-a Choix des technos | 5 | React · C# / .NET 10 · PostgreSQL 17 · Auth0 · Docker Compose · GitHub Actions · Render (voir README) |
| PBI-01-b Docker Compose | 2 | [`docker-compose.yml`](../docker-compose.yml), [`backend/Dockerfile`](../backend/Dockerfile), [`frontend/Dockerfile`](../frontend/Dockerfile) |
| PBI-02-a Schéma BDD | 3 | [`User.cs`](../backend/src/ShopMaster.Api/Models/User.cs), [`ShopMasterDbContext.cs`](../backend/src/ShopMaster.Api/Data/ShopMasterDbContext.cs), migration [`InitialCreate`](../backend/src/ShopMaster.Api/Data/Migrations) |
| PBI-02-b BDD hébergée | 2 | `shopmaster-db` dans [`render.yaml`](../render.yaml) + conversion de l'URL Render ([`PostgresConnectionString.cs`](../backend/src/ShopMaster.Api/Data/PostgresConnectionString.cs)) |
| PBI-03-a Auth0 + base API | 3 | [`Program.cs`](../backend/src/ShopMaster.Api/Program.cs), [`Auth0Options.cs`](../backend/src/ShopMaster.Api/Auth/Auth0Options.cs), [`docs/auth0.md`](auth0.md) |
| PBI-03-b Modèles users | 1 | [`User.cs`](../backend/src/ShopMaster.Api/Models/User.cs), [`Roles.cs`](../backend/src/ShopMaster.Api/Models/Roles.cs), [`UserDto.cs`](../backend/src/ShopMaster.Api/Contracts/UserDto.cs) |
| PBI-03-c Controllers users | 2 | [`UsersController.cs`](../backend/src/ShopMaster.Api/Controllers/UsersController.cs) |
| PBI-04 Tests endpoints | 3 | [`UsersApiTests.cs`](../backend/tests/ShopMaster.Api.Tests/UsersApiTests.cs) (+ santé, conversion d'URL) |
| PBI-05-a Fichier CI/CD | 3 | [`.github/workflows/ci.yml`](../.github/workflows/ci.yml), [`render.yaml`](../render.yaml) |
| PBI-05-b Tests dans la CI | 2 | jobs `api-tests`, `frontend`, `docker-compose` + `autoDeployTrigger: checksPass` |

Le backlog ne comportait aucune tâche frontend (point d'attention du Sprint Planning) : le minimum nécessaire
à la démonstration a été ajouté dans `frontend/` (pages de connexion, d'inscription et d'accueil connecté).

## Critères d'acceptation

| PBI | Critère | Comment le vérifier |
| --- | --- | --- |
| 01 | Docker Compose complet, technologies et architecture arrêtées | `docker compose up --build` ; job CI `docker-compose` |
| 02 | Base fonctionnelle, capable de stocker les utilisateurs | table `users` créée par migration ; test `GetUsers_AsAdmin_ReturnsStoredAccounts` |
| 03 | Créer un compte et s'y connecter de manière sécurisée | inscription Auth0 puis `POST /api/users` ; jetons JWT vérifiés (signature, émetteur, audience) |
| 04 | Des tests vérifient que l'API renvoie et crée des comptes | `dotnet test` dans `backend/` (25 tests) |
| 05 | Environnement déployé sur Render avec Docker | Blueprint `render.yaml`, déploiement conditionné à la CI |

## Démonstration en Sprint Review

1. Réveiller l'API (`https://<api>.onrender.com/health`) puis ouvrir le frontend.
2. **Créer un compte** : *Créer un compte* → formulaire Auth0.
3. **Se connecter** de manière sécurisée → retour sur ShopMaster.
4. **Page d'accueil** : informations du compte lues en base via l'API (avec un compte `Admin`, liste des comptes).
5. Montrer le **pipeline CI vert** sur GitHub et les services en ligne sur Render.

## Hors périmètre (sprints suivants)

Liste des articles, fiches produit, barre de recherche, panier, diagnostics et performances,
double authentification, autorisations fines, profil détaillé, tests E2E Selenium.
