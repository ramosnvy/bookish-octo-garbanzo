import { Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "../context/AuthContext";
import LoginPage from "../pages/LoginPage";
import DashboardPage from "../pages/DashboardPage";
import ClientesPage from "../pages/ClientesPage";
import FornecedoresPage from "../pages/FornecedoresPage";
import FornecedorCategoriasPage from "../pages/FornecedorCategoriasPage";
import ProdutosPage from "../pages/ProdutosPage";
import UsersPage from "../pages/UsersPage";
import EmpresasPage from "../pages/EmpresasPage";
import ContasPagarPage from "../pages/ContasPagarPage";
import ContasReceberPage from "../pages/ContasReceberPage";
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
        path="/fornecedores/categorias"
        element={
          <ProtectedRoute>
            <FornecedorCategoriasPage />
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
        path="/financeiro/contas-pagar"
        element={
          <ProtectedRoute>
            <ContasPagarPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/financeiro/contas-receber"
        element={
          <ProtectedRoute>
            <ContasReceberPage />
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  </AuthProvider>
);

export default App;
