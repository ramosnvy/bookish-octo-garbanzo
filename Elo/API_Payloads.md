# Documentação Básica de Endpoints e Payloads

Abaixo estão os modelos de payload JSON e endpoints disponíveis para Assinaturas, Usuários e Afiliados.

---

## 1. Assinatura

**Endpoints Disponíveis:**
- `POST /api/assinaturas` (Criação)
- `GET /api/assinaturas` (Listagem)

### Criar Assinatura (`POST`)

**Payload:**

```json
{
  "clienteId": 10,
  "isRecorrente": true,
  "intervaloDias": 30,
  "dataInicio": "2024-01-01T00:00:00",
  "dataFim": null,
  "gerarFinanceiro": true,
  "gerarImplantacao": true,
  "itens": [
    {
      "produtoId": 1,
      "produtoModuloId": 5
    },
    {
      "produtoId": 1,
      "produtoModuloId": null
    }
  ]
}
```

*Nota: Atualmente, apenas a Criação e Listagem de assinaturas estão implementadas.*

---

## 2. Usuário

**Endpoints Disponíveis:**
- `GET /api/users` (Listagem)
- `GET /api/users/{id}` (Obter por ID)
- `POST /api/users` (Criação)
- `PUT /api/users/{id}` (Atualização)
- `DELETE /api/users/{id}` (Remoção)

### Criar Usuário (`POST`)

**Payload:**

```json
{
  "nome": "Maria Souza",
  "email": "maria.souza@empresa.com",
  "password": "SenhaForte@123",
  "role": "Gerente",
  "empresaId": 1
}
```

### Atualizar Usuário (`PUT`)

**Payload:**

```json
{
  "nome": "Maria Souza Alterada",
  "email": "maria.souza@empresa.com",
  "role": "Admin",
  "empresaId": 1
}
```

*Nota: O ID do usuário é passado na URL.*

---

## 3. Afiliado

**Endpoints Disponíveis:**
- `GET /api/afiliados` (Listagem)
- `GET /api/afiliados/{id}` (Obter por ID)
- `POST /api/afiliados` (Criação)
- `PUT /api/afiliados/{id}` (Atualização)
- `DELETE /api/afiliados/{id}` (Remoção)

### Criar Afiliado (`POST`)

**Payload:**

```json
{
  "nome": "Parceiro Comercial Ltda",
  "email": "contato@parceiro.com",
  "documento": "12.345.678/0001-90",
  "telefone": "(11) 98765-4321",
  "porcentagem": 15.0,
  "status": 1
}
```

### Atualizar Afiliado (`PUT`)

**Payload:**

```json
{
  "nome": "Parceiro Comercial S/A",
  "email": "novo@parceiro.com",
  "documento": "12.345.678/0001-90",
  "telefone": "(11) 98765-4321",
  "porcentagem": 20.0,
  "status": 1
}
```

*Nota: `status` é um Enum (0 = Inativo, 1 = Ativo). O ID é passado na URL.*
