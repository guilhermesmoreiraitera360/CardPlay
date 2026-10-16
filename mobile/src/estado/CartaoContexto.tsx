import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { cartaoApi } from '../api/cartaoApi';
import { ErroApi } from '../api/clienteHttp';
import { obterCartaoIdSalvo, salvarCartaoId } from '../armazenamento/cartaoSelecionado';
import type { Cartao, Compra, Movimentacao } from '../tipos';

type CartaoContextoValor = {
  cartao: Cartao | null;
  movimentacoes: Movimentacao[];
  carregando: boolean;
  erro: string | null;
  solicitar: (nomeTitular: string) => Promise<void>;
  recarregar: (valor: number, descricao?: string) => Promise<void>;
  comprar: (produtoId: string, quantidade: number, precoUnitarioVisto: number) => Promise<Compra>;
  cancelar: (compraId: string) => Promise<Compra>;
  atualizar: () => Promise<void>;
};

function aplicarCancelamentoLocal(
  lista: Movimentacao[],
  compraId: string,
  estorno: Movimentacao,
): Movimentacao[] {
  const marcada = lista.map((item) =>
    item.id === compraId && (item.dataCancelamento == null || item.dataCancelamento === '')
      ? { ...item, dataCancelamento: estorno.dataHora }
      : item,
  );
  if (marcada.some((item) => item.id === estorno.id)) {
    return marcada;
  }
  return [estorno, ...marcada];
}

const CartaoContexto = createContext<CartaoContextoValor | undefined>(undefined);

export function CartaoProvider({ children }: { children: ReactNode }) {
  const [cartao, setCartao] = useState<Cartao | null>(null);
  const [movimentacoes, setMovimentacoes] = useState<Movimentacao[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);

  const carregarPorId = useCallback(async (id: string) => {
    const [cartaoAtual, historico] = await Promise.all([
      cartaoApi.obter(id),
      cartaoApi.listarMovimentacoes(id),
    ]);
    setCartao(cartaoAtual);
    setMovimentacoes(historico);
  }, []);

  const atualizar = useCallback(async () => {
    if (!cartao) {
      return;
    }

    setErro(null);
    await carregarPorId(cartao.id);
  }, [cartao, carregarPorId]);

  useEffect(() => {
    let ativo = true;

    async function iniciar() {
      try {
        const id = await obterCartaoIdSalvo();
        if (id && ativo) {
          await carregarPorId(id);
        }
      } catch (falha) {
        if (ativo) {
          setErro(falha instanceof ErroApi ? falha.message : 'Não foi possível carregar o cartão.');
        }
      } finally {
        if (ativo) {
          setCarregando(false);
        }
      }
    }

    void iniciar();
    return () => {
      ativo = false;
    };
  }, [carregarPorId]);

  const solicitar = useCallback(async (nomeTitular: string) => {
    setErro(null);
    setCarregando(true);
    try {
      const criado = await cartaoApi.solicitar(nomeTitular);
      await salvarCartaoId(criado.id);
      setCartao(criado);
      setMovimentacoes([]);
    } catch (falha) {
      setErro(falha instanceof ErroApi ? falha.message : 'Não foi possível solicitar o cartão.');
      throw falha;
    } finally {
      setCarregando(false);
    }
  }, []);

  const recarregar = useCallback(async (valor: number, descricao?: string) => {
    if (!cartao) {
      throw new Error('Solicite um cartão antes de recarregar.');
    }

    setErro(null);
    const atualizado = await cartaoApi.recarregar(cartao.id, valor, descricao);
    const historico = await cartaoApi.listarMovimentacoes(cartao.id);
    setCartao(atualizado);
    setMovimentacoes(historico);
  }, [cartao]);

  const comprar = useCallback(async (produtoId: string, quantidade: number, precoUnitarioVisto: number) => {
    if (!cartao) {
      throw new Error('Solicite um cartão antes de comprar.');
    }

    setErro(null);
    const compra = await cartaoApi.comprar(cartao.id, produtoId, quantidade, precoUnitarioVisto);
    const historico = await cartaoApi.listarMovimentacoes(cartao.id);
    setCartao({ ...cartao, saldo: compra.saldo });
    setMovimentacoes(historico);
    return compra;
  }, [cartao]);

  const cancelar = useCallback(async (compraId: string) => {
    if (!cartao) {
      throw new Error('Solicite um cartão antes de cancelar.');
    }

    setErro(null);
    const cancelamento = await cartaoApi.cancelar(cartao.id, compraId);
    setCartao({ ...cartao, saldo: cancelamento.saldo });
    setMovimentacoes((atuais) => aplicarCancelamentoLocal(atuais, compraId, cancelamento.registro));
    try {
      const historico = await cartaoApi.listarMovimentacoes(cartao.id);
      setMovimentacoes(historico);
    } catch (falha) {
      const mensagem = falha instanceof ErroApi
        ? `O cancelamento foi registrado. ${falha.message}`
        : 'O cancelamento foi registrado. Não foi possível atualizar a lista.';
      setErro(mensagem);
      throw new Error(mensagem);
    }
    return cancelamento;
  }, [cartao]);

  const valor = useMemo(
    () => ({ cartao, movimentacoes, carregando, erro, solicitar, recarregar, comprar, cancelar, atualizar }),
    [cartao, movimentacoes, carregando, erro, solicitar, recarregar, comprar, cancelar, atualizar],
  );

  return <CartaoContexto.Provider value={valor}>{children}</CartaoContexto.Provider>;
}

export function useCartao(): CartaoContextoValor {
  const contexto = useContext(CartaoContexto);
  if (!contexto) {
    throw new Error('useCartao deve ser usado dentro de CartaoProvider.');
  }

  return contexto;
}
