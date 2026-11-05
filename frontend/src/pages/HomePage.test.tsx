import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { HomePage } from './HomePage';
import { mockApi, mockAuth0, renderWithProviders } from '../test/helpers';

vi.mock('@auth0/auth0-react', () => ({ useAuth0: vi.fn() }));

const alice = {
  id: '0193a1b2-0000-7000-8000-000000000001',
  email: 'alice@example.com',
  name: 'Alice Martin',
  role: 'Customer',
  createdAt: '2026-11-02T09:30:00Z',
  lastLoginAt: '2026-11-04T08:00:00Z',
};
const admin = { ...alice, id: '0193a1b2-0000-7000-8000-000000000002', name: 'Inès Admin', email: 'ines@example.com', role: 'Admin' };

describe("Page d'accueil connectée", () => {
  it("crée / synchronise le compte puis l'affiche", async () => {
    mockAuth0({ isAuthenticated: true });
    const fetchMock = mockApi({ 'POST /api/users': { status: 201, body: alice } });

    render(renderWithProviders(<HomePage />));

    expect(await screen.findByRole('heading', { name: 'Bonjour, Alice Martin' })).toBeInTheDocument();
    expect(screen.getByText('alice@example.com')).toBeInTheDocument();
    expect(screen.getByText('Client')).toBeInTheDocument();
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe('http://api.test/api/users');
    expect(new Headers(init?.headers).get('Authorization')).toBe('Bearer test-token');
  });

  it("n'appelle pas la liste des comptes pour un client", async () => {
    mockAuth0({ isAuthenticated: true });
    const fetchMock = mockApi({ 'POST /api/users': { status: 200, body: alice } });

    render(renderWithProviders(<HomePage />));
    await screen.findByRole('heading', { name: 'Bonjour, Alice Martin' });

    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  it("affiche les comptes enregistrés pour un administrateur", async () => {
    mockAuth0({ isAuthenticated: true });
    mockApi({
      'POST /api/users': { status: 200, body: admin },
      'GET /api/users': { status: 200, body: [alice, admin] },
    });

    render(renderWithProviders(<HomePage />));

    const table = await screen.findByRole('table', { name: 'Comptes enregistrés' });
    expect(within(table).getByText('Alice Martin')).toBeInTheDocument();
    expect(within(table).getByText('Inès Admin')).toBeInTheDocument();
  });

  it("explique l'erreur et permet de réessayer quand l'API ne répond pas", async () => {
    mockAuth0({ isAuthenticated: true });
    mockApi({ 'POST /api/users': [{ status: 503 }, { status: 200, body: alice }] });

    render(renderWithProviders(<HomePage />));

    expect(await screen.findByRole('alert')).toHaveTextContent('503');
    await userEvent.click(screen.getByRole('button', { name: 'Réessayer' }));
    expect(await screen.findByRole('heading', { name: 'Bonjour, Alice Martin' })).toBeInTheDocument();
  });

  it('déconnecte via Auth0', async () => {
    const auth0 = mockAuth0({ isAuthenticated: true });
    mockApi({ 'POST /api/users': { status: 200, body: alice } });

    render(renderWithProviders(<HomePage />));
    await userEvent.click(await screen.findByRole('button', { name: 'Se déconnecter' }));

    expect(auth0.logout).toHaveBeenCalledTimes(1);
  });
});
