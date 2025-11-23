import { createContext, useContext, useState, ReactNode, useEffect } from "react";
import { AuthService } from "../services/AuthService";

interface AuthUserInfo {
  nome: string;
  email: string;
  role: string;
  empresaId?: number | null;
  avatarUrl?: string;
}

interface AuthContextState {
  token: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  user?: AuthUserInfo | null;
  selectedEmpresaId: number | null;
  setSelectedEmpresaId: (empresaId: number | null) => void;
}

const AuthContext = createContext<AuthContextState | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem("token"));
  const [user, setUser] = useState<AuthContextState["user"]>(null);
  const [selectedEmpresaId, setSelectedEmpresaId] = useState<number | null>(() => {
    const stored = localStorage.getItem("selectedEmpresaId");
    return stored ? Number(stored) : null;
  });

  useEffect(() => {
    if (token) {
      AuthService.me(token)
        .then((data) =>
          setUser({ nome: data.nome, email: data.email, role: data.role, empresaId: data.empresaId })
        )
        .catch(() => {
          setToken(null);
          setUser(null);
          localStorage.removeItem("token");
        });
    }
  }, [token]);

  useEffect(() => {
    if (user?.empresaId) {
      setSelectedEmpresaId(user.empresaId);
      localStorage.setItem("selectedEmpresaId", String(user.empresaId));
    } else if (!user) {
      setSelectedEmpresaId(null);
      localStorage.removeItem("selectedEmpresaId");
    }
  }, [user]);

  useEffect(() => {
    if (!user?.empresaId) {
      if (selectedEmpresaId !== null && selectedEmpresaId !== undefined) {
        localStorage.setItem("selectedEmpresaId", String(selectedEmpresaId));
      } else {
        localStorage.removeItem("selectedEmpresaId");
      }
    }
  }, [selectedEmpresaId, user?.empresaId]);

  const login = async (email: string, password: string) => {
    const response = await AuthService.login(email, password);
    setToken(response.token);
    localStorage.setItem("token", response.token);
    setUser({ nome: response.nome, email: response.email, role: response.role, empresaId: response.empresaId });
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    setSelectedEmpresaId(null);
    localStorage.removeItem("token");
    localStorage.removeItem("selectedEmpresaId");
  };

  return (
    <AuthContext.Provider
      value={{ token, user, isAuthenticated: !!token, login, logout, selectedEmpresaId, setSelectedEmpresaId }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
