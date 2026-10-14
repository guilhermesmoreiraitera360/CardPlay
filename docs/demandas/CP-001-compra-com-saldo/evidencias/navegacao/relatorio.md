# CP-001 — Navegação da compra com saldo

Rodada executada em 2026-10-08 no navegador integrado do Cursor. A decisão de aceitar a entrega continua humana.

## Ambiente

| Campo | Valor |
| --- | --- |
| Início | 2026-10-08T10:12-03:00 |
| Fim | 2026-10-08T10:20-03:00 |
| Máquina | guilherme-Latitude-3420 |
| Branch | `docs/cp-001-compra-com-saldo` |
| Commit | `4a199f2` |
| Aplicativo | `http://localhost:8081` |
| API | `http://127.0.0.1:5080` |
| Navegador | `cursor-ide-browser`, aba visível |

## Resultado

| Total | Quantidade |
| --- | --- |
| Planejado | 8 |
| Executado | 8 |
| Passou | 8 |
| Falhou | 0 |
| Bloqueado | 0 |
| Não executado | 0 |

## Cartões criados

| Código | Id | Saldo ao fim | Movimentações |
| --- | --- | --- | --- |
| CP-GK2U8Y | `e768cba9-032d-4e64-b597-1a7ec3a838ce` | 1,5 | Recarga 10,00; compra Café especial 8,50, quantidade 1 |
| CP-RZ3TQ2 | `653199a3-3f5a-4100-9d97-9fca85507cff` | 0,0 | Recarga 4,00; compra Adesivo CardPlay 4,00, quantidade 1 |

O segundo cartão exigiu remover só a chave `@cardplay/cartaoId` do armazenamento do navegador. A interface não oferece troca de cartão. O primeiro cartão permaneceu na API.

## Cenários

### NAV-01 — passou

Sem cartão salvo, o Catálogo mostrou “Usar cartão” visível e “Solicite um cartão” em cada produto. Os botões estavam desabilitados. O subtítulo dizia “Solicite um cartão na primeira aba para comprar.”

### NAV-02 — passou

Cartão CP-GK2U8Y, titular Titular Navegação. Recarga de R$ 10,00. O catálogo mostrou saldo R$ 10,00, Café especial e R$ 8,50. O histórico mostrou “Recarga” e “+ R$ 10,00”, em 08/10/2026, 13:13.

Captura: `nav-02-historico-recarga.png`.

### NAV-03 — passou

O aviso foi “Confirmar a compra de Café especial por R$ 8,50?”. Cancelar manteve o saldo em R$ 10,00. O histórico continuou só com a recarga. Entre o cancelamento e a compra seguinte, a lista de rede não teve `POST /compras`.

Captura: `nav-03-historico-sem-compra.png`.

### NAV-04 — passou

A confirmação do café permaneceu no catálogo. Mensagem: “Compra registrada. Saldo: R$ 1,50”. A aba Cartão mostrou R$ 1,50. O histórico mostrou “Café especial”, “Quantidade 1”, “R$ 8,50” sem “+”, em 08/10/2026, 13:15, e a recarga com “+ R$ 10,00”.

`GET` posterior: saldo 1,5; compra com `nomeProduto` “Café especial”, quantidade 1, valor 8,5; recarga 10,0.

Capturas: `nav-04-catalogo-apos-cafe.png`, `nav-04-cartao-saldo-150.png`, `nav-04-historico-cafe.png`.

### NAV-05 — passou

Cartão, Recarga, Histórico e Catálogo mostraram saldo R$ 1,50. A URL do documento permaneceu `http://localhost:8081/`; o título da aba e o destino do link da barra mudaram (`/Cartao`, `/Recarga`, `/Movimentacoes`, `/Catalogo`). Voltar e avançar do navegador ficam não aplicáveis.

Depois de recarregar a página, CP-GK2U8Y, saldo R$ 1,50 e as mesmas duas movimentações continuaram visíveis.

Capturas: `nav-05-recarga-saldo.png`, `nav-05-cartao-apos-reload.png`, `nav-05-historico-apos-reload.png`.

### NAV-06 — passou

Confirmar Adesivo CardPlay a R$ 4,00 com saldo R$ 1,50 mostrou “Saldo insuficiente para concluir a compra.” O saldo permaneceu R$ 1,50. O histórico não ganhou o adesivo. `POST /api/cartoes/e768cba9-032d-4e64-b597-1a7ec3a838ce/compras` respondeu 400.

Capturas: `nav-06-saldo-insuficiente.png`, `nav-06-historico-sem-adesivo.png`.

### NAV-07 — passou

Cartão CP-RZ3TQ2. Recarga digitada de R$ 4,00. Confirmação do adesivo de R$ 4,00. Catálogo e cartão em R$ 0,00, com “Compra registrada. Saldo: R$ 0,00”. Histórico: “Adesivo CardPlay”, “Quantidade 1”, “R$ 4,00” sem “+”, em 08/10/2026, 13:18; recarga com “+ R$ 4,00”.

`GET` posterior: saldo 0,0; compra do adesivo, quantidade 1, valor 4,0; recarga 4,0.

Capturas: `nav-07-segundo-cartao.png`, `nav-07-recarga-4.png`, `nav-07-catalogo-apos-adesivo.png`, `nav-07-cartao-zero.png`, `nav-07-historico-adesivo.png`.

### NAV-08 — passou

A primeira tentativa, com a largura vinda da janela, manteve duas colunas em 640 px e em 390 px. A grade passou a usar a largura medida do próprio catálogo.

Nova execução, sem compra nova, no cartão CP-RZ3TQ2, saldo R$ 0,00, botão “Usar cartão” habilitado:

- 640×900: um produto por linha, ícone ao lado do texto.
- 960×900: duas colunas, ícone acima do nome.

Capturas da nova execução: `nav-08-abaixo-720.png`, `nav-08-largo.png`. As imagens `nav-08-largura-390.png` e `nav-08-390-apos-reload.png` são da tentativa anterior.

## Fora desta rodada

ACE-07, ACE-08, alerta nativo do celular e `dotnet test` não faziam parte de NAV-01 a NAV-08.
