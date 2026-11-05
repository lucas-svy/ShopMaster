# Configurer Auth0 (PBI-03)

Auth0 gère l'inscription, la connexion et les rôles. L'API ShopMaster ne voit jamais de mot de passe :
elle vérifie seulement les jetons JWT émis par Auth0. Comptez une quinzaine de minutes.

## 1. Créer l'application (frontend)

1. *Applications → Applications → Create Application* : nom `ShopMaster`, type **Single Page Web Applications**.
2. Onglet *Settings*, notez **Domain** (`AUTH0_DOMAIN`) et **Client ID** (`AUTH0_CLIENT_ID`).
3. Renseignez les URL autorisées (séparées par des virgules) :

   | Champ | Valeur |
   | --- | --- |
   | Allowed Callback URLs | `http://localhost:3000, http://localhost:5173, https://<frontend>.onrender.com` |
   | Allowed Logout URLs | idem |
   | Allowed Web Origins | idem |

4. *Save Changes*.

## 2. Déclarer l'API (backend)

1. *Applications → APIs → Create API* : nom `ShopMaster API`, identifiant **`https://api.shopmaster.app`**, algorithme RS256.
2. Cet identifiant est l'**audience** : `AUTH0_AUDIENCE` (frontend et API) doit avoir exactement la même valeur.

## 3. Ajouter email, nom et rôles au jeton

Par défaut, le jeton d'accès Auth0 ne contient ni l'email, ni le nom, ni les rôles. Une *Action* les ajoute.

1. *Actions → Library → Create Action → Build from scratch* : nom `ShopMaster claims`, trigger **Login / Post Login**.
2. Collez le code suivant puis *Deploy* :

   ```js
   exports.onExecutePostLogin = async (event, api) => {
     const namespace = 'https://shopmaster.app';
     api.accessToken.setCustomClaim(`${namespace}/roles`, event.authorization?.roles ?? []);
     api.accessToken.setCustomClaim(`${namespace}/email`, event.user.email);
     api.accessToken.setCustomClaim(`${namespace}/name`, event.user.name);
   };
   ```

3. *Actions → Triggers → post-login* : glissez l'action `ShopMaster claims` dans le flux, puis *Apply*.

Le préfixe `https://shopmaster.app` correspond au paramètre `Auth0__ClaimsNamespace` de l'API (valeur par défaut).

## 4. Rôles

- Tout compte est **Client** (`Customer`) par défaut.
- Pour un administrateur : *User Management → Roles → Create Role* `Admin`, puis assignez-le à l'utilisateur
  (*Users → l'utilisateur → Roles → Assign Roles*).
- Le rôle est relu à chaque connexion : un changement dans Auth0 est pris en compte à la reconnexion suivante.
- Un administrateur voit la liste de tous les comptes sur la page d'accueil (`GET /api/users`).

## 5. Reporter les valeurs

Dans `.env` (Docker Compose) :

```env
AUTH0_DOMAIN=votre-tenant.eu.auth0.com
AUTH0_CLIENT_ID=xxxxxxxxxxxxxxxxxxxxxxxx
AUTH0_AUDIENCE=https://api.shopmaster.app
```

Sur Render, les mêmes valeurs vont dans les variables des deux services (voir `deploiement-render.md`).

## Bon à savoir

- En `localhost`, Auth0 affiche un écran de consentement à la première connexion : il suffit d'accepter.
- La double authentification, les autorisations fines et le profil détaillé sont hors périmètre de ce sprint.
