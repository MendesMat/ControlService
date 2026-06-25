import { UserCircle, Bell } from "lucide-react";

export default function Header() {
  return (
    <header className="h-16 bg-surface border-b border-border px-8 flex items-center justify-end shrink-0">
      <div className="flex items-center gap-4">
        <button
          aria-label="Notificações"
          className="text-content-secondary hover:text-content-primary transition-colors p-2 rounded-lg hover:bg-surface-app"
        >
          <Bell size={22} />
        </button>

        <button
          aria-label="Perfil do usuário"
          className="flex items-center gap-2 text-content-secondary hover:text-content-primary transition-colors"
        >
          <UserCircle size={32} />
        </button>
      </div>
    </header>
  );
}
