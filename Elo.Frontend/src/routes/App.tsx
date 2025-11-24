import { Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "../context/AuthContext";
import LoginPage from "../pages/LoginPage";
import DashboardPage from "../pages/DashboardPage";
import ClientesPage from "../pages/ClientesPage";
import FornecedoresPage from "../pages/FornecedoresPage";
import ProdutosPage from "../pages/ProdutosPage";
import UsersPage from "../pages/UsersPage";
import EmpresasPage from "../pages/EmpresasPage";
import FinanceiroPage from "../pages/FinanceiroPage";
import FornecedorCategoriasPage from "../pages/FornecedorCategoriasPage";
import HistoriasPage from "../pages/HistoriasPage";
import ProtectedRoute from "./ProtectedRoute";

const App = () => (
  <AuthProvider>
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <DashboardPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/fornecedores/categorias"
        element={
          <ProtectedRoute>
            <FornecedorCategoriasPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/clientes"
        element={
          <ProtectedRoute>
            <ClientesPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/fornecedores"
        element={
          <ProtectedRoute>
            <FornecedoresPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/produtos"
        element={
          <ProtectedRoute>
            <ProdutosPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/historias"
        element={
          <ProtectedRoute>
            <HistoriasPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/usuarios"
        element={
          <ProtectedRoute>
            <UsersPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/empresas"
        element={
          <ProtectedRoute>
            <EmpresasPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/financeiro"
        element={
          <ProtectedRoute>
            <FinanceiroPage />
          </ProtectedRoute>
        }
      />
      <Route path="/financeiro/contas-pagar" element={<Navigate to="/financeiro" replace />} />
      <Route path="/financeiro/contas-receber" element={<Navigate to="/financeiro" replace />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  </AuthProvider>
);

export default App;
