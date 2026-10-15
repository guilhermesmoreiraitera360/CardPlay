import { useState } from 'react';
import { Alert, Modal, Platform, Pressable, StyleSheet, Text, View } from 'react-native';
import type { Produto } from '../tipos';
import { cores } from '../tema/cores';
import { formatarMoeda } from '../util/formatacao';

type Propriedades = {
  produto: Produto;
  layout?: 'grade' | 'lista';
  podeConfirmar?: boolean;
  enviando?: boolean;
  bloqueado?: boolean;
  onConfirmar?: (produto: Produto, quantidade: number) => void;
};

export function CardProduto({
  produto,
  layout = 'grade',
  podeConfirmar = false,
  enviando = false,
  bloqueado = false,
  onConfirmar,
}: Propriedades) {
  const lista = layout === 'lista';
  const habilitado = podeConfirmar && !enviando && !bloqueado && onConfirmar !== undefined;
  const [confirmando, setConfirmando] = useState(false);
  const [quantidade, setQuantidade] = useState(1);
  const total = produto.preco * quantidade;
  const mensagem = `Confirmar a compra de ${produto.nome}, quantidade ${quantidade}, preço unitário ${formatarMoeda(produto.preco)}, total ${formatarMoeda(total)}?`;

  function alterarQuantidade(delta: number) {
    setQuantidade((atual) => Math.max(1, atual + delta));
  }

  function pedirConfirmacao() {
    if (!onConfirmar) {
      return;
    }

    if (Platform.OS === 'web') {
      setConfirmando(true);
      return;
    }

    Alert.alert('Usar cartão', mensagem, [
      { text: 'Cancelar', style: 'cancel' },
      { text: 'Confirmar', onPress: () => onConfirmar(produto, quantidade) },
    ]);
  }

  function cancelar() {
    setConfirmando(false);
  }

  function confirmar() {
    setConfirmando(false);
    onConfirmar?.(produto, quantidade);
  }

  return (
    <View style={[estilos.card, lista && estilos.cardLista]}>
      <Text style={[estilos.icone, lista && estilos.iconeLista]}>{produto.icone}</Text>
      <View style={lista ? estilos.detalhes : undefined}>
        <Text style={estilos.nome}>{produto.nome}</Text>
        <Text style={[estilos.descricao, lista && estilos.descricaoLista]}>{produto.descricaoCurta}</Text>
        <Text style={estilos.preco}>{formatarMoeda(produto.preco)}</Text>
        <View style={estilos.quantidade}>
          <Pressable
            accessibilityLabel={`Diminuir quantidade de ${produto.nome}`}
            disabled={!habilitado || quantidade <= 1}
            onPress={() => alterarQuantidade(-1)}
            style={[estilos.quantidadePasso, (!habilitado || quantidade <= 1) && estilos.quantidadePassoDesabilitado]}
          >
            <Text style={estilos.quantidadePassoTexto}>−</Text>
          </Pressable>
          <Text style={estilos.quantidadeValor} accessibilityLabel={`Quantidade de ${produto.nome}`}>
            {quantidade}
          </Text>
          <Pressable
            accessibilityLabel={`Aumentar quantidade de ${produto.nome}`}
            disabled={!habilitado}
            onPress={() => alterarQuantidade(1)}
            style={[estilos.quantidadePasso, !habilitado && estilos.quantidadePassoDesabilitado]}
          >
            <Text style={estilos.quantidadePassoTexto}>+</Text>
          </Pressable>
        </View>
        <Pressable
          disabled={!habilitado}
          onPress={pedirConfirmacao}
          style={[estilos.botao, habilitado && estilos.botaoHabilitado]}
        >
          <Text style={[estilos.botaoTexto, habilitado && estilos.botaoTextoHabilitado]}>
            {enviando ? 'Comprando...' : 'Usar cartão'}
          </Text>
          {!podeConfirmar && !enviando ? <Text style={estilos.emBreve}>Solicite um cartão</Text> : null}
        </Pressable>
      </View>
      <Modal visible={confirmando} transparent animationType="none" onRequestClose={cancelar}>
        <View style={estilos.alertaFundo}>
          <View style={estilos.alertaCortina} />
          <View style={estilos.alerta}>
            <Text style={estilos.alertaTitulo}>Usar cartão</Text>
            <Text style={estilos.alertaMensagem}>{mensagem}</Text>
            <View style={estilos.alertaAcoes}>
              <Pressable onPress={cancelar} style={estilos.alertaCancelar}>
                <Text style={estilos.alertaCancelarTexto}>Cancelar</Text>
              </Pressable>
              <Pressable onPress={confirmar} style={estilos.alertaConfirmar}>
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
  card: {
    flex: 1,
    backgroundColor: cores.superficie,
    borderRadius: 24,
    padding: 16,
    margin: 6,
    minHeight: 280,
    borderWidth: 1,
    borderColor: cores.linha,
  },
  cardLista: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    minHeight: 0,
    marginHorizontal: 0,
    marginBottom: 12,
  },
  icone: {
    fontSize: 36,
    marginBottom: 12,
  },
  iconeLista: {
    fontSize: 40,
    marginBottom: 0,
    marginRight: 14,
  },
  detalhes: {
    flex: 1,
  },
  nome: {
    fontSize: 16,
    fontWeight: '700',
    color: cores.tinta,
  },
  descricao: {
    marginTop: 8,
    color: cores.tintaSuave,
    fontSize: 13,
    lineHeight: 18,
    minHeight: 54,
  },
  descricaoLista: {
    minHeight: 0,
  },
  preco: {
    marginTop: 8,
    fontSize: 16,
    fontWeight: '700',
    color: cores.laranja,
  },
  quantidade: {
    marginTop: 12,
    flexDirection: 'row',
    alignItems: 'center',
    alignSelf: 'flex-start',
    gap: 8,
  },
  quantidadePasso: {
    width: 36,
    height: 36,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: cores.laranja,
    alignItems: 'center',
    justifyContent: 'center',
  },
  quantidadePassoDesabilitado: {
    opacity: 0.4,
  },
  quantidadePassoTexto: {
    color: cores.laranja,
    fontSize: 20,
    fontWeight: '700',
    lineHeight: 22,
  },
  quantidadeValor: {
    minWidth: 24,
    textAlign: 'center',
    color: cores.laranja,
    fontSize: 16,
    fontWeight: '700',
  },
  botao: {
    marginTop: 14,
    backgroundColor: cores.azulSuave,
    borderRadius: 16,
    paddingVertical: 10,
    alignItems: 'center',
    opacity: 0.85,
  },
  botaoHabilitado: {
    backgroundColor: cores.laranja,
    opacity: 1,
  },
  botaoTexto: {
    color: cores.azul,
    fontWeight: '700',
  },
  botaoTextoHabilitado: {
    color: cores.noDestaque,
  },
  emBreve: {
    color: cores.tintaSuave,
    fontSize: 11,
    marginTop: 2,
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
    zIndex: 0,
  },
  alerta: {
    width: '100%',
    maxWidth: 360,
    backgroundColor: cores.superficie,
    borderRadius: 20,
    padding: 20,
    zIndex: 1,
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
