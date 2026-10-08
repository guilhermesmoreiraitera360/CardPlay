# Produto — comportamento desta versão

CardPlay é uma plataforma lúdica interna. O cartão não representa um cartão bancário e não se conecta a nenhum sistema financeiro.

## O que a pessoa consegue fazer

1. Solicitar um cartão virtual informando o próprio nome.
2. Ver o cartão com nome do titular, código amigável e saldo (inicia em zero).
3. Adicionar saldo com uma recarga simulada, desde que o valor seja maior que zero.
4. Consultar o histórico das recargas e das compras.
5. Percorrer um catálogo pequeno de produtos fictícios já cadastrados.
6. Comprar um produto com o saldo do cartão, depois de ver o nome e o preço e confirmar. A quantidade é uma unidade. O saldo final é o anterior menos o preço do catálogo. Se o saldo for igual ao preço, fica zero. Se for menor, a compra não acontece e o saldo permanece.

## O que esta versão não faz

- Autenticar usuários
- Administrar catálogo
- Controlar estoque ou carrinho
- Integrar banco, adquirente ou gateway
- Gerar dados de cartão de pagamento

Uma recarga sem descrição usa o texto padrão “Recarga”. O aplicativo guarda localmente o cartão selecionado; a API pode ter vários cartões, mas não há login.

## Visual e usabilidade

A interface é lúdica, com bastante espaço e duas cores de destaque: **azul** (cartão, navegação) e **laranja** (saldo, preços, recarga e compra).

A barra inferior permanece visível no celular, com ícone e rótulo acima da área segura. O catálogo lista um produto por linha em telas estreitas e duas colunas em telas largas.

O botão **Usar cartão** fica visível. Com um cartão no aplicativo, ele pede confirmação do nome e do preço. Sem cartão, permanece desabilitado.

Detalhe da paleta e das regras: [ui.md](ui.md).
