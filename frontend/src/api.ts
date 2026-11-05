import { useMemo } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { useAppConfig } from './config';

export type Role = 'Customer' | 'Admin';

/** Compte renvoyé par l'API ShopMaster. */
export interface Account {
  id: string;
  email: string | null;
  name: string | null;
  role: Role;
  createdAt: string;
  lastLoginAt: string;
}

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
  }
}

export function createApiClient(baseUrl: string, getToken: () => Promise<string>) {
  async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
    const token = await getToken();
    let response: Response;
    try {
      response = await fetch(`${baseUrl}${path}`, {
        ...init,
        headers: { Accept: 'application/json', Authorization: `Bearer ${token}` },
      });
    } catch {
      throw new ApiError(0, `L'API (${baseUrl}) est injoignable.`);
    }
    if (!response.ok) {
      throw new ApiError(response.status, `L'API a répondu ${response.status} sur ${path}.`);
    }
    return (await response.json()) as T;
  }

  return {
    /** Crée le compte à la première connexion, le synchronise avec Auth0 ensuite. */
    syncAccount: () => request<Account>('/api/users', { method: 'POST' }),
    /** Tous les comptes (administrateurs uniquement). */
    listAccounts: () => request<Account[]>('/api/users'),
  };
}

export type ApiClient = ReturnType<typeof createApiClient>;

export function useApi(): ApiClient {
  const { apiUrl } = useAppConfig();
  const { getAccessTokenSilently } = useAuth0();
  return useMemo(
    () =>
      createApiClient(apiUrl, async () => {
        const token = await getAccessTokenSilently();
        if (!token) throw new ApiError(401, 'Session expirée : reconnectez-vous.');
        return token;
      }),
    [apiUrl, getAccessTokenSilently],
  );
}
