import { useEffect, useState } from 'react';
import { Alert, Modal, Platform, Pressable, StyleSheet, Text, View } from 'react-native';
import { cores } from '../tema/cores';

type BotaoAlerta = {
  text?: string;
  onPress?: () => void;
  style?: string;
};

type PedidoAlerta = {
  titulo: string;
  mensagem: string;
  botoes: BotaoAlerta[];
};

export function AlertaWeb() {
  const [pedido, setPedido] = useState<PedidoAlerta | null>(null);

  useEffect(() => {
    if (Platform.OS !== 'web') {
      return;
    }

    const alertaDaPlataforma = Alert.alert.bind(Alert);
    Alert.alert = (titulo, mensagem, botoes) => {
      const lista = botoes && botoes.length > 0 ? [...botoes] : [{ text: 'OK' }];
      setPedido({
        titulo: titulo ?? '',
        mensagem: mensagem ?? '',
        botoes: lista,
      });
    };

    return () => {
      Alert.alert = alertaDaPlataforma;
    };
  }, []);

  function fechar(botao?: BotaoAlerta) {
    setPedido(null);
    botao?.onPress?.();
  }

  return (
    <Modal visible={pedido !== null} transparent animationType="fade" onRequestClose={() => fechar()}>
      <View style={estilos.fundo}>
        <View style={estilos.velo} />
        <View style={estilos.caixa}>
          <Text style={estilos.titulo}>{pedido?.titulo}</Text>
          {pedido?.mensagem ? <Text style={estilos.mensagem}>{pedido.mensagem}</Text> : null}
          <View style={estilos.acoes}>
            {pedido?.botoes.map((botao) => {
              const cancelar = botao.style === 'cancel';
              return (
                <Pressable
                  key={botao.text ?? 'ok'}
                  onPress={() => fechar(botao)}
                  style={[estilos.botao, cancelar ? estilos.botaoCancelar : estilos.botaoConfirmar]}
                >
                  <Text style={cancelar ? estilos.textoCancelar : estilos.textoConfirmar}>{botao.text}</Text>
                </Pressable>
              );
            })}
          </View>
        </View>
      </View>
    </Modal>
  );
}

const estilos = StyleSheet.create({
  fundo: {
    flex: 1,
    justifyContent: 'center',
    padding: 24,
  },
  velo: {
    ...StyleSheet.absoluteFill,
    backgroundColor: cores.tinta,
    opacity: 0.45,
  },
  caixa: {
    backgroundColor: cores.superficie,
    borderRadius: 20,
    padding: 20,
  },
  titulo: {
    color: cores.tinta,
    fontSize: 18,
    fontWeight: '800',
  },
  mensagem: {
    marginTop: 8,
    color: cores.tintaSuave,
    fontSize: 15,
    lineHeight: 22,
  },
  acoes: {
    marginTop: 20,
    flexDirection: 'row',
    justifyContent: 'flex-end',
    gap: 8,
  },
  botao: {
    borderRadius: 14,
    paddingVertical: 10,
    paddingHorizontal: 16,
  },
  botaoCancelar: {
    backgroundColor: cores.azulSuave,
  },
  botaoConfirmar: {
    backgroundColor: cores.laranja,
  },
  textoCancelar: {
    color: cores.azul,
    fontWeight: '700',
  },
  textoConfirmar: {
    color: cores.noDestaque,
    fontWeight: '700',
  },
});
