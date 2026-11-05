import { StrictMode } from 'react';
import type { ReactNode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, useNavigate } from 'react-router-dom';
import { Auth0Provider } from '@auth0/auth0-react';
import type { AppState } from '@auth0/auth0-react';
import { App } from './App';
import { AppConfigContext, loadConfig, missingSettings } from './config';
import type { AppConfig } from './config';
import { BrandTag } from './components/BrandTag';
import './styles.css';

/** Auth0Provider branché sur le routeur : après connexion, on revient sur la page demandée. */
function Auth0WithRouter({ config, children }: { config: AppConfig; children: ReactNode }) {
  const navigate = useNavigate();
  return (
    <Auth0Provider
      domain={config.auth0Domain}
      clientId={config.auth0ClientId}
      authorizationParams={{
        redirect_uri: window.location.origin,
        audience: config.auth0Audience,
        scope: 'openid profile email',
      }}
      onRedirectCallback={(appState?: AppState) => navigate(appState?.returnTo ?? '/', { replace: true })}
    >
      {children}
    </Auth0Provider>
  );
}

function MissingConfiguration({ missing }: { missing: string[] }) {
  return (
    <main className="config-error">
      <BrandTag />
      <h1>Configuration Auth0 incomplète</h1>
      <p>Renseignez ces variables d'environnement puis redémarrez le frontend :</p>
      <ul>
        {missing.map((name) => (
          <li key={name}>
            <code>{name}</code>
          </li>
        ))}
      </ul>
      <p>La marche à suivre est décrite dans docs/auth0.md.</p>
    </main>
  );
}

const config = loadConfig();
const missing = missingSettings(config);

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    {missing.length > 0 ? (
      <MissingConfiguration missing={missing} />
    ) : (
      <AppConfigContext.Provider value={config}>
        <BrowserRouter>
          <Auth0WithRouter config={config}>
            <App />
          </Auth0WithRouter>
        </BrowserRouter>
      </AppConfigContext.Provider>
    )}
  </StrictMode>,
);
