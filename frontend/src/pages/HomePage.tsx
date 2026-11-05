import { useEffect, useState } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { useApi } from '../api';
import type { Account, Role } from '../api';
import { BrandTag } from '../components/BrandTag';

type State =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; account: Account; accounts: Account[] | null };

const roleLabels: Record<Role, string> = { Customer: 'Client', Admin: 'Administrateur' };
const dateFormat = new Intl.DateTimeFormat('fr-FR', { dateStyle: 'long' });
const dateTimeFormat = new Intl.DateTimeFormat('fr-FR', { dateStyle: 'medium', timeStyle: 'short' });

/** Au-delà de ce délai, on prévient que l'API hébergée sur Render sort peut-être de veille. */
const SLOW_API_DELAY_MS = 4000;

export function HomePage() {
  const { logout, user } = useAuth0();
  const api = useApi();
  const [attempt, setAttempt] = useState(0);
  const [state, setState] = useState<State>({ status: 'loading' });
  const [slow, setSlow] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setState({ status: 'loading' });
    setSlow(false);
    const timer = setTimeout(() => setSlow(true), SLOW_API_DELAY_MS);

    (async () => {
      try {
        const account = await api.syncAccount();
        const accounts = account.role === 'Admin' ? await api.listAccounts() : null;
        if (!cancelled) setState({ status: 'ready', account, accounts });
      } catch (error) {
        if (!cancelled) setState({ status: 'error', message: error instanceof Error ? error.message : String(error) });
      } finally {
        clearTimeout(timer);
      }
    })();

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [api, attempt]);

  return (
    <div className="home">
      <header className="topbar">
        <BrandTag />
        <button
          type="button"
          className="button button--quiet"
          onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}
        >
          Se déconnecter
        </button>
      </header>

      <main className="home__main">
        {state.status === 'loading' && (
          <p className="status" role="status">
            Chargement de votre compte…
            {slow && <span className="status__slow"> Le serveur sort de veille, cela peut prendre jusqu'à une minute.</span>}
          </p>
        )}

        {state.status === 'error' && (
          <div className="notice notice--error" role="alert">
            <p>Votre compte n'a pas pu être chargé. {state.message}</p>
            <button type="button" className="button button--quiet" onClick={() => setAttempt((n) => n + 1)}>
              Réessayer
            </button>
          </div>
        )}

        {state.status === 'ready' && (
          <>
            <h1>Bonjour, {state.account.name ?? user?.name ?? state.account.email}</h1>

            <section className="section" aria-labelledby="account-title">
              <h2 id="account-title">Votre compte</h2>
              <dl className="facts">
                <div>
                  <dt>Email</dt>
                  <dd>{state.account.email ?? 'Non renseigné'}</dd>
                </div>
                <div>
                  <dt>Rôle</dt>
                  <dd>{roleLabels[state.account.role]}</dd>
                </div>
                <div>
                  <dt>Membre depuis</dt>
                  <dd>{dateFormat.format(new Date(state.account.createdAt))}</dd>
                </div>
                <div>
                  <dt>Dernière connexion</dt>
                  <dd>{dateTimeFormat.format(new Date(state.account.lastLoginAt))}</dd>
                </div>
              </dl>
            </section>

            {state.accounts && (
              <section className="section" aria-labelledby="accounts-title">
                <h2 id="accounts-title">
                  Comptes enregistrés <span className="count">{state.accounts.length}</span>
                </h2>
                <div className="table-scroll">
                  <table aria-label="Comptes enregistrés">
                    <thead>
                      <tr>
                        <th scope="col">Nom</th>
                        <th scope="col">Email</th>
                        <th scope="col">Rôle</th>
                        <th scope="col">Inscription</th>
                      </tr>
                    </thead>
                    <tbody>
                      {state.accounts.map((account) => (
                        <tr key={account.id}>
                          <td>{account.name ?? '—'}</td>
                          <td>{account.email ?? '—'}</td>
                          <td>{roleLabels[account.role]}</td>
                          <td>{dateFormat.format(new Date(account.createdAt))}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </section>
            )}
          </>
        )}
      </main>
    </div>
  );
}
