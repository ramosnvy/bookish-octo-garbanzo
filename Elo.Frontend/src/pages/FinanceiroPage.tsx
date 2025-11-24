import { useState } from "react";
import AppLayout from "../components/AppLayout";
import ContasPagarPage from "./ContasPagarPage";
import ContasReceberPage from "./ContasReceberPage";

const FinanceiroPage = () => {
  const [tab, setTab] = useState<"pagar" | "receber">("pagar");

  return (
    <AppLayout title="Financeiro" subtitle="Gerencie pagamentos e recebimentos">
      <div className="filters-bar">
        <div className="view-toggle">
          <button type="button" className={`ghost ${tab === "pagar" ? "active" : ""}`} onClick={() => setTab("pagar")}>
            Contas a pagar
          </button>
          <button
            type="button"
            className={`ghost ${tab === "receber" ? "active" : ""}`}
            onClick={() => setTab("receber")}
          >
            Contas a receber
          </button>
        </div>
      </div>

      {tab === "pagar" ? <ContasPagarPage embedded /> : <ContasReceberPage embedded />}
    </AppLayout>
  );
};

export default FinanceiroPage;
