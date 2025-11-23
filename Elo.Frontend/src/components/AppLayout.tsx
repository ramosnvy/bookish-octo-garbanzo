import { ReactNode, useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { EmpresaService } from "../services/EmpresaService";
import { EmpresaDto } from "../models";

interface AppLayoutProps {
  title: string;
  subtitle?: string;
  actions?: ReactNode;
  children: ReactNode;
}

interface NavItem {
  label: string;
  icon: JSX.Element;
  to: string;
  disabled?: boolean;
  children?: NavItem[];
  roles?: string[];
  requiresGlobalAdmin?: boolean;
}

const IconDashboard = () => (
  <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="1.8">
    <rect x="3" y="3" width="7" height="9" rx="2" />
    <rect x="14" y="3" width="7" height="5" rx="2" />
    <rect x="14" y="11" width="7" height="10" rx="2" />
    <rect x="3" y="14" width="7" height="7" rx="2" />
  </svg>
);
const IconClients = () => (
  <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="1.8">
    <circle cx="8" cy="7" r="3" />
    <circle cx="17" cy="9" r="2.5" />
    <path d="M3 19a4 4 0 0 1 4-4h2a4 4 0 0 1 4 4v1H3z" />
    <path d="M15 19c0-1.8.9-3.2 2.5-3.6" />
  </svg>
);
const IconSuppliers = () => (
  <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="1.8">
    <path d="M4 10h16" />
    <path d="M4 14h16" />
    <rect x="5" y="5" width="4" height="4" rx="1" />
    <rect x="15" y="5" width="4" height="4" rx="1" />
    <rect x="5" y="15" width="4" height="4" rx="1" />
    <rect x="15" y="15" width="4" height="4" rx="1" />
  </svg>
);

const IconCategories = () => (
  <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="1.8">
    <path d="M4 7h16" />
    <path d="M4 12h16" />
    <path d="M4 17h16" />
    <circle cx="7" cy="7" r="1.4" />
    <circle cx="7" cy="12" r="1.4" />
    <circle cx="7" cy="17" r="1.4" />
  </svg>
);

const IconUsers = () => (
  <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="1.8">
    <circle cx="9" cy="7" r="3" />
    <circle cx="17" cy="9" r="2.5" />
    <path d="M3 21a6 6 0 0 1 12 0" />
    <path d="M14.5 19c0-1.8.9-3.2 2.5-3.6" />
  </svg>
);

const IconFinance = () => (
  <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="1.8">
    <path d="M4 19h16" />
    <path d="M4 14h16" />
    <path d="M4 9h16" />
    <path d="M8 5l4-2 4 2" />
  </svg>
);

const navItems: NavItem[] = [
  { label: "Painel", icon: <IconDashboard />, to: "/" },
  { label: "Clientes", icon: <IconClients />, to: "/clientes" },
  {
    label: "Produtos",
    icon: (
      <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="1.8">
        <rect x="4" y="4" width="16" height="6" rx="2" />
        <rect x="4" y="14" width="16" height="6" rx="2" />
        <path d="M9 4v16" />
      </svg>
    ),
    to: "/produtos",
  },
  {
    label: "Fornecedores",
    icon: <IconSuppliers />,
    to: "/fornecedores",
    children: [
      {
        label: "Categorias",
        icon: <IconCategories />,
        to: "/fornecedores/categorias",
      },
    ],
  },
  {
    label: "Usuários",
    icon: <IconUsers />,
    to: "/usuarios",
    roles: ["admin", "manager"],
  },
  {
    label: "Empresas",
    icon: <IconUsers />,
    to: "/empresas",
    roles: ["admin"],
    requiresGlobalAdmin: true,
  },
  {
    label: "Financeiro",
    icon: <IconFinance />,
    to: "/financeiro/contas-pagar",
    children: [
      { label: "Contas a pagar", icon: <IconFinance />, to: "/financeiro/contas-pagar" },
      { label: "Contas a receber", icon: <IconFinance />, to: "/financeiro/contas-receber" },
    ],
  },
];

const getInitialExpanded = (): Record<string, boolean> => {
  if (typeof window === "undefined") {
    return {};
  }

  try {
    const stored = window.localStorage.getItem("sidebar:expanded");
    return stored ? (JSON.parse(stored) as Record<string, boolean>) : {};
  } catch {
    return {};
  }
};

const UserIcon = () => (
  <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="#1d4ed8" strokeWidth="1.8">
    <circle cx="12" cy="8" r="4" />
    <path d="M5 21a7 7 0 0 1 14 0" />
  </svg>
);

const AppLayout = ({ children, actions }: AppLayoutProps) => {
  const location = useLocation();
  const { user, logout, selectedEmpresaId, setSelectedEmpresaId } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [expandedParents, setExpandedParents] = useState<Record<string, boolean>>(getInitialExpanded);
  const [empresas, setEmpresas] = useState<EmpresaDto[]>([]);

  const toggleMenu = () => setMenuOpen((prev) => !prev);
  const closeMenu = () => setMenuOpen(false);
  const toggleSidebar = () => setSidebarCollapsed((prev) => !prev);
  const toggleMobileMenu = () => setMobileMenuOpen((prev) => !prev);
  const closeMobileMenu = () => setMobileMenuOpen(false);

  const normalizedRole = typeof user?.role === "string" ? user.role.toLowerCase() : String(user?.role ?? "").toLowerCase();

  useEffect(() => {
    if (!user?.empresaId && user) {
      EmpresaService.getAll()
        .then(setEmpresas)
        .catch(() => setEmpresas([]));
    }
  }, [user]);

  useEffect(() => {
    if (typeof window === "undefined") {
      return;
    }

    try {
      window.localStorage.setItem("sidebar:expanded", JSON.stringify(expandedParents));
    } catch {
      // ignore storage errors
    }
  }, [expandedParents]);

  useEffect(() => {
    navItems.forEach((item) => {
      if (item.children && item.children.some((child) => location.pathname.startsWith(child.to))) {
        setExpandedParents((prev) => (prev[item.label] ? prev : { ...prev, [item.label]: true }));
      }
    });
  }, [location.pathname]);

  return (
    <div className={`app-shell ${sidebarCollapsed ? "collapsed" : ""} ${mobileMenuOpen ? "mobile-menu-open" : ""}`}>
      {mobileMenuOpen && <div className="sidebar-overlay" onClick={closeMobileMenu} />}
      <aside className={`sidebar ${mobileMenuOpen ? "open" : ""}`}>
        <button className="collapse-button icon" onClick={toggleSidebar} aria-label="Alternar menu">
          {sidebarCollapsed ? (
            <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="#1d4ed8" strokeWidth="2">
              <path d="M9 6l6 6-6 6" />
            </svg>
          ) : (
            <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="#1d4ed8" strokeWidth="2">
              <path d="M15 6l-6 6 6 6" />
            </svg>
          )}
        </button>
        <nav className="nav">
          {navItems.map((item) => {
            if (item.roles && !item.roles.includes(normalizedRole)) {
              return null;
            }
            if (item.requiresGlobalAdmin && user?.empresaId) {
              return null;
            }
            const isParentActive = location.pathname === item.to || location.pathname.startsWith(`${item.to}/`);
            return (
              <div key={item.label} className="nav-group">
                <div className="nav-parent">
                  <Link
                    to={item.disabled ? "#" : item.to}
                    className={`nav-link ${isParentActive ? "active" : ""} ${item.disabled ? "disabled" : ""}`}
                    onClick={closeMobileMenu}
                  >
                    <span className="nav-icon">{item.icon}</span>
                    <span className="nav-label">{item.label}</span>
                  </Link>
                  {item.children && (
                    <button
                      type="button"
                      className={`collapse-toggle ${expandedParents[item.label] ? "open" : ""}`}
                      onClick={() =>
                        setExpandedParents((prev) => ({
                          ...prev,
                          [item.label]: !prev[item.label],
                        }))
                      }
                      aria-label="Alternar filhos"
                    >
                      <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="#1d4ed8" strokeWidth="2">
                        <path d="M7 9l5 5 5-5" />
                      </svg>
                    </button>
                  )}
                </div>
                {item.children && expandedParents[item.label] && (
                  <div className="nav-sub">
                    {item.children
                      .filter((child) => !child.roles || child.roles.includes(normalizedRole))
                      .map((child) => (
                        <Link
                          key={child.label}
                          to={child.disabled ? "#" : child.to}
                          className={`nav-link child ${
                            location.pathname === child.to ? "active" : ""
                          } ${child.disabled ? "disabled" : ""}`}
                          onClick={closeMobileMenu}
                        >
                          <span className="nav-icon">{child.icon}</span>
                          <span className="nav-label">{child.label}</span>
                        </Link>
                      ))}
                  </div>
                )}
              </div>
            );
          })}
        </nav>
      </aside>

      <main className="main-panel">
        <header className="topbar">
          <button className="menu-button" onClick={toggleMobileMenu} aria-label="Abrir menu">
            {mobileMenuOpen ? (
              <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="#1d4ed8" strokeWidth="2">
                <path d="M6 6l12 12M6 18L18 6" />
              </svg>
            ) : (
              <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="#1d4ed8" strokeWidth="2">
                <path d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            )}
          </button>
          <div className="topbar-actions">
            {actions}
            {!user?.empresaId && (
              <div className="company-filter">
                <label htmlFor="company-filter-select">Empresa</label>
                <div className="select-wrapper">
                  <select
                    id="company-filter-select"
                    value={selectedEmpresaId ?? ""}
                    onChange={(event) =>
                      setSelectedEmpresaId(event.target.value ? Number(event.target.value) : null)
                    }
                  >
                    <option value="">Todas</option>
                    {empresas.map((empresa) => (
                      <option key={empresa.id} value={empresa.id}>
                        {empresa.nome}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
            )}
            <div className={`user-pill ${menuOpen ? "open" : ""}`} onClick={toggleMenu} title={user?.nome}>
              {user?.avatarUrl ? (
                <div className="avatar image">
                  <img src={user.avatarUrl} alt={user.nome} />
                </div>
              ) : (
                <div className="avatar icon">
                  <UserIcon />
                </div>
              )}
              <div className={`user-menu ${menuOpen ? "visible" : ""}`}>
                <button type="button" className="ghost full" onClick={closeMenu}>
                  {user?.role ? `Perfil ÃƒÆ’Ã‚Â¯Ãƒâ€šÃ‚Â¿Ãƒâ€šÃ‚Â½ ${user.role}` : "Perfil"}
                </button>
                <button type="button" className="ghost full">
                  Alterar tema
                </button>
                {normalizedRole === "admin" && (
                  <button type="button" className="ghost full">
                    PermissÃƒÆ’Ã‚Â¯Ãƒâ€šÃ‚Â¿Ãƒâ€šÃ‚Â½es
                  </button>
                )}
                <button type="button" className="danger full" onClick={logout}>
                  Sair
                </button>
              </div>
            </div>
          </div>
        </header>
        <div className="content">{children}</div>
      </main>
    </div>
  );
};

export default AppLayout;

