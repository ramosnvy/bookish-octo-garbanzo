# Padrão visual (referência para prompts e novas telas)
- **Consistência**: reaproveitar componentes e tokens existentes (`primary`, `ghost` e variantes, modais com cantos 20px, sombras suaves). Não crie novos esquemas de cor se não for necessário.
- **Botões**: usar `primary` para ações principais, `ghost`/`ghost.primary|secondary|success|danger` para secundárias; preferir `icon-only` quando o espaço é curto (editar/lixeira). Altura mínima 40px/42px conforme classes.
- **Modais**: usar `.modal`/`.modal-overlay` padrões; manter largura máx. 640–1100px conforme breakpoints; bloquear rolagem do body com classe `modal-open`.
- **Formulários**: campos com cantos arredondados, espaçamento 0.65–0.9rem; manter hierarquia de seções (`form-section`, `form-row`, `form-field`) e hints (`input-hint`).
- **Calendários**: células largas com resumo por status (dot colorido, label, contagem, valor); sem listas extensas dentro da célula. Respeitar grid responsiva e classes existentes (`calendar-status-*`).
- **Listas/Tabelas**: usar badges existentes para status; ações com seletor de status + ícones de edição/remoção; manter espaçamentos e sombras atuais.
- **Responsividade**: preservar breakpoints já definidos; evitar larguras fixas que causem overflow; para grids grandes, usar `min-width`/scroll horizontal somente quando necessário.
- **Acessibilidade**: todo ícone de ação deve ter `aria-label`/`title` claro; manter contraste existente; seguir labels visíveis em formulários.
- **Tom geral**: UI clean, amigável, sem introduzir novos degradês ou fontes. Qualquer nova variação deve seguir as cores atuais (azul primário, cinzas suaves, verdes/vermelhos para status).***
