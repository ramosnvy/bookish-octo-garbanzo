export const getApiErrorMessage = (error: any, fallback: string): string => {
  const payload = error?.response?.data;
  if (!payload) {
    return fallback;
  }

  if (typeof payload === "string") {
    return payload;
  }

  return payload.details ?? payload.message ?? fallback;
};

