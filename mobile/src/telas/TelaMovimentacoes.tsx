import { useState } from 'react';
import { Alert, Modal, Platform, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useCartao } from '../estado/CartaoContexto';
import { cores } from '../tema/cores';
import { usePaddingTela } from '../tema/usePaddingTela';
import { formatarDataHora, formatarMoeda } from '../util/formatacao';
import type { Movimentacao } from '../tipos';

function ehCompra(movimentacao: Movimentacao): boolean {
  return movimentacao.nomeProduto != null && movimentacao.nomeProduto.length > 0;
}

function ehEstorno(movimentacao: Movimentacao): boolean {
  return movimentacao.compraId != null && movimentacao.compraId.length > 0;
}

function nomeVinculado(movimentacao: Movimentacao, lista: Movimentacao[]): string | null {
  const compra = lista.find((item) => item.id === movimentacao.compraId);
  return compra?.nomeProduto ?? null;
}

function mensagemDeCancelamento(compra: Movimentacao): string {
  const unitario = compra.precoUnitario == null ? '' : `, preço unitário ${formatarMoeda(compra.precoUnitario)}`;
  return `Confirmar o cancelamento de ${compra.nomeProduto}, quantidade ${compra.quantidade}${unitario}, total pago ${formatarMoeda(compra.valor)}, estorno ${formatarMoeda(compra.valor)}?`;
}

export function TelaMovimentacoes() {
  const { cartao, movimentacoes, cancelar } = useCartao();
  const paddingTela = usePaddingTela();
  const [compraEmConfirmacao, setCompraEmConfirmacao] = useState<Movimentacao | null>(null);
  const [enviandoId, setEnviandoId] = useState<string | null>(null);
  const [erroCancelamento, setErroCancelamento] = useState<string | null>(null);

  async function confirmarCancelamento(compra: Movimentacao) {
    setEnviandoId(compra.id);
    setErroCancelamento(null);
    try {
      await cancelar(compra.id);
    } catch (falha) {
      setErroCancelamento(falha instanceof Error ? falha.message : 'Não foi possível cancelar a compra.');
    } finally {
      setEnviandoId(null);
    }
  }

  function pedirConfirmacao(compra: Movimentacao) {
    if (Platform.OS === 'web') {
      setCompraEmConfirmacao(compra);
      return;
    }

    Alert.alert('Cancelar compra', mensagemDeCancelamento(compra), [
      { text: 'Cancelar', style: 'cancel' },
      { text: 'Confirmar', onPress: () => void confirmarCancelamento(compra) },
    ]);
  }

  if (!cartao) {
    return (
      <View style={[estilos.conteudo, paddingTela]}>
        <Text style={estilos.titulo}>Movimentações</Text>
        <Text style={estilos.subtitulo}>Quando houver um cartão, o histórico aparecerá aqui.</Text>
      </View>
    );
  }

  return (
    <View style={estilos.flex}>
      <ScrollView contentContainerStyle={[estilos.conteudo, paddingTela]}>
        <Text style={estilos.titulo}>Movimentações</Text>
        <Text style={estilos.subtitulo}>Histórico deste cartão.</Text>
        {erroCancelamento ? <Text style={estilos.erro}>{erroCancelamento}</Text> : null}

        {movimentacoes.length === 0 ? (
          <View style={estilos.vazio}>
            <Text style={estilos.vazioTexto}>Nenhuma movimentação ainda.</Text>
          </View>
        ) : (
          movimentacoes.map((movimentacao) => {
            const compra = ehCompra(movimentacao);
            const estorno = ehEstorno(movimentacao);
            const nomeEstorno = estorno ? nomeVinculado(movimentacao, movimentacoes) : null;
            const podeCancelar = compra && movimentacao.dataCancelamento == null;
            return (
              <View key={movimentacao.id} style={estilos.item}>
                <View style={estilos.itemTopo}>
                  <Text style={estilos.descricao}>
                    {compra ? movimentacao.nomeProduto : nomeEstorno ?? movimentacao.descricao}
                  </Text>
                  <Text style={estilos.valor}>
                    {compra
                      ? formatarMoeda(movimentacao.valor)
                      : `+ ${formatarMoeda(movimentacao.valor)}`}
                  </Text>
                </View>
                {estorno ? <Text style={estilos.quantidade}>Estorno</Text> : null}
                {compra ? (
                  <Text style={estilos.quantidade}>Quantidade {movimentacao.quantidade}</Text>
                ) : null}
                {compra && movimentacao.precoUnitario != null ? (
                  <Text style={estilos.quantidade}>
                    Preço unitário {formatarMoeda(movimentacao.precoUnitario)}
                  </Text>
                ) : null}
                {compra && movimentacao.dataCancelamento != null ? (
                  <Text style={estilos.quantidade}>Cancelada</Text>
                ) : null}
                <Text style={estilos.data}>{formatarDataHora(movimentacao.dataHora)}</Text>
                {podeCancelar ? (
                  <Pressable
                    accessibilityLabel={`Cancelar compra de ${movimentacao.nomeProduto}`}
                    disabled={enviandoId === movimentacao.id}
                    onPress={() => pedirConfirmacao(movimentacao)}
                    style={[estilos.botao, enviandoId !== movimentacao.id && estilos.botaoHabilitado]}
                  >
                    <Text style={estilos.botaoTexto}>
                      {enviandoId === movimentacao.id ? 'Cancelando...' : 'Cancelar compra'}
                    </Text>
                  </Pressable>
                ) : null}
              </View>
            );
          })
        )}
      </ScrollView>
      <Modal
        visible={compraEmConfirmacao !== null}
        transparent
        animationType="none"
        onRequestClose={() => setCompraEmConfirmacao(null)}
      >
        <View style={estilos.alertaFundo}>
          <View style={estilos.alertaCortina} />
          <View style={estilos.alerta}>
            <Text style={estilos.alertaTitulo}>Cancelar compra</Text>
            <Text style={estilos.alertaMensagem}>
              {compraEmConfirmacao ? mensagemDeCancelamento(compraEmConfirmacao) : ''}
            </Text>
            <View style={estilos.alertaAcoes}>
              <Pressable onPress={() => setCompraEmConfirmacao(null)} style={estilos.alertaCancelar}>
                <Text style={estilos.alertaCancelarTexto}>Cancelar</Text>
              </Pressable>
              <Pressable
                onPress={() => {
                  const compra = compraEmConfirmacao;
                  setCompraEmConfirmacao(null);
                  if (compra) {
                    void confirmarCancelamento(compra);
                  }
                }}
                style={estilos.alertaConfirmar}
              >
                <Text style={estilos.alertaConfirmarTexto}>Confirmar</Text>
              </Pressable>
            </View>
          </View>
        </View>
      </Modal>
    </View>
  );
}

const estilos = StyleSheet.create({
  flex: { flex: 1, backgroundColor: cores.fundo },
  conteudo: {
    flexGrow: 1,
    backgroundColor: cores.fundo,
  },
  titulo: {
    fontSize: 32,
    fontWeight: '800',
    color: cores.tinta,
  },
  subtitulo: {
    marginTop: 8,
    marginBottom: 24,
    color: cores.tintaSuave,
    fontSize: 15,
    lineHeight: 22,
  },
  erro: {
    marginTop: -12,
    marginBottom: 16,
    color: cores.coral,
    fontSize: 14,
  },
  vazio: {
    backgroundColor: cores.superficie,
    borderRadius: 20,
    padding: 20,
    borderWidth: 1,
    borderColor: cores.linha,
  },
  vazioTexto: {
    color: cores.tintaSuave,
  },
  item: {
    backgroundColor: cores.superficie,
    borderRadius: 20,
    padding: 16,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: cores.linha,
  },
  itemTopo: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    gap: 12,
  },
  descricao: {
    flex: 1,
    color: cores.tinta,
    fontWeight: '600',
    fontSize: 16,
  },
  valor: {
    color: cores.laranja,
    fontWeight: '700',
  },
  quantidade: {
    marginTop: 8,
    color: cores.tintaSuave,
    fontSize: 13,
  },
  data: {
    marginTop: 8,
    color: cores.tintaSuave,
    fontSize: 13,
  },
  botao: {
    marginTop: 12,
    borderRadius: 14,
    paddingVertical: 10,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: cores.laranja,
  },
  botaoHabilitado: {
    backgroundColor: cores.laranja,
  },
  botaoTexto: {
    color: cores.noDestaque,
    fontWeight: '700',
  },
  alertaFundo: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    padding: 24,
  },
  alertaCortina: {
    ...StyleSheet.absoluteFill,
    backgroundColor: cores.tinta,
    opacity: 0.45,
  },
  alerta: {
    width: '100%',
    maxWidth: 360,
    backgroundColor: cores.superficie,
    borderRadius: 20,
    padding: 20,
  },
  alertaTitulo: {
    fontSize: 18,
    fontWeight: '800',
    color: cores.tinta,
  },
  alertaMensagem: {
    marginTop: 8,
    color: cores.tintaSuave,
    fontSize: 15,
    lineHeight: 22,
  },
  alertaAcoes: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    marginTop: 18,
    gap: 8,
  },
  alertaCancelar: {
    paddingVertical: 10,
    paddingHorizontal: 14,
  },
  alertaCancelarTexto: {
    color: cores.tintaSuave,
    fontWeight: '700',
  },
  alertaConfirmar: {
    backgroundColor: cores.laranja,
    borderRadius: 14,
    paddingVertical: 10,
    paddingHorizontal: 14,
  },
  alertaConfirmarTexto: {
    color: cores.noDestaque,
    fontWeight: '700',
  },
});
