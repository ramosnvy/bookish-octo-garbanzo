import React from "react";
import { ContaStatus } from "../models";
import "../styles.css";

export type CalendarDayStatus = "none" | "to_receive" | "received" | "late";

export interface CalendarDay {
  date: string; // ISO string
  labelDay: number;
  status: CalendarDayStatus;
  totalAmount?: number;
  installmentsCount?: number;
  isToday?: boolean;
  isCurrentMonth?: boolean;
  statusBreakdown?: Array<{
    status: CalendarDayStatus;
    count: number;
    amount: number;
  }>;
}

interface MobileCalendarViewProps {
  monthLabel: string;
  summaryLabel: string;
  days: CalendarDay[]; // 42 items (6 weeks)
  onPrevMonth?: () => void;
  onNextMonth?: () => void;
  onDayClick?: (day: CalendarDay) => void;
}

const statusLabels: Record<CalendarDayStatus, string> = {
  none: "Sem parcelas",
  to_receive: "A receber",
  received: "Recebido",
  late: "Atrasado",
};

export const MobileCalendarView: React.FC<MobileCalendarViewProps> = ({
  monthLabel,
  summaryLabel,
  days,
  onPrevMonth,
  onNextMonth,
  onDayClick,
}) => {
  return (
    <section className="mc-card">
      <header className="mc-header">
        <div className="mc-month-nav">
          <button className="mc-icon-btn" onClick={onPrevMonth} aria-label="Mês anterior">
            ‹
          </button>
          <div className="mc-month-text">
            <span className="mc-month-label">{monthLabel}</span>
            <span className="mc-summary">{summaryLabel}</span>
          </div>
          <button className="mc-icon-btn" onClick={onNextMonth} aria-label="Próximo mês">
            ›
          </button>
        </div>
      </header>

      <div className="mc-weekdays">
        {["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"].map((d) => (
          <span key={d}>{d}</span>
        ))}
      </div>

      <div className="mc-grid">
        {days.map((day) => (
          <CalendarDayCell key={day.date} day={day} onClick={() => onDayClick?.(day)} />
        ))}
      </div>
    </section>
  );
};

interface CalendarDayCellProps {
  day: CalendarDay;
  onClick?: () => void;
}

const CalendarDayCell: React.FC<CalendarDayCellProps> = ({ day, onClick }) => {
  const hasData = day.status !== "none";
  const breakdown = day.statusBreakdown ?? [];

  return (
    <button
      className={[
        "mc-day",
        day.isToday ? "mc-today" : "",
        !day.isCurrentMonth ? "mc-faded" : "",
        hasData ? "mc-has-data" : "",
      ].join(" ")}
      onClick={onClick}
      aria-label={`Dia ${day.labelDay}, ${statusLabels[day.status]}${
        day.totalAmount ? `, valor ${day.totalAmount}` : ""
      }`}
    >
      <div className="mc-day-number">{day.labelDay}</div>
      <div className="mc-day-content">
        {hasData ? (
          <>
            <span className={`mc-badge mc-${day.status}`}>{statusLabels[day.status]}</span>
            <div className="mc-day-values">
              <strong>{day.installmentsCount ?? 0}x</strong>
              {day.totalAmount !== undefined && <span>{formatCurrency(day.totalAmount)}</span>}
            </div>
            {breakdown.length > 1 && (
              <div className="mc-status-breakdown">
                {breakdown.map((item) => (
                  <div key={`${day.date}-${item.status}`} className="mc-status-breakdown-row">
                    <span className={`mc-dot mc-${item.status}`} />
                    <span className="mc-status-text">
                      {statusLabels[item.status]} — {item.count}x
                    </span>
                    <span className="mc-status-amount">{formatCurrency(item.amount)}</span>
                  </div>
                ))}
              </div>
            )}
          </>
        ) : (
          <span className="mc-empty">Sem parcelas</span>
        )}
      </div>
    </button>
  );
};

const formatCurrency = (value: number) =>
  value.toLocaleString("pt-BR", { style: "currency", currency: "BRL", minimumFractionDigits: 2 });
