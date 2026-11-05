# Déployer sur Render (PBI-05)

Le fichier [`render.yaml`](../render.yaml) décrit l'environnement : une base PostgreSQL et deux services Docker
(API et frontend), construits à partir des mêmes Dockerfile que Docker Compose.

## Première mise en ligne

1. Poussez le dépôt sur GitHub (branche `main`) et vérifiez que le workflow **CI** est vert.
2. Sur [dashboard.render.com](https://dashboard.render.com) : *New → Blueprint*, choisissez le dépôt.
3. Render crée `shopmaster-db`, `shopmaster-api` et `shopmaster-web`, et demande les variables `sync: false` :

   | Service | Variable | Valeur |
   | --- | --- | --- |
   | shopmaster-api | `Auth0__Domain` | domaine Auth0, ex. `shopmaster.eu.auth0.com` |
   | shopmaster-api | `Auth0__Audience` | `https://api.shopmaster.app` |
   | shopmaster-api | `Cors__AllowedOrigins` | URL du frontend, ex. `https://shopmaster-web.onrender.com` |
   | shopmaster-web | `AUTH0_DOMAIN` | domaine Auth0 |
   | shopmaster-web | `AUTH0_CLIENT_ID` | Client ID de l'application SPA |
   | shopmaster-web | `AUTH0_AUDIENCE` | `https://api.shopmaster.app` |
   | shopmaster-web | `API_URL` | URL de l'API, ex. `https://shopmaster-api.onrender.com` |

   Les URL exactes (`*.onrender.com`) s'affichent sur la page de chaque service. Si un nom est déjà pris,
   Render ajoute un suffixe : reportez les URL réelles, puis redéployez.
4. Ajoutez l'URL du frontend dans Auth0 (Callback, Logout, Web Origins : voir `auth0.md`).
5. La chaîne de connexion de la base est injectée automatiquement ; l'API applique les migrations au démarrage.

## Déploiements suivants

- Chaque push sur `main` lance la CI. Grâce à `autoDeployTrigger: checksPass`, Render ne déploie
  **que si tous les checks GitHub passent** : un test en échec bloque le déploiement (PBI-05-b).
- Conseillé : protéger `main` sur GitHub (*Settings → Branches*) en exigeant les checks de la CI avant fusion.

## Limites du plan gratuit

- **Mise en veille** : un service gratuit s'endort après 15 minutes sans trafic ; le premier accès prend
  jusqu'à une minute (le frontend l'indique). Avant la Sprint Review, ouvrez l'API (`/health`) puis le frontend
  pour les réveiller.
- **Base gratuite** : elle expire au bout de 30 jours ; prévoir un plan payant ou une recréation pour la suite.
- L'hébergeur peut changer (hypothèse du PBI-05) : tout repose sur des images Docker standard et des
  variables d'environnement, rien n'est propre à Render en dehors de `render.yaml`.
