# Exemplos de Uso da API Elo

Este arquivo contém exemplos práticos de como usar a API Elo.

## Configuração Inicial

### 1. Iniciar a API

```bash
cd Elo/Elo
dotnet run
```

A API estará disponível em:
- HTTPS: `https://localhost:7000`
- HTTP: `http://localhost:5000`

### 2. Acessar o Swagger

Abra seu navegador e acesse: `https://localhost:7000/swagger`

## Exemplos de Requisições

### 1. Login (Criar Token JWT)

**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "email": "admin@elo.com",
  "password": "123456"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "nome": "Administrador",
  "email": "admin@elo.com",
  "role": "Admin",
  "expiresAt": "2024-10-26T20:14:00.000Z"
}
```

### 2. Registrar Novo Usuário

**Endpoint:** `POST /api/auth/register`

**Request:**
```json
{
  "nome": "João Silva",
  "email": "joao@elo.com",
  "password": "123456",
  "role": "Employee"
}
```

### 3. Listar Clientes

**Endpoint:** `GET /api/clientes`

**Headers:**
```
Authorization: Bearer SEU_JWT_TOKEN_AQUI
```

**Response:**
```json
[
  {
    "id": 1,
    "nome": "Cliente Exemplo",
    "cnpjCpf": "12345678901",
    "email": "cliente@exemplo.com",
    "telefone": "11999999999",
    "status": "Ativo",
    "dataCadastro": "2024-10-26T17:14:00.000Z",
    "updatedAt": null
  }
]
```

### 4. Criar Cliente

**Endpoint:** `POST /api/clientes`

**Headers:**
```
Authorization: Bearer SEU_JWT_TOKEN_AQUI
Content-Type: application/json
```

**Request:**
```json
{
  "nome": "Empresa ABC Ltda",
  "cnpjCpf": "12345678000195",
  "email": "contato@empresaabc.com",
  "telefone": "1133334444",
  "status": "Ativo"
}
```

**Response:**
```json
{
  "id": 2,
  "nome": "Empresa ABC Ltda",
  "cnpjCpf": "12345678000195",
  "email": "contato@empresaabc.com",
  "telefone": "1133334444",
  "status": "Ativo",
  "dataCadastro": "2024-10-26T17:14:00.000Z",
  "updatedAt": null
}
```

### 5. Obter Cliente por ID

**Endpoint:** `GET /api/clientes/{id}`

**Headers:**
```
Authorization: Bearer SEU_JWT_TOKEN_AQUI
```

**Response:**
```json
{
  "id": 1,
  "nome": "Cliente Exemplo",
  "cnpjCpf": "12345678901",
  "email": "cliente@exemplo.com",
  "telefone": "11999999999",
  "status": "Ativo",
  "dataCadastro": "2024-10-26T17:14:00.000Z",
  "updatedAt": null
}
```

### 6. Atualizar Cliente

**Endpoint:** `PUT /api/clientes/{id}`

**Headers:**
```
Authorization: Bearer SEU_JWT_TOKEN_AQUI
Content-Type: application/json
```

**Request:**
```json
{
  "nome": "Cliente Atualizado",
  "cnpjCpf": "12345678901",
  "email": "novoemail@cliente.com",
  "telefone": "1188887777",
  "status": "Ativo"
}
```

### 7. Deletar Cliente

**Endpoint:** `DELETE /api/clientes/{id}`

**Headers:**
```
Authorization: Bearer SEU_JWT_TOKEN_AQUI
```

**Response:** `204 No Content`

## Exemplos com cURL

### Login
```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@elo.com",
    "password": "123456"
  }'
```

### Criar Cliente
```bash
curl -X POST "https://localhost:7000/api/clientes" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -d '{
    "nome": "Empresa XYZ",
    "cnpjCpf": "98765432000123",
    "email": "contato@empresaxyz.com",
    "telefone": "1155556666",
    "status": "Ativo"
  }'
```

### Listar Clientes
```bash
curl -X GET "https://localhost:7000/api/clientes" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

## Códigos de Status HTTP

- `200 OK` - Sucesso
- `201 Created` - Recurso criado com sucesso
- `204 No Content` - Sucesso sem conteúdo (ex: delete)
- `400 Bad Request` - Dados inválidos
- `401 Unauthorized` - Token JWT inválido ou ausente
- `404 Not Found` - Recurso não encontrado
- `500 Internal Server Error` - Erro interno do servidor

## Usuário Administrador Padrão

A API cria automaticamente um usuário administrador padrão:

- **Email:** admin@elo.com
- **Senha:** 123456
- **Role:** Admin

Use essas credenciais para fazer login e obter o token JWT necessário para acessar os endpoints protegidos.

## Próximos Passos

1. Implementar controllers para as demais entidades (Fornecedores, Produtos, etc.)
2. Adicionar validações mais robustas
3. Implementar paginação nos endpoints de listagem
4. Adicionar filtros e busca
5. Implementar logs de auditoria
