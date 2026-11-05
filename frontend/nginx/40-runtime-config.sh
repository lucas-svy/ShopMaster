#!/bin/sh
# Exécuté par l'image nginx au démarrage du conteneur.
# Écrit la configuration du frontend à partir des variables d'environnement :
# la même image fonctionne ainsi en local (Docker Compose) et sur Render, sans rebuild.
set -eu

escape() { printf '%s' "$1" | sed -e 's/\\/\\\\/g' -e 's/"/\\"/g'; }

cat > /usr/share/nginx/html/config.js <<EOF
window.__APP_CONFIG__ = {
  auth0Domain: "$(escape "${AUTH0_DOMAIN:-}")",
  auth0ClientId: "$(escape "${AUTH0_CLIENT_ID:-}")",
  auth0Audience: "$(escape "${AUTH0_AUDIENCE:-}")",
  apiUrl: "$(escape "${API_URL:-}")"
};
EOF

echo "40-runtime-config.sh: config.js written (API_URL=${API_URL:-unset})"
