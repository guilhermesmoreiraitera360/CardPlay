export type Cartao = {
  id: string;
  nomeTitular: string;
  codigoAmigavel: string;
  saldo: number;
  dataCriacao: string;
};

export type Movimentacao = {
  id: string;
  valor: number;
  descricao: string;
  dataHora: string;
  produtoId?: string | null;
  nomeProduto?: string | null;
  quantidade?: number | null;
};

export type Compra = {
  cartaoId: string;
  saldo: number;
  registro: Movimentacao;
};

export type Produto = {
  id: string;
  nome: string;
  descricaoCurta: string;
  preco: number;
  icone: string;
  disponivel: boolean;
};
