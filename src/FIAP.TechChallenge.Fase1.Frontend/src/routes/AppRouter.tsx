import { Navigate, Route, Routes } from "react-router-dom";
import { AdminLayout } from "@/layouts/AdminLayout";
import { LoginPage } from "@/pages/auth/LoginPage";
import { ClientesListPage } from "@/pages/clientes/ClientesListPage";
import { ClienteDetailsPage } from "@/pages/clientes/ClienteDetailsPage";
import { ClienteFormPage } from "@/pages/clientes/ClienteFormPage";
import { DashboardPage } from "@/pages/dashboard/DashboardPage";
import { OrdensServicoListPage } from "@/pages/ordens-servico/OrdensServicoListPage";
import { OrdemServicoDetailsPage } from "@/pages/ordens-servico/OrdemServicoDetailsPage";
import { OrdemServicoFormPage } from "@/pages/ordens-servico/OrdemServicoFormPage";
import { PecaInsumoDetailsPage } from "@/pages/pecas-insumos/PecaInsumoDetailsPage";
import { PecaInsumoFormPage } from "@/pages/pecas-insumos/PecaInsumoFormPage";
import { PecasInsumosListPage } from "@/pages/pecas-insumos/PecasInsumosListPage";
import { ServicoFormPage } from "@/pages/servicos/ServicoFormPage";
import { ServicosListPage } from "@/pages/servicos/ServicosListPage";
import { VeiculoDetailsPage } from "@/pages/veiculos/VeiculoDetailsPage";
import { VeiculoFormPage } from "@/pages/veiculos/VeiculoFormPage";
import { VeiculosListPage } from "@/pages/veiculos/VeiculosListPage";
import { useAuth } from "@/hooks/useAuth";
import { PrivateRoute } from "@/routes/PrivateRoute";

function LoginRoute() {
  const { isAuthenticated } = useAuth();
  if (isAuthenticated) return <Navigate to="/" replace />;
  return <LoginPage />;
}

export function AppRouter() {
  return (
    <Routes>
      <Route path="/login" element={<LoginRoute />} />

      <Route element={<PrivateRoute />}>
        <Route element={<AdminLayout />}>
          <Route index element={<DashboardPage />} />

          <Route path="/clientes" element={<ClientesListPage />} />
          <Route path="/clientes/novo" element={<ClienteFormPage />} />
          <Route path="/clientes/:id" element={<ClienteDetailsPage />} />
          <Route path="/clientes/:id/editar" element={<ClienteFormPage />} />

          <Route path="/veiculos" element={<VeiculosListPage />} />
          <Route path="/veiculos/novo" element={<VeiculoFormPage />} />
          <Route path="/veiculos/:id" element={<VeiculoDetailsPage />} />
          <Route path="/veiculos/:id/editar" element={<VeiculoFormPage />} />

          <Route path="/servicos" element={<ServicosListPage />} />
          <Route path="/servicos/novo" element={<ServicoFormPage />} />
          <Route path="/servicos/:id/editar" element={<ServicoFormPage />} />

          <Route path="/pecas-insumos" element={<PecasInsumosListPage />} />
          <Route path="/pecas-insumos/novo" element={<PecaInsumoFormPage />} />
          <Route path="/pecas-insumos/:id" element={<PecaInsumoDetailsPage />} />
          <Route path="/pecas-insumos/:id/editar" element={<PecaInsumoFormPage />} />

          <Route path="/ordens-servico" element={<OrdensServicoListPage />} />
          <Route path="/ordens-servico/nova" element={<OrdemServicoFormPage />} />
          <Route path="/ordens-servico/:id" element={<OrdemServicoDetailsPage />} />
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
