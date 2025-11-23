export type RecurrencePresetValue = number | "custom";

export const RECURRENCE_OPTIONS: { label: string; value: RecurrencePresetValue }[] = [
  { label: "Semanal (7 dias)", value: 7 },
  { label: "Quinzenal (15 dias)", value: 15 },
  { label: "Mensal (30 dias)", value: 30 },
  { label: "Bimestral (60 dias)", value: 60 },
  { label: "Trimestral (90 dias)", value: 90 },
  { label: "Semestral (180 dias)", value: 180 },
  { label: "Anual (365 dias)", value: 365 },
  { label: "Personalizado", value: "custom" },
];

export const DEFAULT_RECURRENCE_INTERVAL = 30;

export const resolvePresetForInterval = (interval?: number): RecurrencePresetValue => {
  if (!interval) {
    return DEFAULT_RECURRENCE_INTERVAL;
  }
  const match = RECURRENCE_OPTIONS.find(
    (option) => option.value !== "custom" && option.value === interval
  );
  return match ? match.value : "custom";
};

