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
  onConfirmar?: (produto: Produto) => void;
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
  const mensagem = `Confirmar a compra de ${produto.nome} por ${formatarMoeda(produto.preco)}?`;

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
      { text: 'Confirmar', onPress: () => onConfirmar(produto) },
    ]);
  }

  function cancelar() {
    setConfirmando(false);
  }

  function confirmar() {
    setConfirmando(false);
    onConfirmar?.(produto);
  }

  return (
    <View style={[estilos.card, lista && estilos.cardLista]}>
      <Text style={[estilos.icone, lista && estilos.iconeLista]}>{produto.icone}</Text>
      <View style={lista ? estilos.detalhes : undefined}>
        <Text style={estilos.nome}>{produto.nome}</Text>
        <Text style={[estilos.descricao, lista && estilos.descricaoLista]}>{produto.descricaoCurta}</Text>
        <Text style={estilos.preco}>{formatarMoeda(produto.preco)}</Text>
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
    minHeight: 230,
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
