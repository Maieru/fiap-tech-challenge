import { useMemo, useState } from "react";
import {
  CarFront,
  ClipboardList,
  LayoutDashboard,
  LogOut,
  Menu,
  PackageSearch,
  UserRound,
  Wrench,
  X,
} from "lucide-react";
import { NavLink, Outlet, useLocation } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/hooks/useAuth";
import { cn } from "@/lib/utils";

const menuItems = [
  { to: "/", label: "Dashboard", icon: LayoutDashboard },
  { to: "/clientes", label: "Clientes", icon: UserRound },
  { to: "/veiculos", label: "Veículos", icon: CarFront },
  { to: "/servicos", label: "Serviços", icon: Wrench },
  { to: "/pecas-insumos", label: "Peças e Insumos", icon: PackageSearch },
  { to: "/ordens-servico", label: "Ordens de Serviço", icon: ClipboardList },
];

const routeTitleMap: Record<string, string> = {
  "/": "Painel Administrativo",
  "/clientes": "Gestão de Clientes",
  "/veiculos": "Gestão de Veículos",
  "/servicos": "Gestão de Serviços",
  "/pecas-insumos": "Estoque de Peças e Insumos",
  "/ordens-servico": "Ordens de Serviço",
};

export function AdminLayout() {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const { session, logout } = useAuth();
  const location = useLocation();

  const currentTitle = useMemo(() => {
    const matchedPrefix = Object.keys(routeTitleMap).find((route) =>
      route === "/" ? location.pathname === route : location.pathname.startsWith(route),
    );
    return matchedPrefix ? routeTitleMap[matchedPrefix] : "Sistema da Oficina";
  }, [location.pathname]);

  return (
    <div className="flex min-h-screen bg-gradient-to-b from-slate-50 to-slate-100/90">
      <aside className="hidden w-64 border-r bg-white/80 px-4 py-6 lg:block">
        <div className="mb-8">
          <p className="text-xs uppercase tracking-wide text-muted-foreground">Oficina</p>
          <h2 className="text-xl font-semibold">Mecânica Pro</h2>
        </div>
        <nav className="space-y-1">
          {menuItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to === "/"}
                className={({ isActive }) =>
                  cn(
                    "flex items-center gap-3 rounded-md px-3 py-2 text-sm transition-colors",
                    isActive ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:bg-muted",
                  )
                }
              >
                <Icon className="h-4 w-4" />
                {item.label}
              </NavLink>
            );
          })}
        </nav>
      </aside>

      {isMobileMenuOpen && (
        <div className="fixed inset-0 z-40 bg-black/40 lg:hidden" onClick={() => setIsMobileMenuOpen(false)} />
      )}
      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-50 w-64 border-r bg-white p-4 shadow-xl transition-transform lg:hidden",
          isMobileMenuOpen ? "translate-x-0" : "-translate-x-full",
        )}
      >
        <div className="mb-6 flex items-center justify-between">
          <h2 className="text-lg font-semibold">Mecânica Pro</h2>
          <Button size="icon" variant="ghost" onClick={() => setIsMobileMenuOpen(false)}>
            <X className="h-4 w-4" />
          </Button>
        </div>
        <nav className="space-y-1">
          {menuItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to === "/"}
                onClick={() => setIsMobileMenuOpen(false)}
                className={({ isActive }) =>
                  cn(
                    "flex items-center gap-3 rounded-md px-3 py-2 text-sm transition-colors",
                    isActive ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:bg-muted",
                  )
                }
              >
                <Icon className="h-4 w-4" />
                {item.label}
              </NavLink>
            );
          })}
        </nav>
      </aside>

      <div className="flex min-h-screen flex-1 flex-col">
        <header className="sticky top-0 z-20 border-b bg-white/90 backdrop-blur">
          <div className="flex items-center justify-between px-4 py-3 lg:px-8">
            <div className="flex items-center gap-2">
              <Button size="icon" variant="ghost" className="lg:hidden" onClick={() => setIsMobileMenuOpen(true)}>
                <Menu className="h-5 w-5" />
              </Button>
              <div>
                <p className="text-xs text-muted-foreground">Painel</p>
                <h1 className="text-base font-semibold">{currentTitle}</h1>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <span className="hidden text-sm text-muted-foreground sm:inline">{session?.username ?? "Administrador"}</span>
              <Button variant="outline" size="sm" onClick={logout}>
                <LogOut className="mr-1 h-4 w-4" />
                Sair
              </Button>
            </div>
          </div>
        </header>

        <main className="flex-1 px-4 py-6 lg:px-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
