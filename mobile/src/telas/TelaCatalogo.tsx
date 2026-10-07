import { useEffect, useState } from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, useWindowDimensions, View } from 'react-native';
import { produtoApi } from '../api/produtoApi';
import { CardProduto } from '../componentes/CardProduto';
import { useCartao } from '../estado/CartaoContexto';
import { cores } from '../tema/cores';
import { usePaddingTela } from '../tema/usePaddingTela';
import { formatarMoeda } from '../util/formatacao';
import type { Produto } from '../tipos';

const LARGURA_DUAS_COLUNAS = 720;

export function TelaCatalogo() {
  const [produtos, setProdutos] = useState<Produto[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);
  const [mensagem, setMensagem] = useState<string | null>(null);
  const [erroCompra, setErroCompra] = useState<string | null>(null);
  const [enviandoId, setEnviandoId] = useState<string | null>(null);
  const { cartao, comprar } = useCartao();
  const { width } = useWindowDimensions();
  const paddingTela = usePaddingTela(18);
  const duasColunas = width >= LARGURA_DUAS_COLUNAS;

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      try {
        const lista = await produtoApi.listar();
        if (ativo) {
          setProdutos(lista);
        }
      } catch (falha) {
        if (ativo) {
          setErro(falha instanceof Error ? falha.message : 'Não foi possível carregar o catálogo.');
        }
      } finally {
        if (ativo) {
          setCarregando(false);
        }
      }
    }

    void carregar();
    return () => {
      ativo = false;
    };
  }, []);

  async function aoConfirmar(produto: Produto) {
    setEnviandoId(produto.id);
    setMensagem(null);
    setErroCompra(null);
    try {
      const compra = await comprar(produto.id);
      setMensagem(`Compra registrada. Saldo: ${formatarMoeda(compra.saldo)}`);
    } catch (falha) {
      setErroCompra(falha instanceof Error ? falha.message : 'Não foi possível concluir a compra.');
    } finally {
      setEnviandoId(null);
    }
  }

  return (
    <ScrollView contentContainerStyle={[estilos.conteudo, paddingTela]}>
      <Text style={estilos.titulo}>Catálogo</Text>
      <Text style={estilos.subtitulo}>
        {cartao
          ? 'O preço de cada produto é o valor debitado do saldo ao confirmar.'
          : 'Solicite um cartão na primeira aba para comprar.'}
      </Text>
      {cartao ? <Text style={estilos.saldo}>Saldo atual: {formatarMoeda(cartao.saldo)}</Text> : null}

      {carregando ? <ActivityIndicator color={cores.azul} /> : null}
      {erro ? <Text style={estilos.erro}>{erro}</Text> : null}
      {erroCompra ? <Text style={estilos.erro}>{erroCompra}</Text> : null}
      {mensagem ? <Text style={estilos.mensagem}>{mensagem}</Text> : null}

      <View style={duasColunas ? estilos.grade : estilos.lista}>
        {produtos.map((produto) => (
          <View key={produto.id} style={duasColunas ? estilos.itemGrade : estilos.itemLista}>
            <CardProduto
              produto={produto}
              layout={duasColunas ? 'grade' : 'lista'}
              podeConfirmar={cartao !== null}
              enviando={enviandoId === produto.id}
              bloqueado={enviandoId !== null && enviandoId !== produto.id}
              onConfirmar={(escolhido) => void aoConfirmar(escolhido)}
            />
          </View>
        ))}
      </View>
    </ScrollView>
  );
}

const estilos = StyleSheet.create({
  conteudo: {
    flexGrow: 1,
    backgroundColor: cores.fundo,
  },
  titulo: {
    fontSize: 32,
    fontWeight: '800',
    color: cores.tinta,
    marginHorizontal: 6,
  },
  subtitulo: {
    marginTop: 8,
    marginBottom: 20,
    marginHorizontal: 6,
    color: cores.tintaSuave,
    fontSize: 15,
    lineHeight: 22,
  },
  saldo: {
    marginBottom: 16,
    marginHorizontal: 6,
    color: cores.laranja,
    fontWeight: '700',
    fontSize: 16,
  },
  erro: {
    color: cores.coral,
    marginHorizontal: 6,
    marginBottom: 12,
  },
  mensagem: {
    color: cores.tintaSuave,
    marginHorizontal: 6,
    marginBottom: 12,
  },
  grade: {
    flexDirection: 'row',
    flexWrap: 'wrap',
  },
  lista: {
    flexDirection: 'column',
  },
  itemGrade: {
    width: '50%',
  },
  itemLista: {
    width: '100%',
  },
});
