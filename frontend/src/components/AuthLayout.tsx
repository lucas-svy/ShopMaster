import type { ReactNode } from 'react';
import { BrandTag } from './BrandTag';

/** Mise en page commune aux pages de connexion et d'inscription. */
export function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <div className="auth">
      <div className="auth__brand">
        <BrandTag size="large" />
      </div>
      <main className="auth__panel">{children}</main>
    </div>
  );
}
