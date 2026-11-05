import { describe, expect, it } from 'vitest';
import { missingSettings, resolveConfig } from './config';

describe('configuration', () => {
  it("privilégie la configuration injectée par le conteneur et retire le / final de l'URL d'API", () => {
    const config = resolveConfig(
      { auth0Domain: 'runtime.eu.auth0.com', apiUrl: 'https://api.onrender.com/' },
      { VITE_AUTH0_DOMAIN: 'build.eu.auth0.com', VITE_AUTH0_CLIENT_ID: 'abc' },
    );

    expect(config.auth0Domain).toBe('runtime.eu.auth0.com');
    expect(config.auth0ClientId).toBe('abc');
    expect(config.apiUrl).toBe('https://api.onrender.com');
  });

  it('liste les variables Auth0 manquantes', () => {
    const config = resolveConfig({}, {});

    expect(missingSettings(config)).toEqual(['AUTH0_DOMAIN', 'AUTH0_CLIENT_ID', 'AUTH0_AUDIENCE']);
  });
});
