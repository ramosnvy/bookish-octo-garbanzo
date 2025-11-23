import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const LoginPage = () => {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [email, setEmail] = useState("admin@elo.com");
  const [password, setPassword] = useState("123456");
  const [remember, setRemember] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    try {
      await login(email, password);
      navigate("/");
    } catch {
      setError("Credenciais inválidas");
    }
  };

  return (
    <div className="auth-wrapper">
      <div className="auth-card">
        <div className="auth-logo">BH</div>
        <h2>BusinessHub</h2>
        <p className="muted">Plataforma de Gestão</p>
        <h3>Bem-vindo de volta</h3>
        <p className="muted">Entre na sua conta para continuar</p>

        <form className="auth-form" onSubmit={handleSubmit}>
          {error && <p className="error">{error}</p>}
          <label>Endereço de e-mail</label>
          <input type="email" placeholder="voce@empresa.com" value={email} onChange={(e) => setEmail(e.target.value)} required />

          <label>Senha</label>
          <input
            type="password"
            placeholder="Digite sua senha"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />

          <div className="auth-form-row">
            <label className="checkbox">
              <input type="checkbox" checked={remember} onChange={(e) => setRemember(e.target.checked)} />
              Lembrar de mim
            </label>
            <a className="link" href="#">
              Esqueceu a senha?
            </a>
          </div>

          <button type="submit" className="primary">
            Entrar
          </button>
        </form>
        <p className="muted">
          Não tem uma conta? <a className="link" href="#">Fale com o administrador</a>
        </p>
      </div>
      <p className="auth-footer">© {new Date().getFullYear()} BusinessHub. Todos os direitos reservados.</p>
    </div>
  );
};

export default LoginPage;
