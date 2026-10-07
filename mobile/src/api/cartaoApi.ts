import { clienteHttp } from './clienteHttp';
import type { Cartao, Compra, Movimentacao } from '../tipos';

export const cartaoApi = {
  solicitar(nomeTitular: string): Promise<Cartao> {
    return clienteHttp.post<Cartao>('/api/cartoes', { nomeTitular });
  },

  obter(id: string): Promise<Cartao> {
    return clienteHttp.get<Cartao>(`/api/cartoes/${id}`);
  },

  recarregar(id: string, valor: number, descricao?: string): Promise<Cartao> {
    return clienteHttp.post<Cartao>(`/api/cartoes/${id}/recargas`, { valor, descricao });
  },

  comprar(id: string, produtoId: string): Promise<Compra> {
    return clienteHttp.post<Compra>(`/api/cartoes/${id}/compras`, { produtoId });
  },

  listarMovimentacoes(id: string): Promise<Movimentacao[]> {
    return clienteHttp.get<Movimentacao[]>(`/api/cartoes/${id}/movimentacoes`);
  },
};
