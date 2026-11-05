import type { ReactNode } from 'react';
import { MemoryRouter } from 'react-router-dom';
import { vi } from 'vitest';
import { useAuth0 } from '@auth0/auth0-react';
import { AppConfigContext } from '../config';
import type { AppConfig } from '../config';

export const testConfig: AppConfig = {
  auth0Domain: 'shopmaster-test.eu.auth0.com',
  auth0ClientId: 'test-client-id',
  auth0Audience: 'https://api.shopmaster.test',
  apiUrl: 'http://api.test',
};

/** Simule le SDK Auth0 (à combiner avec vi.mock('@auth0/auth0-react') dans le fichier de test). */
export function mockAuth0(
  state: Partial<Pick<ReturnType<typeof useAuth0>, 'isLoading' | 'isAuthenticated' | 'error' | 'user'>> = {},
) {
  const actions = {
    loginWithRedirect: vi.fn(),
    logout: vi.fn(),
    getAccessTokenSilently: vi.fn().mockResolvedValue('test-token'),
  };
  const auth0 = { isLoading: false, isAuthenticated: false, error: undefined, user: undefined, ...state, ...actions };
  vi.mocked(useAuth0).mockReturnValue(auth0 as unknown as ReturnType<typeof useAuth0>);
  return actions;
}

type Route = { status: number; body?: unknown };

/** Remplace fetch par un faux serveur : clés "METHODE /chemin". */
export function mockApi(routes: Record<string, Route | Route[]>) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const key = `${init?.method ?? 'GET'} ${new URL(input.toString()).pathname}`;
    const entry = routes[key];
    const route = Array.isArray(entry) ? (entry.length > 1 ? entry.shift()! : entry[0]) : entry;
    if (!route) return new Response(null, { status: 404 });
    return new Response(route.body === undefined ? null : JSON.stringify(route.body), {
      status: route.status,
      headers: { 'Content-Type': 'application/json' },
    });
  });
  vi.stubGlobal('fetch', fetchMock);
  return fetchMock;
}

export function renderWithProviders(ui: ReactNode, initialPath = '/') {
  return (
    <AppConfigContext.Provider value={testConfig}>
      <MemoryRouter initialEntries={[initialPath]}>{ui}</MemoryRouter>
    </AppConfigContext.Provider>
  );
}
