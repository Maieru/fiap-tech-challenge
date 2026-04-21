import { Navigate, Outlet, useLocation } from "react-router-dom";
import { LoadingState } from "@/components/common/LoadingState";
import { useAuth } from "@/hooks/useAuth";

export function PrivateRoute() {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) return <LoadingState message="Validando sessão..." />;

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />;
  }

  return <Outlet />;
}
