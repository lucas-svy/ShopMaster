import type { ReactNode } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { useAuth0 } from '@auth0/auth0-react';
import { HomePage } from './pages/HomePage';
import { LoginPage } from './pages/LoginPage';
import { SignupPage } from './pages/SignupPage';

function Loading() {
  return (
    <p className="status status--page" role="status">
      Chargement…
    </p>
  );
}

/** Page réservée aux utilisateurs connectés. */
function RequireAuth({ children }: { children: ReactNode }) {
  const { isLoading, isAuthenticated } = useAuth0();
  if (isLoading) return <Loading />;
  return isAuthenticated ? children : <Navigate to="/connexion" replace />;
}

/** Page de connexion / inscription : inutile si l'on est déjà connecté. */
function GuestOnly({ children }: { children: ReactNode }) {
  const { isLoading, isAuthenticated } = useAuth0();
  if (isLoading) return <Loading />;
  return isAuthenticated ? <Navigate to="/" replace /> : children;
}

export function App() {
  return (
    <Routes>
      <Route path="/" element={<RequireAuth><HomePage /></RequireAuth>} />
      <Route path="/connexion" element={<GuestOnly><LoginPage /></GuestOnly>} />
      <Route path="/inscription" element={<GuestOnly><SignupPage /></GuestOnly>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
