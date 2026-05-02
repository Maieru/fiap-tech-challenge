import { useState, type FormEvent } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { UserPlus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAuth } from "@/hooks/useAuth";
import { getApiErrorMessage } from "@/services/api";
import { authService } from "@/services/auth.service";

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isRegisterOpen, setIsRegisterOpen] = useState(false);
  const [isRegistering, setIsRegistering] = useState(false);
  const [usuario, setUsuario] = useState("");
  const [senha, setSenha] = useState("");
  const [novoUsuario, setNovoUsuario] = useState("");
  const [novaSenha, setNovaSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);

    try {
      await login({ usuario, senha });
      toast.success("Login realizado com sucesso.");
      const redirectTo = location.state?.from ?? "/";
      navigate(redirectTo, { replace: true });
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao autenticar."));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleRegisterSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (novaSenha !== confirmarSenha) {
      toast.error("As senhas nao conferem.");
      return;
    }

    setIsRegistering(true);

    try {
      const trimmedUsuario = novoUsuario.trim();
      await authService.register({ usuario: trimmedUsuario, senha: novaSenha });
      toast.success("Usuario cadastrado com sucesso.");
      setUsuario(trimmedUsuario);
      setSenha("");
      setNovoUsuario("");
      setNovaSenha("");
      setConfirmarSenha("");
      setIsRegisterOpen(false);
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Falha ao cadastrar usuario."));
    } finally {
      setIsRegistering(false);
    }
  }

  function handleRegisterOpenChange(open: boolean) {
    setIsRegisterOpen(open);

    if (!open) {
      setNovoUsuario("");
      setNovaSenha("");
      setConfirmarSenha("");
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-gradient-to-b from-slate-100 to-slate-200/80 px-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Oficina Mecânica</CardTitle>
          <CardDescription>Acesse o painel administrativo para gerenciar clientes, serviços e ordens.</CardDescription>
        </CardHeader>
        <CardContent>
          <form className="space-y-4" onSubmit={handleSubmit}>
            <div className="space-y-2">
              <Label htmlFor="usuario">Usuário</Label>
              <Input
                id="usuario"
                placeholder="admin"
                value={usuario}
                onChange={(event) => setUsuario(event.target.value)}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="senha">Senha</Label>
              <Input
                id="senha"
                type="password"
                placeholder="••••••••"
                value={senha}
                onChange={(event) => setSenha(event.target.value)}
                required
              />
            </div>

            <Button className="w-full" type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Entrando..." : "Entrar"}
            </Button>

            <Button
              className="w-full"
              type="button"
              variant="outline"
              onClick={() => setIsRegisterOpen(true)}
              disabled={isSubmitting}
            >
              <UserPlus className="h-4 w-4" />
              Cadastrar
            </Button>
          </form>
        </CardContent>
      </Card>

      <Dialog open={isRegisterOpen} onOpenChange={handleRegisterOpenChange}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Criar usuario</DialogTitle>
            <DialogDescription>Cadastre um novo acesso para entrar no painel administrativo.</DialogDescription>
          </DialogHeader>

          <form className="space-y-4" onSubmit={handleRegisterSubmit}>
            <div className="space-y-2">
              <Label htmlFor="novoUsuario">Usuario</Label>
              <Input
                id="novoUsuario"
                placeholder="novo.usuario"
                value={novoUsuario}
                onChange={(event) => setNovoUsuario(event.target.value)}
                required
                minLength={3}
                disabled={isRegistering}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="novaSenha">Senha</Label>
              <Input
                id="novaSenha"
                type="password"
                placeholder="********"
                value={novaSenha}
                onChange={(event) => setNovaSenha(event.target.value)}
                required
                minLength={6}
                disabled={isRegistering}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="confirmarSenha">Confirmar senha</Label>
              <Input
                id="confirmarSenha"
                type="password"
                placeholder="********"
                value={confirmarSenha}
                onChange={(event) => setConfirmarSenha(event.target.value)}
                required
                minLength={6}
                disabled={isRegistering}
              />
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => handleRegisterOpenChange(false)}
                disabled={isRegistering}
              >
                Cancelar
              </Button>
              <Button type="submit" disabled={isRegistering}>
                {isRegistering ? "Cadastrando..." : "Criar conta"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </main>
  );
}
