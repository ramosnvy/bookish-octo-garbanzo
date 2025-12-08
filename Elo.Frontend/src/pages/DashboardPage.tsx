import { useEffect, useState } from "react";
import AppLayout from "../components/AppLayout";
import { useAuth } from "../context/AuthContext";
import {
  ClienteDto,
  ContaPagarDto,
  ContaReceberDto,
  ContaStatus,
  FornecedorDto,
  HistoriaDto,
  ProdutoDto,
  TicketDto,
  TicketPrioridade,
  TicketStatus,
} from "../models";
import { ClienteService } from "../services/ClienteService";
import { FornecedorService } from "../services/FornecedorService";
import { ProdutoService } from "../services/ProdutoService";
import { ContasReceberService, ContasPagarService } from "../services/FinanceiroService";
import { TicketService } from "../services/TicketService";
import { HistoriaService } from "../services/HistoriaService";
import { getApiErrorMessage } from "../utils/apiError";

type FinanceEntry = {
  key: string;
  descricao: string;
  nome: string;
  valor: number;
  data?: string | null;
  status: ContaStatus;
};

type DashboardState = {
  totalClientes: number;
  clientesAtivos: number;
  totalFornecedores: number;
  fornecedoresAtivos: number;
  produtosAtivos: number;
  produtosTotal: number;
  historiasAbertas: number;
  ticketsAbertos: number;
  receitaPendente: number;
  pagamentosPendentes: number;
  proximasReceitas: FinanceEntry[];
  proximosPagamentos: FinanceEntry[];
  ticketsCriticos: TicketDto[];
  historiaStatusResumo: Array<{ id: number; nome: string; cor?: string | null; total: number }>;
};

const initialState: DashboardState = {
  totalClientes: 0,
  clientesAtivos: 0,
  totalFornecedores: 0,
  fornecedoresAtivos: 0,
  produtosAtivos: 0,
  produtosTotal: 0,
  historiasAbertas: 0,
  ticketsAbertos: 0,
  receitaPendente: 0,
  pagamentosPendentes: 0,
  proximasReceitas: [],
  proximosPagamentos: [],
  ticketsCriticos: [],
  historiaStatusResumo: [],
};

const currencyFormatter = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

const statusNameToValue: Record<string, number> = {
  ativo: 1,
  inativo: 2,
  suspenso: 3,
  cancelado: 4,
};

const normalizeStatusValue = (value: number | string | undefined | null): number => {
  if (typeof value === "number") {
    return value;
  }
  if (typeof value === "string") {
    const parsed = Number(value);
    if (!Number.isNaN(parsed)) {
      return parsed;
    }
    const normalized = value.trim().toLowerCase();
    return statusNameToValue[normalized] ?? 0;
  }
  return 0;
};

const isActiveStatus = (value: number | string | undefined | null) => normalizeStatusValue(value) === 1;

const priorityLabels: Record<TicketPrioridade, string> = {
  [TicketPrioridade.Baixa]: "Baixa",
  [TicketPrioridade.Media]: "Média",
  [TicketPrioridade.Alta]: "Alta",
  [TicketPrioridade.Critica]: "Crítica",
};

const ticketStatusLabels: Record<TicketStatus, string> = {
  [TicketStatus.Aberto]: "Aberto",
  [TicketStatus.EmAndamento]: "Em andamento",
  [TicketStatus.PendenteCliente]: "Pendente cliente",
  [TicketStatus.Resolvido]: "Resolvido",
  [TicketStatus.Fechado]: "Fechado",
  [TicketStatus.Cancelado]: "Cancelado",
};

const pendingStatuses = new Set<ContaStatus>([ContaStatus.Pendente, ContaStatus.Vencido]);
const openTicketStatuses = new Set<TicketStatus>([
  TicketStatus.Aberto,
  TicketStatus.EmAndamento,
  TicketStatus.PendenteCliente,
]);

const formatDateLabel = (value?: string | null) =>
  value ? new Date(value).toLocaleDateString("pt-BR", { day: "2-digit", month: "short" }) : "-";

const buildReceberEntries = (contas: ContaReceberDto[]): FinanceEntry[] =>
  contas.flatMap((conta) => {
    const parcelas =
      conta.parcelas && conta.parcelas.length
        ? conta.parcelas
        : [
            {
              id: conta.id,
              numero: 1,
              valor: conta.valor,
              dataVencimento: conta.dataVencimento,
              status: conta.status,
            },
          ];

    return parcelas.map((parcela) => ({
      key: `receber-${conta.id}-${parcela.id ?? parcela.numero}`,
      descricao: conta.descricao,
      nome: conta.clienteNome,
      valor: parcela.valor ?? conta.valor,
      data: parcela.dataVencimento ?? conta.dataVencimento,
      status: parcela.status ?? conta.status,
    }));
  });

const buildPagarEntries = (contas: ContaPagarDto[]): FinanceEntry[] =>
  contas.flatMap((conta) => {
    const parcelas =
      conta.parcelas && conta.parcelas.length
        ? conta.parcelas
        : [
            {
              id: conta.id,
              numero: 1,
              valor: conta.valor,
              dataVencimento: conta.dataVencimento,
              status: conta.status,
            },
          ];

    return parcelas.map((parcela) => ({
      key: `pagar-${conta.id}-${parcela.id ?? parcela.numero}`,
      descricao: conta.descricao,
      nome: conta.fornecedorNome,
      valor: parcela.valor ?? conta.valor,
      data: parcela.dataVencimento ?? conta.dataVencimento,
      status: parcela.status ?? conta.status,
    }));
  });

const sortByDate = (entries: FinanceEntry[]) =>
  entries
    .slice()
    .sort(
      (a, b) =>
        new Date(a.data ?? new Date()).getTime() - new Date(b.data ?? new Date()).getTime()
    );

const DashboardPage = () => {
  const { selectedEmpresaId } = useAuth();
  const [dashboard, setDashboard] = useState<DashboardState>(initialState);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    const loadDashboard = async () => {
      setLoading(true);
      setError(null);
      const empresaIdParam = selectedEmpresaId ?? undefined;

      const results = await Promise.allSettled([
        ClienteService.getAll(empresaIdParam),
        FornecedorService.getAll(empresaIdParam),
        ProdutoService.getAll(empresaIdParam),
        ContasReceberService.getAll({ empresaId: empresaIdParam }),
        ContasPagarService.getAll({ empresaId: empresaIdParam }),
        TicketService.getAll({ empresaId: empresaIdParam }),
        HistoriaService.getAll({ empresaId: empresaIdParam }),
      ]);

      if (cancelled) {
        return;
      }

      const [
        clientesResult,
        fornecedoresResult,
        produtosResult,
        contasReceberResult,
        contasPagarResult,
        ticketsResult,
        historiasResult,
      ] = results;

      const messages: string[] = [];

      const unwrap = <T,>(result: PromiseSettledResult<T>, fallback: T, label: string): T => {
        if (result.status === "fulfilled") {
          return result.value;
        }
        messages.push(getApiErrorMessage(result.reason, `Não foi possível carregar ${label}.`));
        return fallback;
      };

      const clientes = unwrap<ClienteDto[]>(clientesResult, [], "clientes");
      const fornecedores = unwrap<FornecedorDto[]>(fornecedoresResult, [], "fornecedores");
      const produtos = unwrap<ProdutoDto[]>(produtosResult, [], "produtos");
      const contasReceber = unwrap<ContaReceberDto[]>(
        contasReceberResult,
        [],
        "contas a receber"
      );
      const contasPagar = unwrap<ContaPagarDto[]>(contasPagarResult, [], "contas a pagar");
      const tickets = unwrap<TicketDto[]>(ticketsResult, [], "tickets");
      const historias = unwrap<HistoriaDto[]>(historiasResult, [], "histórias");

      const receberEntries = buildReceberEntries(contasReceber);
      const pagarEntries = buildPagarEntries(contasPagar);

      const receitaPendente = receberEntries.reduce(
        (total, entry) => (pendingStatuses.has(entry.status) ? total + entry.valor : total),
        0
      );
      const pagamentosPendentes = pagarEntries.reduce(
        (total, entry) => (pendingStatuses.has(entry.status) ? total + entry.valor : total),
        0
      );

      const proximasReceitas = sortByDate(
        receberEntries.filter((entry) => pendingStatuses.has(entry.status))
      ).slice(0, 5);

      const proximosPagamentos = sortByDate(
        pagarEntries.filter((entry) => pendingStatuses.has(entry.status))
      ).slice(0, 5);

      const openTickets = tickets.filter((ticket) => openTicketStatuses.has(ticket.status));
      const ticketsCriticos = openTickets
        .slice()
        .sort(
          (a, b) =>
            b.prioridade - a.prioridade ||
            new Date(a.dataAbertura).getTime() - new Date(b.dataAbertura).getTime()
        )
        .slice(0, 5);

      const historiaStatusResumo = Array.from(
        historias.reduce((map, historia) => {
          const current = map.get(historia.statusId);
          if (current) {
            current.total += 1;
            return map.set(historia.statusId, current);
          }
          return map.set(historia.statusId, {
            id: historia.statusId,
            nome: historia.statusNome || "Sem status",
            cor: historia.statusCor,
            total: 1,
          });
        }, new Map<number, { id: number; nome: string; cor?: string | null; total: number }>())
      )
        .sort((a, b) => b.total - a.total)
        .slice(0, 4);

      setDashboard({
        totalClientes: clientes.length,
        clientesAtivos: clientes.filter((cliente) => isActiveStatus(cliente.status)).length,
        totalFornecedores: fornecedores.length,
        fornecedoresAtivos: fornecedores.filter((fornecedor) => isActiveStatus(fornecedor.status)).length,
        produtosAtivos: produtos.filter((produto) => produto.ativo).length,
        produtosTotal: produtos.length,
        historiasAbertas: historias.filter(
          (historia) => !historia.statusFechaHistoria && !historia.dataFinalizacao
        ).length,
        ticketsAbertos: openTickets.length,
        receitaPendente,
        pagamentosPendentes,
        proximasReceitas,
        proximosPagamentos,
        ticketsCriticos,
        historiaStatusResumo,
      });

      setError(messages.length ? messages.join(" ") : null);
      setLoading(false);
    };

    loadDashboard();
    return () => {
      cancelled = true;
    };
  }, [selectedEmpresaId]);

  const stats = [
    {
      label: "Clientes",
      value: dashboard.totalClientes,
      detail: `${dashboard.clientesAtivos} ativos`,
    },
    {
      label: "Fornecedores",
      value: dashboard.totalFornecedores,
      detail: `${dashboard.fornecedoresAtivos} ativos`,
    },
    {
      label: "Produtos ativos",
      value: dashboard.produtosAtivos,
      detail: `${dashboard.produtosTotal} cadastrados`,
    },
    {
      label: "Histórias abertas",
      value: dashboard.historiasAbertas,
      detail: `${dashboard.historiaStatusResumo.length} status em uso`,
    },
    {
      label: "Tickets abertos",
      value: dashboard.ticketsAbertos,
      detail: `${dashboard.ticketsCriticos.length} críticos`,
    },
  ];

  return (
    <AppLayout title="Painel" subtitle="Visão consolidada da operação">
      {error && <div className="toast alert">{error}</div>}
      {loading ? (
        <section className="card">
          <p>Carregando painel...</p>
        </section>
      ) : (
        <>
          <section className="stats-grid">
            {stats.map((stat) => (
              <article key={stat.label} className="card stat-card">
                <p className="muted">{stat.label}</p>
                <h3>{stat.value}</h3>
                <small>{stat.detail}</small>
              </article>
            ))}
          </section>

          <section className="dashboard-grid">
            <article className="card dashboard-highlight">
              <header className="dashboard-highlight-header">
                <div>
                  <p className="modal-eyebrow">Financeiro</p>
                  <h3>Saúde do caixa</h3>
                </div>
                <span
                  className={`trend ${
                    dashboard.receitaPendente >= dashboard.pagamentosPendentes ? "trend-up" : "trend-down"
                  }`}
                >
                  {dashboard.receitaPendente >= dashboard.pagamentosPendentes
                    ? "Saldo positivo"
                    : "Atenção ao fluxo"}
                </span>
              </header>
              <div className="dashboard-metrics">
                <div className="dashboard-metric">
                  <p className="muted">Receitas pendentes</p>
                  <h4>{currencyFormatter.format(dashboard.receitaPendente)}</h4>
                  <small>{dashboard.proximasReceitas.length} parcelas aguardando</small>
                </div>
                <div className="dashboard-metric">
                  <p className="muted">Pagamentos pendentes</p>
                  <h4>{currencyFormatter.format(dashboard.pagamentosPendentes)}</h4>
                  <small>{dashboard.proximosPagamentos.length} compromissos</small>
                </div>
              </div>
            </article>

            <article className="card dashboard-highlight">
              <header className="dashboard-highlight-header">
                <div>
                  <p className="modal-eyebrow">Histórias</p>
                  <h3>Distribuição por status</h3>
                </div>
              </header>
              <div className="status-list">
                {dashboard.historiaStatusResumo.length === 0 ? (
                  <p className="muted">Nenhuma história encontrada.</p>
                ) : (
                  dashboard.historiaStatusResumo.map((status) => (
                    <div key={status.id} className="status-item">
                      <div className="status-pill">
                        <span
                          className="status-dot"
                          style={{ backgroundColor: status.cor ?? "#94a3b8" }}
                        />
                        {status.nome}
                      </div>
                      <strong>{status.total}</strong>
                    </div>
                  ))
                )}
              </div>
            </article>
          </section>

          <section className="dashboard-grid">
            <article className="card table-wrapper dashboard-table">
              <header className="dashboard-table-header">
                <div>
                  <p className="modal-eyebrow">Recebimentos</p>
                  <h3>Próximas parcelas a receber</h3>
                </div>
              </header>
              {dashboard.proximasReceitas.length === 0 ? (
                <p className="muted">Nenhum lançamento pendente.</p>
              ) : (
                <table>
                  <thead>
                    <tr>
                      <th>Cliente</th>
                      <th>Descrição</th>
                      <th>Vencimento</th>
                      <th>Valor</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dashboard.proximasReceitas.map((entry) => (
                      <tr key={entry.key}>
                        <td className="table-title">{entry.nome}</td>
                        <td>{entry.descricao}</td>
                        <td>{formatDateLabel(entry.data)}</td>
                        <td>{currencyFormatter.format(entry.valor)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </article>

            <article className="card table-wrapper dashboard-table">
              <header className="dashboard-table-header">
                <div>
                  <p className="modal-eyebrow">Pagamentos</p>
                  <h3>Compromissos a pagar</h3>
                </div>
              </header>
              {dashboard.proximosPagamentos.length === 0 ? (
                <p className="muted">Nenhum pagamento pendente.</p>
              ) : (
                <table>
                  <thead>
                    <tr>
                      <th>Fornecedor</th>
                      <th>Descrição</th>
                      <th>Vencimento</th>
                      <th>Valor</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dashboard.proximosPagamentos.map((entry) => (
                      <tr key={entry.key}>
                        <td className="table-title">{entry.nome}</td>
                        <td>{entry.descricao}</td>
                        <td>{formatDateLabel(entry.data)}</td>
                        <td>{currencyFormatter.format(entry.valor)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </article>

            <article className="card table-wrapper dashboard-table">
              <header className="dashboard-table-header">
                <div>
                  <p className="modal-eyebrow">Tickets</p>
                  <h3>Fila crítica</h3>
                </div>
                <span className="muted">{dashboard.ticketsCriticos.length} itens</span>
              </header>
              {dashboard.ticketsCriticos.length === 0 ? (
                <p className="muted">Nenhum ticket pendente.</p>
              ) : (
                <table>
                  <thead>
                    <tr>
                      <th>Ticket</th>
                      <th>Cliente</th>
                      <th>Status</th>
                      <th>Prioridade</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dashboard.ticketsCriticos.map((ticket) => (
                      <tr key={ticket.id}>
                        <td>
                          <p className="table-title">#{ticket.id}</p>
                          <small>{ticket.titulo}</small>
                        </td>
                        <td>{ticket.clienteNome}</td>
                        <td>
                          <span className="badge pendente">{ticketStatusLabels[ticket.status]}</span>
                        </td>
                        <td>
                          <span className={`priority-pill priority-${ticket.prioridade}`}>
                            {priorityLabels[ticket.prioridade]}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </article>
          </section>
        </>
      )}
    </AppLayout>
  );
};

export default DashboardPage;
