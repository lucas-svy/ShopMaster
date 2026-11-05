import { useAuth0 } from '@auth0/auth0-react';
import { Link } from 'react-router-dom';
import { AuthLayout } from '../components/AuthLayout';

export function LoginPage() {
  const { loginWithRedirect, error } = useAuth0();

  return (
    <AuthLayout>
      <h1>Connexion</h1>
      <p className="lead">Accédez à votre compte ShopMaster.</p>
      {error && (
        <p className="notice notice--error" role="alert">
          La connexion a échoué : {error.message}
        </p>
      )}
      <button type="button" className="button" onClick={() => loginWithRedirect({ appState: { returnTo: '/' } })}>
        Se connecter
      </button>
      <p className="hint">Vous serez redirigé vers la page sécurisée d'Auth0.</p>
      <p className="switch">
        Pas encore de compte ? <Link to="/inscription">Créer un compte</Link>
      </p>
    </AuthLayout>
  );
}
