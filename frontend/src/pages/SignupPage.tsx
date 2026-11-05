import { useAuth0 } from '@auth0/auth0-react';
import { Link } from 'react-router-dom';
import { AuthLayout } from '../components/AuthLayout';

export function SignupPage() {
  const { loginWithRedirect, error } = useAuth0();

  return (
    <AuthLayout>
      <h1>Créer un compte</h1>
      <p className="lead">Une adresse email et un mot de passe suffisent.</p>
      {error && (
        <p className="notice notice--error" role="alert">
          L'inscription a échoué : {error.message}
        </p>
      )}
      <button
        type="button"
        className="button"
        onClick={() =>
          loginWithRedirect({ authorizationParams: { screen_hint: 'signup' }, appState: { returnTo: '/' } })
        }
      >
        Créer mon compte
      </button>
      <p className="hint">Le formulaire d'inscription s'ouvre sur la page sécurisée d'Auth0.</p>
      <p className="switch">
        Déjà inscrit ? <Link to="/connexion">Se connecter</Link>
      </p>
    </AuthLayout>
  );
}
