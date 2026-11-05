import { createContext, useContext } from 'react';

export interface AppConfig {
  auth0Domain: string;
  auth0ClientId: string;
  auth0Audience: string;
  apiUrl: string;
}

declare global {
  interface Window {
    /** Généré au démarrage du conteneur nginx (voir nginx/40-runtime-config.sh). */
    __APP_CONFIG__?: Partial<AppConfig>;
  }
}

type BuildEnv = Partial<Record<'VITE_AUTH0_DOMAIN' | 'VITE_AUTH0_CLIENT_ID' | 'VITE_AUTH0_AUDIENCE' | 'VITE_API_URL', string>>;

/** La configuration du conteneur (runtime) l'emporte sur celle du build (variables VITE_*). */
export function resolveConfig(runtime: Partial<AppConfig>, env: BuildEnv): AppConfig {
  return {
    auth0Domain: runtime.auth0Domain || env.VITE_AUTH0_DOMAIN || '',
    auth0ClientId: runtime.auth0ClientId || env.VITE_AUTH0_CLIENT_ID || '',
    auth0Audience: runtime.auth0Audience || env.VITE_AUTH0_AUDIENCE || '',
    apiUrl: (runtime.apiUrl || env.VITE_API_URL || 'http://localhost:8080').replace(/\/+$/, ''),
  };
}

export function loadConfig(): AppConfig {
  return resolveConfig(window.__APP_CONFIG__ ?? {}, import.meta.env as BuildEnv);
}

/** Noms des variables d'environnement Auth0 qui ne sont pas renseignées. */
export function missingSettings(config: AppConfig): string[] {
  const required: [keyof AppConfig, string][] = [
    ['auth0Domain', 'AUTH0_DOMAIN'],
    ['auth0ClientId', 'AUTH0_CLIENT_ID'],
    ['auth0Audience', 'AUTH0_AUDIENCE'],
  ];
  return required.filter(([key]) => !config[key]).map(([, name]) => name);
}

export const AppConfigContext = createContext<AppConfig | null>(null);

export function useAppConfig(): AppConfig {
  const config = useContext(AppConfigContext);
  if (!config) throw new Error('AppConfigContext manquant');
  return config;
}
