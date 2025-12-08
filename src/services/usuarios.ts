import { api } from "@/lib/api";
import { UserDto } from "@/types/api";

export const UsuarioService = {
  async getAll(empresaId?: number | null): Promise<UserDto[]> {
    const params = empresaId ? { empresaId } : undefined;
    return api.get<UserDto[]>("/users", { params });
  },
};

