import { NavigationContainer } from '@react-navigation/native';
import { StatusBar } from 'expo-status-bar';
import { useEffect } from 'react';
import { Platform, StyleSheet } from 'react-native';
import { SafeAreaProvider, SafeAreaView } from 'react-native-safe-area-context';
import { AlertaWeb } from './src/componentes/AlertaWeb';
import { CartaoProvider } from './src/estado/CartaoContexto';
import { NavegacaoAbas } from './src/navegacao/NavegacaoAbas';
import { cores } from './src/tema/cores';

export default function App() {
  useEffect(() => {
    if (Platform.OS !== 'web' || typeof document === 'undefined') {
      return;
    }

    const meta = document.querySelector('meta[name="viewport"]');
    meta?.setAttribute(
      'content',
      'width=device-width, initial-scale=1, viewport-fit=cover',
    );
  }, []);

  return (
    <SafeAreaProvider>
      <SafeAreaView style={estilos.area} edges={['bottom']}>
        <CartaoProvider>
          <NavigationContainer>
            <StatusBar style="dark" />
            <NavegacaoAbas />
            <AlertaWeb />
          </NavigationContainer>
        </CartaoProvider>
      </SafeAreaView>
    </SafeAreaProvider>
  );
}

const estilos = StyleSheet.create({
  area: {
    flex: 1,
    backgroundColor: cores.superficie,
  },
});
