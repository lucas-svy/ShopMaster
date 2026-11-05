import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { LoginPage } from './LoginPage';
import { SignupPage } from './SignupPage';
import { mockAuth0, renderWithProviders } from '../test/helpers';

vi.mock('@auth0/auth0-react', () => ({ useAuth0: vi.fn() }));

describe('Page de connexion', () => {
  it("redirige vers la page de connexion sécurisée d'Auth0", async () => {
    const auth0 = mockAuth0();
    render(renderWithProviders(<LoginPage />, '/connexion'));

    await userEvent.click(screen.getByRole('button', { name: 'Se connecter' }));

    expect(auth0.loginWithRedirect).toHaveBeenCalledTimes(1);
    expect(auth0.loginWithRedirect.mock.calls[0][0]?.authorizationParams?.screen_hint).toBeUndefined();
  });

  it("affiche l'erreur renvoyée par Auth0", () => {
    mockAuth0({ error: new Error('access_denied') });
    render(renderWithProviders(<LoginPage />, '/connexion'));

    expect(screen.getByRole('alert')).toHaveTextContent('access_denied');
  });

  it("propose de créer un compte", () => {
    mockAuth0();
    render(renderWithProviders(<LoginPage />, '/connexion'));

    expect(screen.getByRole('link', { name: 'Créer un compte' })).toHaveAttribute('href', '/inscription');
  });
});

describe("Page d'inscription", () => {
  it("ouvre directement le formulaire d'inscription Auth0", async () => {
    const auth0 = mockAuth0();
    render(renderWithProviders(<SignupPage />, '/inscription'));

    await userEvent.click(screen.getByRole('button', { name: 'Créer mon compte' }));

    expect(auth0.loginWithRedirect).toHaveBeenCalledWith(
      expect.objectContaining({ authorizationParams: { screen_hint: 'signup' } }),
    );
  });
});
